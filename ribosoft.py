#!/usr/bin/env python
import logging
import click
import click_log
import json


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


class MalformedFileException(Exception):
    pass


class DependencyFile(object):
    catalog_url = ''
    packages = []  # type: list[dict[str, str]]

    def set_catalog_url(self, catalog_url):
        self.catalog_url = catalog_url

    def add_package(self, name, version):
        self.packages.append({'name': name, 'version': version})


class DependencyLockFile(object):
    packages = []  # type: list[dict[str, str]]

    def add_package(self, name, version):
        self.packages.append({'name': name, 'version': version})


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


class DependencyResolver(object):
    def __init__(self):
        self.file_parser = DependencyFileParser()

    def check_install_status(self):
        dependencies = self.file_parser.get_dependency_file()
        lock = self.file_parser.get_lock_file()

        for dep in dependencies.packages:
            installed_version = None
            for l in lock.packages:
                if l['name'] == dep['name']:
                    installed_version = l['version']
                    break

            click.echo('{0}: installed = {1}, wanted = {2} '.format(dep['name'], installed_version, dep['version']),
                       nl=False)

            if installed_version is None and dep['version'] is not None:
                click.secho('(install required)', fg='yellow')
            elif installed_version != dep['version']:
                click.secho('(replace required)', fg='blue')
            else:
                click.echo()

        for l in lock.packages:
            depend_version = None
            for dep in dependencies.packages:
                if l['name'] == dep['name']:
                    depend_version = dep['version']
                    break

            if depend_version is None:
                click.secho('{0}: installed = {1}, wanted = {2} '.format(l['name'], l['version'], depend_version),
                            nl=False)
                click.secho('(removal required)', fg='red')


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
    click.echo('install')


if __name__ == '__main__':
    cli()
