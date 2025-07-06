from ribosoft import check, install
from click.testing import CliRunner

def test_check(capsys):
    """Test the check command with modernized output format."""
    runner = CliRunner()
    result = runner.invoke(check, [])
    assert result.exit_code == 0
    
    # Check that the output contains the expected modernized format
    # The actual output depends on whether dependencies are installed
    output = result.output
    assert "viennarna:" in output
    assert "melting:" in output
    assert "installed =" in output
    assert "wanted =" in output
    
    # Check for modernized status indicators
    if "[OK]" in output:
        # Dependencies are installed
        assert "[OK] viennarna: installed = 2.4.3-121e5f9, wanted = 2.4.3-121e5f9 (up to date)" in output
        assert "[OK] melting: installed = 4.3-87f0ff7, wanted = 4.3-87f0ff7 (up to date)" in output
    else:
        # Dependencies need installation
        assert "[INSTALL]" in output or "install required" in output

def test_install(capsys):
    """Test the install command and verify post-install status."""
    runner = CliRunner()
    result = runner.invoke(install, ['--yes'])
    assert result.exit_code == 0

    # After installation, check should show dependencies as up to date
    checkRunner = CliRunner()
    checkResult = checkRunner.invoke(check, [])
    assert checkResult.exit_code == 0
    
    # Verify the modernized output format after installation
    output = checkResult.output
    assert "[OK] viennarna: installed = 2.4.3-121e5f9, wanted = 2.4.3-121e5f9 (up to date)" in output
    assert "[OK] melting: installed = 4.3-87f0ff7, wanted = 4.3-87f0ff7 (up to date)" in output