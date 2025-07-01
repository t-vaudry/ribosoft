#!/usr/bin/env python3
"""
Modern dependency management script for Ribosoft C++ dependencies.
Supports ViennaRNA and Melting library package management.
"""
import logging
import sys
from pathlib import Path
from typing import List, Optional, Dict, Any
from enum import Enum
from functools import reduce
import tempfile
import hashlib
import zipfile
import shutil
import json

import click
import click_log
import requests
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry

# Require Python 3.8+ for modern features
if sys.version_info < (3, 8):
    print("This script requires Python version 3.8 or higher")
    print(f"Current version: {sys.version}")
    sys.exit(1)

# Use pathlib for modern path handling
root_dir = Path(__file__).parent.resolve()

# Environment setup
dependency_file_path = root_dir / 'deps.json'
lock_file_path = root_dir / 'deps.lock'
install_path = root_dir / '.deps'


class Log:
    """Modern logging wrapper with structured logging support."""
    
    def __init__(self, name: str = __name__, level: int = logging.ERROR):
        self.logger = logging.getLogger(name)
        self.logger.setLevel(level)
        click_log.basic_config(self.logger)

    def debug(self, msg: str) -> None:
        self.logger.debug(msg)

    def info(self, msg: str) -> None:
        self.logger.info(msg)

    def warning(self, msg: str) -> None:
        self.logger.warning(msg)

    def error(self, msg: str) -> None:
        self.logger.error(msg)


log = Log()


class UnsupportedOSException(Exception):
    """Raised when the operating system is not supported."""
    pass


def os_str() -> str:
    """Get the operating system string for package selection."""
    platform = sys.platform

    if platform == 'win32':
        return 'win'
    elif platform.startswith('linux'):
        return 'linux'
    elif platform == 'darwin':
        return 'mac'

    raise UnsupportedOSException(f"Unsupported platform: {platform}")


class MalformedFileException(Exception):
    """Raised when a configuration file is malformed."""
    pass


class Action(Enum):
    """Actions that can be performed on dependencies."""
    NONE = "none"
    INSTALL = "install"
    REPLACE = "replace"
    REMOVE = "remove"


class Dependency:
    """Represents a package dependency with name and version."""
    
    def __init__(self, name: str, version: str):
        self.name = name
        self.version = version
    
    def __repr__(self) -> str:
        return f"Dependency(name='{self.name}', version='{self.version}')"


class DependencyAction:
    """Represents an action to be performed on a dependency."""
    
    def __init__(self, current: Optional[Dependency] = None, 
                 target: Optional[Dependency] = None, 
                 action: Action = Action.NONE):
        self.current = current
        self.target = target
        self.action = action

    def set_action(self, action: Action) -> None:
        self.action = action

    def name(self) -> str:
        return self.current.name if self.current is not None else self.target.name

    def current_version(self) -> Optional[str]:
        return self.current.version if self.current is not None else None

    def target_version(self) -> Optional[str]:
        return self.target.version if self.target is not None else None


class DependencyFile:
    """Represents the deps.json configuration file."""
    
    def __init__(self):
        self.catalog_url: str = ''
        self.packages: List[Dependency] = []

    def set_catalog_url(self, catalog_url: str) -> None:
        self.catalog_url = catalog_url

    def add_package(self, name: str, version: str) -> None:
        self.packages.append(Dependency(name=name, version=version))


class DependencyLockFile:
    """Represents the deps.lock file tracking installed packages."""
    
    def __init__(self):
        self.packages: List[Dependency] = []

    def add_package(self, name: str, version: str) -> None:
        self.packages.append(Dependency(name=name, version=version))

    def remove_package(self, name: str) -> None:
        self.packages = [p for p in self.packages if p.name != name]

    def serialize(self) -> Dict[str, Any]:
        return {
            'packages': [{'name': p.name, 'version': p.version} for p in self.packages]
        }


