#!/usr/bin/env python
import logging
import click
import click_log
import json
import sys
from enum import Enum


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
    elif platform == 'linux2':
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
        self.packages.append(Dependency(name=name, version=version))


class DependencyFileParser(object):
    dependency_filename = 'deps.json'
    lock_filename = 'deps.lock'

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

    def __load_dependency_file(self):
        log.debug('Loading dependency file')

        filename = self.dependency_filename
        deps = self.__get_dependency_file_data(filename)
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

        filename = self.lock_filename
        lock = self.__get_lock_file_data(filename)
        lock_file = DependencyLockFile()

        if lock is not None:
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
        if 'packages' not in root:
            raise MalformedFileException('\'packages\' value missing')

        if not isinstance(root['packages'], list):
            raise MalformedFileException('\'packages\' is not an array')

        for package in root['packages']:
            if 'name' not in package:
                raise MalformedFileException('\'name\' value missing for package')

            if 'version' not in package:
                raise MalformedFileException('\'version\' value missing for package \'{0}\''.format(package['name']))


class ActionInstallStrategy(object):
    def resolve(self, package):
        pass


class ActionReplaceStrategy(object):
    def resolve(self, package):
        pass


class ActionRemoveStrategy(object):
    def resolve(self):
        pass


class DependencyResolver(object):
    def __init__(self):
        self.file_parser = DependencyFileParser()

    def check_install_status(self):
        dependencies = self.__analyze_dependencies()

        for dep in dependencies:
            click.echo('{0}: installed = {1}, wanted = {2} '.format(dep.name(), dep.current_version(),
                                                                    dep.target_version()),
                       nl=False)

            if dep.action == Action.INSTALL:
                click.secho('(install required)', fg='yellow', nl=False)
            elif dep.action == Action.REPLACE:
                click.secho('(replace required)', fg='blue', nl=False)
            elif dep.action == Action.REMOVE:
                click.secho('(remove required)', fg='red', nl=False)

            click.echo()

    def resolve(self):
        pass

    def __analyze_dependencies(self):
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

        return result


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
    resolver.check_install_status()


@deps.command()
def install():
    """Install or update packages"""
    resolver = DependencyResolver()
    resolver.resolve()


if __name__ == '__main__':
    cli()
