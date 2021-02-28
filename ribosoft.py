#!/usr/bin/env python
import logging
import click
import click_log
import requests
import json
import sys
import os
import tempfile
import hashlib
import zipfile
import shutil
from functools import reduce
from enum import Enum
from requests.adapters import HTTPAdapter
from requests.packages.urllib3.util.retry import Retry

if sys.version_info[0] != 3 and sys.version_info[1] < 5:
    print("This script requires Python version 3.5")
    sys.exit(1)

root_dir = os.path.dirname(os.path.realpath(__file__))

# Environment setup
dependency_file_path = os.path.join(root_dir, 'deps.json')
lock_file_path = os.path.join(root_dir, 'deps.lock')
install_path = os.path.join(root_dir, '.deps/')


class Log(object):
    def __init__(self, name=__name__, level=logging.DEBUG):
        self.logger = logging.getLogger(name)
        self.logger.setLevel(level)

        click_log.basic_config(self.logger)

    def debug(self, msg):
        self.logger.debug(msg)

    def info(self, msg):
        self.logger.info(msg)

    def warning(self, msg):
        self.logger.warning(msg)

    def error(self, msg):
        self.logger.error(msg)


log = Log()


class UnsupportedOSException(Exception):
    pass


def os_str():
    platform = sys.platform

    if platform == 'win32':
        return 'win'
    elif platform.startswith('linux'):
        return 'linux'
    elif platform == 'darwin':
        return 'mac'

    raise UnsupportedOSException()


class MalformedFileException(Exception):
    pass


class Action(Enum):
    NONE = 1
    INSTALL = 2
    REPLACE = 3
    REMOVE = 4


class Dependency(object):
    def __init__(self, name: str, version: str):
        self.name = name
        self.version = version


class DependencyAction(object):
    def __init__(self, current: Dependency = None, target: Dependency = None, action: Action = Action.NONE):
        self.current = current
        self.target = target
        self.action = action

    def set_action(self, action: Action):
        self.action = action

    def name(self):
        return self.current.name if self.current is not None else self.target.name

    def current_version(self):
        return self.current.version if self.current is not None else None

    def target_version(self):
        return self.target.version if self.target is not None else None


class DependencyFile(object):
    catalog_url = ''
    packages = []  # type: list[Dependency]

    def set_catalog_url(self, catalog_url):
        self.catalog_url = catalog_url

    def add_package(self, name, version):
        self.packages.append(Dependency(name=name, version=version))


class DependencyLockFile(object):
    packages = []  # type: list[Dependency]

    def add_package(self, name, version):
        # TODO: use Dependency as param
        self.packages.append(Dependency(name=name, version=version))

    def remove_package(self, name):
        self.packages = [p for p in self.packages if p.name != name]

    def serialize(self):
        root = {'packages': []}

        for p in self.packages:
            root['packages'].append(p.__dict__)

        return root


class DependencyFileParser(object):
    def __init__(self):
        self.__dependency_file = None  # type: DependencyFile
        self.__lock_file = None  # type: DependencyLockFile

    def get_dependency_file(self):
        if self.__dependency_file is None:
            self.__load_dependency_file()

        return self.__dependency_file

    def get_lock_file(self):
        if self.__lock_file is None:
            self.__load_lock_file()

        return self.__lock_file

    def write_lock_file(self, lock_file: DependencyLockFile):
        with open(lock_file_path, 'w') as f:
            json.dump(lock_file.serialize(), f, indent=2)

    def __load_dependency_file(self):
        log.debug('Loading dependency file')

        deps = self.__get_dependency_file_data(dependency_file_path)
        dependency_file = DependencyFile()

        if deps is not None:
            dependency_file.set_catalog_url(deps['catalog-url'])

            for package in deps['packages']:
                dependency_file.add_package(package['name'], package['version'])
                log.debug('Found package dependency: {0}@{1}'.format(package['name'], package['version']))

        self.__dependency_file = dependency_file

        log.debug('Finished loading dependency file')

    def __load_lock_file(self):
        log.debug('Loading lock file')

        lock = self.__get_lock_file_data(lock_file_path)
        lock_file = DependencyLockFile()

        if lock is not None and 'packages' in lock:
            for package in lock['packages']:
                lock_file.add_package(package['name'], package['version'])
                log.debug('Found installed package: {0}@{1}'.format(package['name'], package['version']))

        self.__lock_file = lock_file

        log.debug('Finished loading lock file')

    def __parse_json(self, str):
        try:
            return json.load(str)
        except ValueError as e:
            log.error('Failure parsing JSON: {0}'.format(e))
            raise

    def __get_dependency_file_data(self, filename):
        deps = None

        try:
            with open(filename) as json_data:
                deps = json.load(json_data)

            self.__validate_dependency_schema(deps)
        except IOError as e:
            log.warning('Cannot open dependency file \'{0}\''.format(filename))
            log.debug(e)
        except ValueError:
            log.error('Failure parsing JSON in \'{0}\''.format(filename))
            raise
        except MalformedFileException:
            log.error('Failure parsing dependency file \'{0}\''.format(filename))
            raise

        return deps

    def __get_lock_file_data(self, filename):
        deps = None

        try:
            with open(filename) as json_data:
                deps = self.__parse_json(json_data)

            self.__validate_lock_schema(deps)
        except IOError:
            log.debug('No lock file found')
        except MalformedFileException as e:
            log.error('Failure parsing lock file \'{0}\': {1}'.format(filename, e))
            raise e

        return deps

    def __validate_dependency_schema(self, root):
        if 'catalog-url' not in root:
            raise MalformedFileException('\'catalog-url\' value missing')

        if 'packages' not in root:
            raise MalformedFileException('\'packages\' value missing')

        if not isinstance(root['packages'], list):
            raise MalformedFileException('\'packages\' is not an array')

        for package in root['packages']:
            if 'name' not in package:
                raise MalformedFileException('\'name\' value missing for package')

            if 'version' not in package:
                raise MalformedFileException('\'version\' value missing for package \'{0}\''.format(package['name']))

    def __validate_lock_schema(self, root):
        if 'packages' in root:
            if not isinstance(root['packages'], list):
                raise MalformedFileException('\'packages\' is not an array')

            for package in root['packages']:
                if 'name' not in package:
                    raise MalformedFileException('\'name\' value missing for package')

                if 'version' not in package:
                    raise MalformedFileException(
                        '\'version\' value missing for package \'{0}\''.format(package['name']))