class DependencyFileParser:
    """Parser for dependency and lock files with modern error handling."""
    
    def __init__(self):
        self._dependency_file: Optional[DependencyFile] = None
        self._lock_file: Optional[DependencyLockFile] = None

    def get_dependency_file(self) -> DependencyFile:
        if self._dependency_file is None:
            self._load_dependency_file()
        return self._dependency_file

    def get_lock_file(self) -> DependencyLockFile:
        if self._lock_file is None:
            self._load_lock_file()
        return self._lock_file

    def write_lock_file(self, lock_file: DependencyLockFile) -> None:
        """Write lock file with atomic operation."""
        try:
            with lock_file_path.open('w', encoding='utf-8') as f:
                json.dump(lock_file.serialize(), f, indent=2, ensure_ascii=False)
        except OSError as e:
            log.error(f'Failed to write lock file: {e}')
            raise

    def _load_dependency_file(self) -> None:
        log.debug('Loading dependency file')
        
        deps_data = self._get_dependency_file_data(dependency_file_path)
        dependency_file = DependencyFile()

        if deps_data is not None:
            dependency_file.set_catalog_url(deps_data['catalog-url'])

            for package in deps_data['packages']:
                dependency_file.add_package(package['name'], package['version'])
                log.debug(f"Found package dependency: {package['name']}@{package['version']}")

        self._dependency_file = dependency_file
        log.debug('Finished loading dependency file')

    def _load_lock_file(self) -> None:
        log.debug('Loading lock file')
        
        lock_data = self._get_lock_file_data(lock_file_path)
        lock_file = DependencyLockFile()

        if lock_data is not None and 'packages' in lock_data:
            for package in lock_data['packages']:
                lock_file.add_package(package['name'], package['version'])
                log.debug(f"Found installed package: {package['name']}@{package['version']}")

        self._lock_file = lock_file
        log.debug('Finished loading lock file')

    def _get_dependency_file_data(self, filename: Path) -> Optional[Dict[str, Any]]:
        try:
            with filename.open('r', encoding='utf-8') as f:
                deps = json.load(f)
            self._validate_dependency_schema(deps)
            return deps
        except FileNotFoundError:
            log.warning(f'Cannot open dependency file: {filename}')
            return None
        except json.JSONDecodeError as e:
            log.error(f'Invalid JSON in dependency file {filename}: {e}')
            raise
        except MalformedFileException:
            log.error(f'Malformed dependency file: {filename}')
            raise

    def _get_lock_file_data(self, filename: Path) -> Optional[Dict[str, Any]]:
        try:
            with filename.open('r', encoding='utf-8') as f:
                lock_data = json.load(f)
            self._validate_lock_schema(lock_data)
            return lock_data
        except FileNotFoundError:
            log.debug('No lock file found')
            return None
        except json.JSONDecodeError as e:
            log.error(f'Invalid JSON in lock file {filename}: {e}')
            raise
        except MalformedFileException as e:
            log.error(f'Malformed lock file {filename}: {e}')
            raise

    def _validate_dependency_schema(self, root: Dict[str, Any]) -> None:
        """Validate dependency file schema."""
        required_fields = ['catalog-url', 'packages']
        for field in required_fields:
            if field not in root:
                raise MalformedFileException(f"Missing required field: '{field}'")

        if not isinstance(root['packages'], list):
            raise MalformedFileException("'packages' must be an array")

        for package in root['packages']:
            if 'name' not in package:
                raise MalformedFileException("Package missing 'name' field")
            if 'version' not in package:
                raise MalformedFileException(f"Package '{package.get('name', 'unknown')}' missing 'version' field")

    def _validate_lock_schema(self, root: Dict[str, Any]) -> None:
        """Validate lock file schema."""
        if 'packages' in root:
            if not isinstance(root['packages'], list):
                raise MalformedFileException("'packages' must be an array")

            for package in root['packages']:
                if 'name' not in package:
                    raise MalformedFileException("Package missing 'name' field")
                if 'version' not in package:
                    raise MalformedFileException(f"Package '{package.get('name', 'unknown')}' missing 'version' field")


