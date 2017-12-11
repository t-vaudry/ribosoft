#pragma once

#include "dll.h"

extern "C" DLL_PUBLIC int math_add(int a, int b);
extern "C" DLL_PUBLIC char* fold(const char* seq);
extern "C" DLL_PUBLIC void accessibility(const char* substrateSequence, const char* substrateTemplate, const int cutsiteIndex, const int cutsiteNumber, float& delta);