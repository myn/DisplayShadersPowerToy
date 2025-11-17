#pragma once

#include "Common.h"
#include <dwrite.h>

// MinHook doesn't need special defines
#define MH_ALL_HOOKS NULL

namespace DisplayShader {

    /// <summary>
    /// DirectWrite text renderer hook
    /// Intercepts glyph rendering to apply custom subpixel logic
    /// </summary>
    class DirectWriteHook {
    public:
        static DirectWriteHook& Instance();

        /// <summary>
        /// Initialize the DirectWrite hook system
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Shutdown and clean up hooks
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Update shader configuration
        /// </summary>
        void UpdateConfig(const ShaderConfig& config);

        /// <summary>
        /// Check if hooking is active
        /// </summary>
        bool IsActive() const { return m_isActive; }

    private:
        DirectWriteHook() = default;
        ~DirectWriteHook() = default;

        // Prevent copying
        DirectWriteHook(const DirectWriteHook&) = delete;
        DirectWriteHook& operator=(const DirectWriteHook&) = delete;

        bool m_isActive = false;
        ShaderConfig m_currentConfig;

        // Hook installation helpers
        bool InstallHooks();
        void RemoveHooks();

        // Hooked function pointers
        static HRESULT STDMETHODCALLTYPE Hook_DrawGlyphRun(
            void* clientDrawingContext,
            FLOAT baselineOriginX,
            FLOAT baselineOriginY,
            DWRITE_MEASURING_MODE measuringMode,
            DWRITE_GLYPH_RUN const* glyphRun,
            DWRITE_GLYPH_RUN_DESCRIPTION const* glyphRunDescription,
            IUnknown* clientDrawingEffect);

        // Original function pointer (will be filled by hooking library)
        using DrawGlyphRunFunc = HRESULT(STDMETHODCALLTYPE*)(
            void*, FLOAT, FLOAT, DWRITE_MEASURING_MODE,
            const DWRITE_GLYPH_RUN*, const DWRITE_GLYPH_RUN_DESCRIPTION*, IUnknown*);
        
        static DrawGlyphRunFunc Original_DrawGlyphRun;
    };

} // namespace DisplayShader
