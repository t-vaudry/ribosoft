# Test Coverage Improvement Plan

## Critical Issues Found

### 1. C++ mfe_default_fold.cpp - 0% Coverage ⚠️ CRITICAL

This is the most serious issue. The `mfe_default_fold.cpp` file has zero test coverage, meaning this core functionality is completely untested.

**Immediate Action Required:**

Create test file: `RibosoftAlgo.Tests/test/test_mfe_default_fold.cpp`

```cpp
#include "catch2/catch_amalgamated.hpp"
#include "mfe_default_fold.h"

TEST_CASE("MFE Default Fold - Basic Functionality", "[mfe_default_fold]") {
    SECTION("Valid RNA sequence folding") {
        std::string sequence = "GGGGAAAACCCC";
        auto result = mfe_default_fold(sequence);
        REQUIRE(!result.empty());
        // Add specific assertions based on expected output
    }
    
    SECTION("Empty sequence handling") {
        std::string empty_seq = "";
        REQUIRE_THROWS_AS(mfe_default_fold(empty_seq), std::invalid_argument);
    }
    
    SECTION("Invalid nucleotides handling") {
        std::string invalid_seq = "GGGGXXXXCCCC";
        REQUIRE_THROWS_AS(mfe_default_fold(invalid_seq), std::invalid_argument);
    }
}

TEST_CASE("MFE Default Fold - Edge Cases", "[mfe_default_fold]") {
    SECTION("Single nucleotide") {
        std::string single = "A";
        auto result = mfe_default_fold(single);
        // Test single nucleotide behavior
    }
    
    SECTION("Very long sequence") {
        std::string long_seq(1000, 'A');
        auto result = mfe_default_fold(long_seq);
        // Test performance and memory handling
    }
}
```

### 2. Python CLI Tool - 54.5% Coverage Gap

The Python CLI tool has significant untested functionality. Here are the specific areas to address:

**Add to `test_ribosoft.py`:**

```python
import pytest
import tempfile
import os
from unittest.mock import patch, mock_open
from ribosoft import check, install, deps

def test_check_missing_dependencies():
    """Test check command when dependencies are missing."""
    with patch('ribosoft.get_dependency_status') as mock_status:
        mock_status.return_value = {'viennarna': {'installed': False, 'wanted': '2.4.3'}}
        runner = CliRunner()
        result = runner.invoke(check, [])
        assert result.exit_code == 0
        assert "[INSTALL]" in result.output

def test_install_network_failure():
    """Test install command with network failure."""
    with patch('ribosoft.download_dependency') as mock_download:
        mock_download.side_effect = ConnectionError("Network error")
        runner = CliRunner()
        result = runner.invoke(install, ['--yes'])
        assert result.exit_code != 0
        assert "Network error" in result.output

def test_install_permission_error():
    """Test install command with permission issues."""
    with patch('ribosoft.extract_dependency') as mock_extract:
        mock_extract.side_effect = PermissionError("Permission denied")
        runner = CliRunner()
        result = runner.invoke(install, ['--yes'])
        assert result.exit_code != 0

def test_deps_command():
    """Test the deps subcommand functionality."""
    runner = CliRunner()
    result = runner.invoke(deps, ['check'])
    assert result.exit_code == 0

def test_invalid_command_args():
    """Test handling of invalid command arguments."""
    runner = CliRunner()
    result = runner.invoke(check, ['--invalid-flag'])
    assert result.exit_code != 0

def test_file_operations():
    """Test file I/O operations."""
    with tempfile.TemporaryDirectory() as temp_dir:
        test_file = os.path.join(temp_dir, 'test_deps.json')
        # Test file creation, reading, writing
        # Add specific file operation tests
```

### 3. C# Controller and Service Testing

The C# coverage excludes critical components. Here's how to add them back:

**Update `coverlet.runsettings`:**

```xml
<!-- Remove these exclusions to include in coverage: -->
<!-- [*]Ribosoft.Controllers.* -->
<!-- [*]Ribosoft.Services.* -->

<!-- Keep these exclusions: -->
<Exclude>[Ribosoft.Tests*]*,[*]Ribosoft.Blast.*,[*]Ribosoft.Data.*,[*]Ribosoft.Extensions.*,[*]Ribosoft.Jobs.*,[*]Ribosoft.Resources.*,[Ribosoft.Views]*</Exclude>
```

**Add Controller Tests:**

Create `Ribosoft.Tests/TestControllers.cs`:

```csharp
[Fact]
public void HomeController_Index_ReturnsViewResult()
{
    // Arrange
    var controller = new HomeController();
    
    // Act
    var result = controller.Index();
    
    // Assert
    Assert.IsType<ViewResult>(result);
}

[Fact]
public void RequestController_Post_ValidModel_ReturnsRedirect()
{
    // Add comprehensive controller tests
}
```

**Add Service Tests:**

Create `Ribosoft.Tests/TestServices.cs`:

```csharp
[Fact]
public void RibozymeService_GenerateDesign_ValidInput_ReturnsDesign()
{
    // Test service layer business logic
}
```

## Implementation Priority

### Week 1 (Critical)
1. ✅ Add tests for `mfe_default_fold.cpp`
2. ✅ Verify C++ coverage reaches 95%+
3. ✅ Add Python error handling tests

### Week 2 (High Priority)  
1. ✅ Include C# controllers in coverage
2. ✅ Add service layer tests
3. ✅ Expand Python CLI test scenarios

### Week 3 (Medium Priority)
1. ✅ Add branch coverage for C++
2. ✅ Implement integration tests
3. ✅ Add performance tests

## Measuring Success

**Target Coverage Goals:**
- C++: 95% line coverage, 85% branch coverage
- C#: 85% overall coverage (including previously excluded components)
- Python: 80% line coverage

**Quality Metrics:**
- Zero critical functions with 0% coverage
- All error paths tested
- Edge cases covered
- Performance characteristics validated

## Running the Improved Tests

After implementing these improvements, use the provided script:

```bash
./run_all_coverage.sh
```

This will run all test suites and generate comprehensive coverage reports, allowing you to track progress against these improvement goals.

## Continuous Improvement

1. **Set up coverage gates in CI/CD:**
   - Fail builds if coverage drops below thresholds
   - Require coverage for new code

2. **Regular coverage reviews:**
   - Weekly coverage reports
   - Identify and address new gaps quickly

3. **Mutation testing:**
   - Validate test quality beyond just coverage
   - Ensure tests actually catch bugs

The key is to address the critical `mfe_default_fold.cpp` gap immediately, as this represents the highest risk to the application's core functionality.
