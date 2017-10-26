#include "dll.h"

#include <iostream>

extern "C" DLL_PUBLIC int math_add(int a, int b);

extern "C"
{
	DLL_PUBLIC int math_add(int a, int b) {
		return a + b;
	}
}