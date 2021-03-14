from ribosoft import *
from click.testing import CliRunner

def test_check(capsys):
    runner = CliRunner()
    result = runner.invoke(check, [])
    assert result.exit_code == 0
    assert result.output == "viennarna: installed = None, wanted = 2.4.3-121e5f9 (install required)\nmelting: installed = None, wanted = 4.3-87f0ff7 (install required)\n"

def test_install(capsys):
    runner = CliRunner()
    result = runner.invoke(install, ['--yes'])
    assert result.exit_code == 0

    checkRunner = CliRunner()
    checkResult = checkRunner.invoke(check, [])
    assert checkResult.exit_code == 0
    assert checkResult.output == "viennarna: installed = 2.4.3-121e5f9, wanted = 2.4.3-121e5f9 \nmelting: installed = 4.3-87f0ff7, wanted = 4.3-87f0ff7 \nviennarna: installed = 2.4.3-121e5f9, wanted = 2.4.3-121e5f9 \nmelting: installed = 4.3-87f0ff7, wanted = 4.3-87f0ff7 \nviennarna: installed = 2.4.3-121e5f9, wanted = 2.4.3-121e5f9 \nmelting: installed = 4.3-87f0ff7, wanted = 4.3-87f0ff7 \n"