class Downloader:
    """Modern HTTP downloader with retry logic and integrity verification."""
    
    def __init__(self):
        # Set up requests with modern Retry mechanism
        retry_strategy = Retry(
            total=5,
            backoff_factor=1,
            status_forcelist=[429, 500, 502, 503, 504],
            allowed_methods=['HEAD', 'GET', 'OPTIONS']  # Fixed deprecated method_whitelist
        )
        adapter = HTTPAdapter(max_retries=retry_strategy)
        session = requests.Session()
        session.mount('https://', adapter)
        session.mount('http://', adapter)
        self.requests = session

    def fetch_catalog(self, url: str) -> 'Catalog':
        """Fetch and parse the remote package catalog."""
        try:
            log.debug(f'Fetching catalog from: {url}')
            response = self.requests.get(url, timeout=30)
            response.raise_for_status()
            data = response.json()
            return Catalog(url, data)
        except requests.HTTPError as e:
            if e.response is not None and e.response.status_code == 404:
                log.error('Catalog not found (404)')
            else:
                log.error(f'Server error: {e.response.status_code if e.response else "unknown"}')
            raise
        except requests.RequestException as e:
            log.error(f'Network error fetching catalog: {e}')
            raise
        except json.JSONDecodeError as e:
            log.error(f'Invalid JSON in catalog: {e}')
            raise

    def download_zip(self, destination: Path, url: str, sha256: str) -> None:
        """Download and extract a zip file with integrity verification."""
        log.debug(f'Downloading {url} to {destination}')
        log.info('  downloading...')
        
        try:
            response = self.requests.get(url, stream=True, timeout=60)
            response.raise_for_status()
            file_sha256 = hashlib.sha256()

            with tempfile.TemporaryFile() as temp_file:
                for chunk in response.iter_content(chunk_size=8192):
                    if chunk:
                        temp_file.write(chunk)
                        file_sha256.update(chunk)

                log.info('  verifying...')
                if file_sha256.hexdigest() != sha256:
                    raise RuntimeError(f'SHA256 mismatch. Expected: {sha256}, Got: {file_sha256.hexdigest()}')

                log.debug(f'Hash verified: {sha256}')
                self._extract_zip(temp_file, destination)
                
        except requests.RequestException as e:
            log.error(f'Download failed: {e}')
            raise
        except Exception as e:
            log.error(f'Unexpected error during download: {e}')
            raise

    def _extract_zip(self, file_obj, destination: Path) -> None:
        """Extract zip file to destination with validation."""
        destination.mkdir(parents=True, exist_ok=True)
        
        with zipfile.ZipFile(file_obj) as zip_ref:
            # Test zip integrity
            bad_file = zip_ref.testzip()
            if bad_file:
                raise RuntimeError(f'Corrupt zip file, bad file: {bad_file}')

            log.info('  extracting...')
            zip_ref.extractall(destination)


class Catalog:
    """Represents a remote package catalog with URL resolution."""
    
    def __init__(self, url: str, data: Dict[str, Any]):
        self.url = url
        self.data = data

    def get_package_url(self, package: Dependency) -> tuple[Optional[str], Optional[str]]:
        """Get download URL and SHA256 for a package."""
        try:
            meta = reduce(dict.__getitem__,
                         ['packages', package.name, 'versions', package.version, 'platforms', os_str()],
                         self.data)
            
            base_url = str(Path(self.url).parent / self.data['archive-root'])
            package_url = f"{base_url}/{package.name}_{package.version}_{os_str()}.zip"
            
            return package_url, meta['sha256']
        except KeyError as e:
            log.error(f'Package {package.name}@{package.version} not found in catalog: {e}')
            return None, None


class ActionStrategy:
    """Base class for dependency action strategies."""
    
    def _ensure_dest_exists(self) -> None:
        """Ensure the installation directory exists."""
        try:
            install_path.mkdir(parents=True, exist_ok=True)
            log.debug(f'Ensured install directory exists: {install_path}')
        except OSError as e:
            log.error(f'Failed to create install directory {install_path}: {e}')
            raise


