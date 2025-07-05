# RibosoftAlgo

Modern C++23 native library for ribozyme design algorithms with ViennaRNA integration.

## Features

- **C++23 Standard**: Built with cutting-edge C++ features for optimal performance
- **ViennaRNA Integration**: Complete RNA folding and structure prediction capabilities
- **Melting Temperature**: Accurate melting temperature calculations
- **Cross-Platform**: Supports Linux, Windows, and macOS (including Apple Silicon)
- **High Performance**: Optimized with OpenMP, LTO, and native CPU instructions
- **Comprehensive Testing**: Full Catch2 test suite with 23+ test cases

## Algorithms Included

- **RNA Folding**: Minimum free energy structure prediction
- **Accessibility**: RNA accessibility calculations for target regions
- **Annealing**: RNA-RNA interaction energy calculations
- **Structure Comparison**: Secondary structure similarity metrics
- **Validation**: RNA sequence and structure validation

## Usage

This library is designed to be consumed by .NET applications through P/Invoke. The native library provides C-style exports that can be called from managed code.

## Requirements

- .NET 8.0 or higher
- Platform-specific runtime:
  - Linux: x64 with glibc 2.17+
  - Windows: x64 with Visual C++ Redistributable
  - macOS: x64 or ARM64 (Apple Silicon)

## Performance

Built with aggressive optimizations:
- `-O3` optimization level
- Link-time optimization (LTO)
- Native CPU instruction targeting
- OpenMP parallelization
- Modern C++23 features

## License

GNU General Public License v3.0

## Repository

https://github.com/t-vaudry/ribosoft
