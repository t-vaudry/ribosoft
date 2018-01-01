#pragma once

#include "dll.h"
#include "error.h"

#include <cstdint>

RIBOSOFT_NAMESPACE_START

typedef uint8_t idx_t;

struct fold_output {
    char* structure;
    float energy;
};

extern "C" DLL_PUBLIC int math_add(int a, int b);

extern "C" DLL_PUBLIC R_STATUS validate_sequence(const char* sequence);
extern "C" DLL_PUBLIC R_STATUS validate_structure(const char* structure);

extern "C" DLL_PUBLIC R_STATUS fold(const char* sequence, /*out*/ fold_output*& output);
extern "C" DLL_PUBLIC R_STATUS structure(const char* candidate, const char* ideal, /*out*/ float& distance);

RIBOSOFT_NAMESPACE_END