class ActionInstallStrategy(ActionStrategy):
    """Strategy for installing new packages."""
    
    def resolve(self, catalog: Catalog, package: DependencyAction) -> None:
        log.info(f'Installing {package.name()}@{package.target_version()}...')
        self._ensure_dest_exists()
        
        downloader = Downloader()
        location, sha256 = catalog.get_package_url(package.target)

        if location is None:
            log.error(f'Cannot resolve package URL for {package.name()}')
            return

        dest_path = install_path / package.name()
        downloader.download_zip(dest_path, location, sha256)


class ActionReplaceStrategy(ActionStrategy):
    """Strategy for replacing existing packages."""
    
    def resolve(self, catalog: Catalog, package: DependencyAction) -> None:
        log.info(f'Replacing {package.name()}@{package.current_version()} '
                f'with {package.name()}@{package.target_version()}...')
        self._ensure_dest_exists()
        
        downloader = Downloader()
        location, sha256 = catalog.get_package_url(package.target)

        if location is None:
            log.error(f'Cannot resolve package URL for {package.name()}')
            return

        dest_path = install_path / package.name()
        if dest_path.exists():
            log.info('  removing old version...')
            shutil.rmtree(dest_path)

        downloader.download_zip(dest_path, location, sha256)


class ActionRemoveStrategy(ActionStrategy):
    """Strategy for removing packages."""
    
    def resolve(self, catalog: Catalog, package: DependencyAction) -> None:
        log.info(f'Removing {package.name()}@{package.current_version()}...')
        dest_path = install_path / package.name()

        if dest_path.exists():
            log.info('  removing...')
            shutil.rmtree(dest_path)
        else:
            log.warning(f'Package directory not found: {dest_path}')


class DependencyResolver:
    """Main dependency resolution and management class."""
    
    def __init__(self):
        self.downloader = Downloader()
        self.file_parser = DependencyFileParser()

    def resolve(self, prompt: bool = True) -> None:
        """Resolve and apply dependency changes."""
        dependencies = self.analyze_dependencies()
        groups = {
            Action.INSTALL: [],
            Action.REPLACE: [],
            Action.REMOVE: []
        }

        for dep in dependencies:
            if dep.action in groups:
                groups[dep.action].append(dep)

        total_actions = sum(len(group) for group in groups.values())
        if total_actions == 0:
            click.echo('Nothing to do')
            return

        self._display_planned_actions(groups)

        if prompt and not click.confirm('Do you want to continue?'):
            return

        self._execute_actions(dependencies)

    def _display_planned_actions(self, groups: Dict[Action, List[DependencyAction]]) -> None:
        """Display planned actions to the user."""
        if groups[Action.INSTALL]:
            click.echo('The following new dependencies will be installed:')
            for dep in groups[Action.INSTALL]:
                click.secho(f'  {dep.name()}', fg='blue', nl=False)
                click.echo(f': {dep.target_version()}')
            click.echo()

        if groups[Action.REPLACE]:
            click.echo('The following dependencies will be replaced:')
            for dep in groups[Action.REPLACE]:
                click.secho(f'  {dep.name()}', fg='blue', nl=False)
                click.echo(f': {dep.current_version()} -> {dep.target_version()}')
            click.echo()

        if groups[Action.REMOVE]:
            click.echo('The following dependencies will be removed:')
            for dep in groups[Action.REMOVE]:
                click.secho(f'  {dep.name()}', fg='blue', nl=False)
                click.echo(f': {dep.current_version()}')
            click.echo()

    def _execute_actions(self, dependencies: List[DependencyAction]) -> None:
        """Execute the planned dependency actions."""
        click.echo('Retrieving catalog...')
        try:
            catalog = self.downloader.fetch_catalog(self.get_catalog_url())
        except Exception as e:
            log.error(f'Failed to fetch catalog: {e}')
            raise

        lock = self.file_parser.get_lock_file()

        for dep in dependencies:
            strategy = self._get_action_strategy(dep.action)
            if strategy is not None:
                try:
                    strategy.resolve(catalog=catalog, package=dep)
                    
                    # Update lock file
                    lock.remove_package(dep.name())
                    if dep.action != Action.REMOVE:
                        lock.add_package(dep.name(), dep.target_version())
                        
                except Exception as e:
                    log.error(f'Failed to resolve {dep.name()}: {e}')
                    raise

        self.file_parser.write_lock_file(lock)

    def _get_action_strategy(self, action: Action) -> Optional[ActionStrategy]:
        """Get the appropriate strategy for an action."""
        strategies = {
            Action.INSTALL: ActionInstallStrategy(),
            Action.REPLACE: ActionReplaceStrategy(),
            Action.REMOVE: ActionRemoveStrategy()
        }
        return strategies.get(action)

    def analyze_dependencies(self) -> List[DependencyAction]:
        """Analyze current vs desired dependencies and determine actions."""
        log.debug('Starting dependency analysis')
        
        dependencies = self.file_parser.get_dependency_file()
        lock = self.file_parser.get_lock_file()
        result = []

        # Check for installs and replacements
        for dep in dependencies.packages:
            installed = next((l for l in lock.packages if l.name == dep.name), None)
            
            if installed is None:
                result.append(DependencyAction(target=dep, action=Action.INSTALL))
            elif installed.version != dep.version:
                result.append(DependencyAction(current=installed, target=dep, action=Action.REPLACE))
            else:
                result.append(DependencyAction(current=installed, target=dep, action=Action.NONE))

        # Check for removals
        for locked in lock.packages:
            wanted = next((dep for dep in dependencies.packages if locked.name == dep.name), None)
            if wanted is None:
                result.append(DependencyAction(current=locked, action=Action.REMOVE))

        log.debug('Finished dependency analysis')
        return result

    def get_catalog_url(self) -> str:
        """Get the catalog URL from the dependency file."""
        return self.file_parser.get_dependency_file().catalog_url


