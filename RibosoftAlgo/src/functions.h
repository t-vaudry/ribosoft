#pragma once

#include "dll.h"
#include "error.h"

typedef uint8_t idx_t;

extern "C" DLL_PUBLIC int math_add(int a, int b);
extern "C" DLL_PUBLIC char* fold(const char* seq);

extern "C" DLL_PUBLIC R_ERROR validate_sequence(const char* sequence);
extern "C" DLL_PUBLIC R_ERROR validate_structure(const char* structure);