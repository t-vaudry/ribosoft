#include "dll.h"

#include <iostream>
#include <cstdio>
#include <cstdlib>
#include <cstring>

#include <ViennaRNA/data_structures.h>

#include "functions.h"

RIBOSOFT_NAMESPACE_START

extern "C"
{
    DLL_PUBLIC int math_add(int a, int b) {
        return a + b;
    }
}

RIBOSOFT_NAMESPACE_END