#pragma once

#include "dll.h"
#include "error.h"

RIBOSOFT_NAMESPACE_START

typedef uint8_t idx_t;

extern "C" DLL_PUBLIC int math_add(int a, int b);
extern "C" DLL_PUBLIC char* fold(const char* seq);

extern "C" DLL_PUBLIC R_STATUS anneal(const char* sequence, const char* structure, float na_concentration, float& temp);

extern "C" DLL_PUBLIC R_STATUS validate_sequence(const char* sequence);
extern "C" DLL_PUBLIC R_STATUS validate_structure(const char* structure);

RIBOSOFT_NAMESPACE_END
