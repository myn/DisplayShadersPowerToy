#include "DirectWriteHook.h"
#include "SubpixelShader.h"
#include "include/MinHook.h"
#include <dwrite_3.h>

#pragma comment(lib, "dwrite.lib")

namespace DisplayShader {

    // Static member initialization
    DirectWriteHook::DrawGlyphRunFunc DirectWriteHook::Original_DrawGlyphRun = nullptr;

    // Store original COM interface pointer
    static IDWriteTextRenderer* g_pOriginalRenderer = nullptr;
    static IDWriteTextRenderer* g_pHookedRenderer = nullptr;

    DirectWriteHook& DirectWriteHook::Instance() {
        static DirectWriteHook instance;
        return instance;
    }

    bool DirectWriteHook::Initialize() {
        LogDebug(L"Initializing DirectWrite hook...");

        if (m_isActive) {
            LogDebug(L"Hook already active");
            return true;
        }

        // Initialize MinHook
        MH_STATUS status = MH_Initialize();
        if (status != MH_OK && status != MH_ERROR_ALREADY_INITIALIZED) {
            LogError(L"Failed to initialize MinHook: %d", status);
            return false;
        }

        if (!InstallHooks()) {
            LogError(L"Failed to install DirectWrite hooks");
            MH_Uninitialize();
            return false;
        }

        m_isActive = true;
        LogDebug(L"DirectWrite hook initialized successfully");
        return true;
    }

    void DirectWriteHook::Shutdown() {
        LogDebug(L"Shutting down DirectWrite hook...");

        if (!m_isActive) {
            return;
        }

        RemoveHooks();
        MH_Uninitialize();
        m_isActive = false;

        LogDebug(L"DirectWrite hook shutdown complete");
    }

    void DirectWriteHook::UpdateConfig(const ShaderConfig& config) {
        LogDebug(L"Updating shader config: Layout=%d, Intensity=%.2f, Enabled=%d",
            static_cast<int>(config.layout), config.intensity, config.enabled);

        m_currentConfig = config;

        // Update the shader engine with new config
        SubpixelShader::Instance().UpdateConfig(config);
    }

    bool DirectWriteHook::InstallHooks() {
        LogDebug(L"Installing DirectWrite hooks...");

        // Get DirectWrite factory
        IDWriteFactory* pFactory = nullptr;
        HRESULT hr = DWriteCreateFactory(
            DWRITE_FACTORY_TYPE_SHARED,
            __uuidof(IDWriteFactory),
            reinterpret_cast<IUnknown**>(&pFactory));

        if (FAILED(hr)) {
            LogError(L"Failed to create DirectWrite factory: 0x%08X", hr);
            return false;
        }

        // Create a simple text layout to get the renderer interface
        IDWriteTextFormat* pTextFormat = nullptr;
        hr = pFactory->CreateTextFormat(
            L"Segoe UI",
            nullptr,
            DWRITE_FONT_WEIGHT_NORMAL,
            DWRITE_FONT_STYLE_NORMAL,
            DWRITE_FONT_STRETCH_NORMAL,
            12.0f,
            L"en-us",
            &pTextFormat);

        if (SUCCEEDED(hr)) {
            IDWriteTextLayout* pTextLayout = nullptr;
            hr = pFactory->CreateTextLayout(
                L"Test",
                4,
                pTextFormat,
                100.0f,
                100.0f,
                &pTextLayout);

            if (SUCCEEDED(hr)) {
                // Get the renderer's vtable
                // The DrawGlyphRun method is at vtable offset 4 (after QueryInterface, AddRef, Release, IsPixelSnappingDisabled)
                void** vtable = *(void***)pTextLayout;
                
                // Hook DrawGlyphRun method
                // Note: This is a simplified approach. Production code would need more robust vtable analysis
                void* pDrawGlyphRun = vtable[4];  // Offset may vary

                MH_STATUS status = MH_CreateHook(
                    pDrawGlyphRun,
                    &Hook_DrawGlyphRun,
                    reinterpret_cast<LPVOID*>(&Original_DrawGlyphRun));

                if (status == MH_OK) {
                    status = MH_EnableHook(pDrawGlyphRun);
                    if (status == MH_OK) {
                        LogDebug(L"DrawGlyphRun hook installed successfully");
                        pTextLayout->Release();
                        pTextFormat->Release();
                        pFactory->Release();
                        return true;
                    }
                    else {
                        LogError(L"Failed to enable hook: %d", status);
                    }
                }
                else {
                    LogError(L"Failed to create hook: %d", status);
                }

                pTextLayout->Release();
            }
            pTextFormat->Release();
        }

        pFactory->Release();
        return false;
    }

    void DirectWriteHook::RemoveHooks() {
        if (Original_DrawGlyphRun != nullptr) {
            MH_DisableHook(MH_ALL_HOOKS);
            MH_RemoveHook(MH_ALL_HOOKS);
            
            LogDebug(L"Hooks removed");
            Original_DrawGlyphRun = nullptr;
        }
    }

    HRESULT STDMETHODCALLTYPE DirectWriteHook::Hook_DrawGlyphRun(
        void* clientDrawingContext,
        FLOAT baselineOriginX,
        FLOAT baselineOriginY,
        DWRITE_MEASURING_MODE measuringMode,
        DWRITE_GLYPH_RUN const* glyphRun,
        DWRITE_GLYPH_RUN_DESCRIPTION const* glyphRunDescription,
        IUnknown* clientDrawingEffect)
    {
        auto& instance = DirectWriteHook::Instance();

        // If shader is disabled or not configured, pass through
        if (!instance.m_currentConfig.enabled) {
            if (Original_DrawGlyphRun != nullptr) {
                return Original_DrawGlyphRun(
                    clientDrawingContext,
                    baselineOriginX,
                    baselineOriginY,
                    measuringMode,
                    glyphRun,
                    glyphRunDescription,
                    clientDrawingEffect);
            }
            return S_OK;
        }

        // Apply custom subpixel rendering
        HRESULT hr = SubpixelShader::Instance().RenderGlyphRun(
            clientDrawingContext,
            baselineOriginX,
            baselineOriginY,
            measuringMode,
            glyphRun,
            glyphRunDescription,
            clientDrawingEffect);

        // Fall back to original if shader fails
        if (FAILED(hr) && Original_DrawGlyphRun != nullptr) {
            return Original_DrawGlyphRun(
                clientDrawingContext,
                baselineOriginX,
                baselineOriginY,
                measuringMode,
                glyphRun,
                glyphRunDescription,
                clientDrawingEffect);
        }

        return hr;
    }

} // namespace DisplayShader
