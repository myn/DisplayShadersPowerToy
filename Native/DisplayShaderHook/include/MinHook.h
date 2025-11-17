/*
 * MinHook - Minimalistic API Hook Library
 * Copyright (C) 2009-2017 Tsuda Kageyu. All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES ARE DISCLAIMED.
 */

// Minimal MinHook header - simplified for our use case
// Full version: https://github.com/TsudaKageyu/minhook

#pragma once

#include <windows.h>

#ifdef __cplusplus
extern "C" {
#endif

// MinHook Error Codes
typedef enum MH_STATUS
{
    MH_OK = 0,
    MH_ERROR_ALREADY_INITIALIZED,
    MH_ERROR_NOT_INITIALIZED,
    MH_ERROR_ALREADY_CREATED,
    MH_ERROR_NOT_CREATED,
    MH_ERROR_ENABLED,
    MH_ERROR_DISABLED,
    MH_ERROR_NOT_EXECUTABLE,
    MH_ERROR_UNSUPPORTED_FUNCTION,
    MH_ERROR_MEMORY_ALLOC,
    MH_ERROR_MEMORY_PROTECT,
    MH_ERROR_MODULE_NOT_FOUND,
    MH_ERROR_FUNCTION_NOT_FOUND
} MH_STATUS;

// Special value for all hooks
#define MH_ALL_HOOKS NULL

// Initialize MinHook
MH_STATUS MH_Initialize();

// Uninitialize MinHook
MH_STATUS MH_Uninitialize();

// Create a hook for the specified function
MH_STATUS MH_CreateHook(LPVOID pTarget, LPVOID pDetour, LPVOID* ppOriginal);

// Enable a disabled hook
MH_STATUS MH_EnableHook(LPVOID pTarget);

// Disable an enabled hook
MH_STATUS MH_DisableHook(LPVOID pTarget);

// Remove a hook
MH_STATUS MH_RemoveHook(LPVOID pTarget);

#ifdef __cplusplus
}
#endif