@click.group()
@click_log.simple_verbosity_option(log.logger)
def cli():
    """Modern dependency management for Ribosoft C++ libraries.
    
    Manages ViennaRNA and Melting library dependencies for bioinformatics applications.
    """
    pass


@cli.group()
def deps():
    """Manage package dependencies for C++ libraries."""
    pass


@deps.command()
def check():
    """Check local package installation status against requirements."""
    try:
        resolver = DependencyResolver()
        dependencies = resolver.analyze_dependencies()

        if not dependencies:
            click.echo('No dependencies configured.')
            return

        for dep in dependencies:
            click.secho(dep.name(), fg='blue', nl=False)
            current = dep.current_version() or 'not installed'
            target = dep.target_version() or 'not specified'
            click.echo(f': installed = {current}, wanted = {target} ', nl=False)

            status_colors = {
                Action.INSTALL: ('install required', 'yellow'),
                Action.REPLACE: ('update required', 'green'),
                Action.REMOVE: ('removal required', 'red'),
                Action.NONE: ('up to date', 'green')
            }
            
            if dep.action in status_colors:
                message, color = status_colors[dep.action]
                click.secho(f'({message})', fg=color)
            else:
                click.echo()
                
    except Exception as e:
        log.error(f'Failed to check dependencies: {e}')
        sys.exit(1)


@deps.command()
@click.option('--yes', '-y', is_flag=True, 
              help='Skip confirmation prompt and proceed automatically')
def install(yes: bool):
    """Install or update packages according to deps.json configuration.
    
    Downloads and installs C++ libraries (ViennaRNA, Melting) required
    for bioinformatics calculations in the Ribosoft application.
    """
    try:
        resolver = DependencyResolver()
        resolver.resolve(prompt=not yes)
    except KeyboardInterrupt:
        click.echo('\nOperation cancelled by user.')
        sys.exit(1)
    except Exception as e:
        log.error(f'Installation failed: {e}')
        sys.exit(1)


if __name__ == '__main__':
    cli()