class Downloader(object):
    def __init__(self):
        # Set up requests with Retry mechanism (max 5 retries with 1s progressive backoff between retries)
        retry_strategy = Retry(
            total=5,
            backoff_factor=1,
            status_forcelist=[429, 500, 502, 503, 504],
            method_whitelist=['HEAD', 'GET', 'OPTIONS']
        )
        adapter = HTTPAdapter(max_retries=retry_strategy)
        session = requests.Session()
        session.mount('https://', adapter)
        session.mount('http://', adapter)
        self.requests = session

    def fetch_catalog(self, url):
        try:
            r = self.requests.get(url)
            r.raise_for_status()
            data = r.json()
        except requests.HTTPError as e:
            if e.response is not None and e.response.status_code == 404:
                log.error('Failure fetching remote catalog: Catalog does not exist')
            else:
                log.error('Failure fetching remote catalog: Server returned bad status')
            raise
        except (requests.RequestException, ConnectionError):
            log.error('Failure fetching remote catalog: Request failed')
            raise
        except ValueError:
            log.error('Failure fetching remote catalog: Catalog is invalid')
            raise

        return Catalog(url, data)

    def download_zip(self, destination, url, sha256):
        log.debug('Downloading \'{0}\' to \'{1}\''.format(url, destination))
        log.info('  downloading...')
        r = self.requests.get(url, stream=True)
        file_sha256 = hashlib.sha256()

        with tempfile.TemporaryFile() as f:
            for chunk in r.iter_content(chunk_size=1024):
                if chunk:
                    f.write(chunk)
                    file_sha256.update(chunk)

            log.info('  verifying...')
            if file_sha256.hexdigest() != sha256:
                log.error('Download hash mismatch')
                raise RuntimeError('SHA256 does not match')

            log.debug('Hash OK: {0}'.format(sha256))

            self.extract_zip(file=f, destination=destination)

    def extract_zip(self, file, destination):
        with zipfile.ZipFile(file) as zip_ref:
            try:
                zip_ref.testzip()
            except RuntimeError:
                log.error('Downloaded zipfile is corrupt')
                raise

            log.info('  extracting...')
            zip_ref.extractall(destination)


class Catalog(object):
    def __init__(self, url, data):
        self.url = url
        self.data = data

    def get_package_url(self, package: Dependency):
        try:
            meta = reduce(dict.__getitem__,
                          ['packages', package.name, 'versions', package.version, 'platforms', os_str()],
                          self.data)
        except KeyError:
            log.error('Package does not exist in catalog')
            return None, None

        return os.path.join(os.path.dirname(self.url), self.data['archive-root'],
                            '{0}_{1}_{2}.zip'.format(package.name, package.version, os_str())), meta['sha256']


class ActionStrategy(object):
    def _ensure_dest_exists(self):
        try:
            if not os.path.exists(install_path):
                log.debug('Creating new install directory at {0}'.format(install_path))
                os.makedirs(install_path)
        except OSError:
            log.error('Failure creating dependency install directory \'{0}\''.format(install_path))
            raise


class ActionInstallStrategy(ActionStrategy):
    def resolve(self, catalog: Catalog, package: DependencyAction):
        log.info('Installing {0}@{1}...'.format(package.name(), package.target_version()))
        self._ensure_dest_exists()
        downloader = Downloader()

        location, sha256 = catalog.get_package_url(package.target)

        if location is None:
            return

        dest_path = os.path.join(install_path, package.name())
        downloader.download_zip(dest_path, location, sha256)


class ActionReplaceStrategy(ActionStrategy):
    def resolve(self, catalog: Catalog, package: DependencyAction):
        log.info('Replacing {0}@{1} with {0}@{2}...'.format(package.name(), package.current_version(),
                                                            package.target_version()))
        self._ensure_dest_exists()
        downloader = Downloader()

        location, sha256 = catalog.get_package_url(package.target)

        if location is None:
            return

        dest_path = os.path.join(install_path, package.name())
        if os.path.exists(dest_path):
            log.info('  removing...')
            shutil.rmtree(dest_path)

        downloader.download_zip(dest_path, location, sha256)


