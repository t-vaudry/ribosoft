#pragma once

#include "dll.h"
#include "error.h"

#include <cstdint>

RIBOSOFT_NAMESPACE_START

typedef uint8_t idx_t;

#pragma pack(push, 8)
struct fold_output {
    char* structure;
    float energy;
};
#pragma pack(pop)

extern "C" DLL_PUBLIC int math_add(int a, int b);

extern "C" DLL_PUBLIC R_STATUS validate_sequence(const char* sequence);
extern "C" DLL_PUBLIC R_STATUS validate_structure(const char* structure);

extern "C" DLL_PUBLIC R_STATUS accessibility(const char* substrateSequence, const char* substrateTemplate, const int cutsiteIndex, const int cutsiteNumber, /*out*/ float& delta);
extern "C" DLL_PUBLIC R_STATUS anneal(const char* sequence, const char* structure, const float na_concentration, const float probe_concentration, float& temp);
extern "C" DLL_PUBLIC R_STATUS fold(const char* sequence, /*out*/ fold_output*& output, /*out*/ size_t& size);
extern "C" DLL_PUBLIC R_STATUS structure(const char* candidate, const char* ideal, /*out*/ float& distance);

RIBOSOFT_NAMESPACE_END
