/*
 * Simplified MinHook Implementation
 * This is a minimal stub implementation for development/testing
 */

#include "include/MinHook.h"
#include <windows.h>
#include <vector>
#include <mutex>

struct HookEntry {
    LPVOID pTarget;
    LPVOID pDetour;
    LPVOID pOriginal;
    bool enabled;
};

static std::vector<HookEntry> g_hooks;
static std::mutex g_hookMutex;
static bool g_initialized = false;

extern "C" {

MH_STATUS MH_Initialize() {
    std::lock_guard<std::mutex> lock(g_hookMutex);
    
    if (g_initialized) {
        return MH_ERROR_ALREADY_INITIALIZED;
    }

    g_hooks.clear();
    g_initialized = true;
    
    return MH_OK;
}

MH_STATUS MH_Uninitialize() {
    std::lock_guard<std::mutex> lock(g_hookMutex);
    
    if (!g_initialized) {
        return MH_ERROR_NOT_INITIALIZED;
    }

    // Disable all hooks
    for (auto& hook : g_hooks) {
        if (hook.enabled) {
            MH_DisableHook(hook.pTarget);
        }
    }

    g_hooks.clear();
    g_initialized = false;
    
    return MH_OK;
}

MH_STATUS MH_CreateHook(LPVOID pTarget, LPVOID pDetour, LPVOID* ppOriginal) {
    std::lock_guard<std::mutex> lock(g_hookMutex);
    
    if (!g_initialized) {
        return MH_ERROR_NOT_INITIALIZED;
    }

    if (!pTarget || !pDetour) {
        return MH_ERROR_UNSUPPORTED_FUNCTION;
    }

    // Check if already hooked
    for (const auto& hook : g_hooks) {
        if (hook.pTarget == pTarget) {
            return MH_ERROR_ALREADY_CREATED;
        }
    }

    // Stub implementation - just store the information
    HookEntry entry;
    entry.pTarget = pTarget;
    entry.pDetour = pDetour;
    entry.pOriginal = pTarget;
    entry.enabled = false;

    if (ppOriginal) {
        *ppOriginal = entry.pOriginal;
    }

    g_hooks.push_back(entry);
    
    return MH_OK;
}

MH_STATUS MH_EnableHook(LPVOID pTarget) {
    std::lock_guard<std::mutex> lock(g_hookMutex);
    
    if (!g_initialized) {
        return MH_ERROR_NOT_INITIALIZED;
    }

    for (auto& hook : g_hooks) {
        if (hook.pTarget == pTarget) {
            if (hook.enabled) {
                return MH_ERROR_ENABLED;
            }
            
            hook.enabled = true;
            return MH_OK;
        }
    }

    return MH_ERROR_NOT_CREATED;
}

MH_STATUS MH_DisableHook(LPVOID pTarget) {
    std::lock_guard<std::mutex> lock(g_hookMutex);
    
    if (!g_initialized) {
        return MH_ERROR_NOT_INITIALIZED;
    }

    // MH_ALL_HOOKS special value
    if (pTarget == MH_ALL_HOOKS) {
        for (auto& hook : g_hooks) {
            if (hook.enabled) {
                hook.enabled = false;
            }
        }
        return MH_OK;
    }

    for (auto& hook : g_hooks) {
        if (hook.pTarget == pTarget) {
            if (!hook.enabled) {
                return MH_ERROR_DISABLED;
            }

            hook.enabled = false;
            return MH_OK;
        }
    }

    return MH_ERROR_NOT_CREATED;
}

MH_STATUS MH_RemoveHook(LPVOID pTarget) {
    std::lock_guard<std::mutex> lock(g_hookMutex);
    
    if (!g_initialized) {
        return MH_ERROR_NOT_INITIALIZED;
    }

    // MH_ALL_HOOKS special value
    if (pTarget == MH_ALL_HOOKS) {
        g_hooks.clear();
        return MH_OK;
    }

    for (auto it = g_hooks.begin(); it != g_hooks.end(); ++it) {
        if (it->pTarget == pTarget) {
            if (it->enabled) {
                MH_DisableHook(pTarget);
            }
            g_hooks.erase(it);
            return MH_OK;
        }
    }

    return MH_ERROR_NOT_CREATED;
}

} // extern "C"