class ActionRemoveStrategy(ActionStrategy):
    def resolve(self, catalog: Catalog, package: DependencyAction):
        log.info('Removing {0}@{1}...'.format(package.name(), package.current_version()))
        self._ensure_dest_exists()
        dest_path = os.path.join(install_path, package.name())

        if os.path.exists(dest_path):
            log.info('  removing...')
            shutil.rmtree(dest_path)


class DependencyResolver(object):
    def __init__(self):
        self.downloader = Downloader()
        self.file_parser = DependencyFileParser()

    def resolve(self, prompt=True):
        dependencies = self.analyze_dependencies()
        groups = {Action.INSTALL: [], Action.REPLACE: [], Action.REMOVE: []}

        for dep in dependencies:
            if dep.action in groups:
                groups[dep.action].append(dep)

        if sum(map(lambda x: len(x), groups.values())) == 0:
            click.echo('Nothing to do')
            return

        if len(groups[Action.INSTALL]):
            click.echo('The following new dependencies will be installed:')
            for d in groups[Action.INSTALL]:
                click.secho('  {0}'.format(d.name()), fg='blue', nl=False)
                click.echo(': {0}'.format(d.target_version()))
            click.echo()

        if len(groups[Action.REPLACE]):
            click.echo('The following dependencies will be replaced:')
            for d in groups[Action.REPLACE]:
                click.secho('  {0}'.format(d.name()), fg='blue', nl=False)
                click.echo(': {0} -> {1}'.format(d.current_version(), d.target_version()))
            click.echo()

        if len(groups[Action.REMOVE]):
            click.echo('The following dependencies will be removed:')
            for d in groups[Action.REMOVE]:
                click.secho('  {0}'.format(d.name()), fg='blue', nl=False)
                click.echo(': {0}'.format(d.current_version()))
            click.echo()

        if prompt and not click.confirm('Do you want to continue?'):
            return

        click.echo('Retrieving catalog')
        catalog = self.downloader.fetch_catalog(self.get_catalog_url())
        lock = self.file_parser.get_lock_file()

        # TODO: pass touch_lock lambda

        for dep in dependencies:
            resolver = None

            if dep.action == Action.INSTALL:
                resolver = ActionInstallStrategy()
            elif dep.action == Action.REPLACE:
                resolver = ActionReplaceStrategy()
            elif dep.action == Action.REMOVE:
                resolver = ActionRemoveStrategy()

            if resolver is not None:
                resolver.resolve(catalog=catalog, package=dep)

                lock.remove_package(dep.name())
                if dep.action != Action.REMOVE:
                    lock.add_package(dep.name(), dep.target_version())

        self.file_parser.write_lock_file(lock)

    def analyze_dependencies(self):
        log.debug('Starting dependency analysis')
        dependencies = self.file_parser.get_dependency_file()
        lock = self.file_parser.get_lock_file()
        result = []  # type: list[DependencyAction]

        for dep in dependencies.packages:
            installed = None
            for l in lock.packages:
                if l.name == dep.name:
                    installed = l
                    break

            if installed is None:
                result.append(DependencyAction(target=dep, action=Action.INSTALL))
            elif installed.version != dep.version:
                result.append(DependencyAction(current=installed, target=dep, action=Action.REPLACE))
            else:
                result.append(DependencyAction(current=installed, target=dep, action=Action.NONE))

        for l in lock.packages:
            wanted = None
            for dep in dependencies.packages:
                if l.name == dep.name:
                    wanted = dep
                    break

            if wanted is None:
                result.append(DependencyAction(current=l, target=wanted, action=Action.REMOVE))

        log.debug('Finished dependency analysis')
        return result

    def get_catalog_url(self):
        return self.file_parser.get_dependency_file().catalog_url


@click.group()
@click_log.simple_verbosity_option(log.logger)
def cli():
    pass


@cli.group()
def deps():
    """Manage package dependencies"""
    pass


@deps.command()
def check():
    """Check local package install status"""
    resolver = DependencyResolver()
    dependencies = resolver.analyze_dependencies()

    for dep in dependencies:
        click.secho(dep.name(), fg='blue', nl=False)
        click.echo(': installed = {0}, wanted = {1} '.format(dep.current_version(), dep.target_version()), nl=False)

        if dep.action == Action.INSTALL:
            click.secho('(install required)', fg='yellow', nl=False)
        elif dep.action == Action.REPLACE:
            click.secho('(replace required)', fg='green', nl=False)
        elif dep.action == Action.REMOVE:
            click.secho('(remove required)', fg='red', nl=False)

        click.echo()


@deps.command()
@click.option('--yes', '-y', is_flag=True, help='do not prompt for confirmation')
def install(yes):
    """Install or update packages"""
    resolver = DependencyResolver()
    resolver.resolve(prompt=not yes)


if __name__ == '__main__':
    cli()
