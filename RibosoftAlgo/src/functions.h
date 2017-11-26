#pragma once

#include "dll.h"

extern "C" DLL_PUBLIC int math_add(int a, int b);
extern "C" DLL_PUBLIC char* fold(const char* seq);
extern "C" DLL_PUBLIC float structure(const char* candidate, const char* ideal);