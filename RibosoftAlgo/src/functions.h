#pragma once

#include "dll.h"

extern "C" DLL_PUBLIC int math_add(int a, int b);
extern "C" DLL_PUBLIC char* fold(const char* seq);
extern "C" DLL_PUBLIC DLL_PUBLIC float anneal(const char* sequence, const char* structure, float na_concentration);