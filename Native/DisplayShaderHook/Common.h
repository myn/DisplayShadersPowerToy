#pragma once

#include <windows.h>
#include <d3d11.h>
#include <dwrite.h>
#include <d2d1.h>
#include <string>
#include <vector>
#include <map>
#include <memory>

// Export macro for DLL
#ifdef DISPLAYSHADER_EXPORTS
#define DISPLAYSHADER_API __declspec(dllexport)
#else
#define DISPLAYSHADER_API __declspec(dllimport)
#endif

namespace DisplayShader {

    /// <summary>
    /// Subpixel layout types matching the C# enum
    /// </summary>
    enum class SubpixelLayout {
        RgbStripe = 0,      // Standard LCD RGB stripe
        WrgbStripe = 1,     // WOLED with white subpixel (W-R-G-B)
        RgbTriangular = 2,  // QD-OLED triangular (Green top, Red/Blue bottom)
        Pentile = 3,        // Diamond pentile pattern
        None = 4            // Disabled
    };

    /// <summary>
    /// Configuration for a specific render profile (per monitor or default)
    /// </summary>
    struct RenderProfile {
        SubpixelLayout layout;
        float intensity;              // 0.0 to 1.0
        
        RenderProfile() 
            : layout(SubpixelLayout::RgbStripe)
            , intensity(1.0f)
        {}
    };

    /// <summary>
    /// Global configuration containing defaults and per-monitor overrides
    /// </summary>
    struct GlobalConfig {
        bool enabled;
        RenderProfile defaultProfile;
        std::map<std::wstring, RenderProfile> monitorProfiles;
        
        GlobalConfig() : enabled(true) {}
    };

    // Typedef for backward compatibility if needed, or just replace usage
    using ShaderConfig = GlobalConfig;

    /// <summary>
    /// Subpixel mask data loaded from PNG
    /// RGB channels indicate where each color subpixel is located
    /// </summary>
    struct SubpixelMask {
        int width;
        int height;
        std::vector<float> redChannel;    // Red subpixel positions (0.0-1.0)
        std::vector<float> greenChannel;  // Green subpixel positions (0.0-1.0)
        std::vector<float> blueChannel;   // Blue subpixel positions (0.0-1.0)

        SubpixelMask(int w, int h)
            : width(w)
            , height(h)
            , redChannel(w * h, 0.0f)
            , greenChannel(w * h, 0.0f)
            , blueChannel(w * h, 0.0f)
        {
        }

        float GetRed(int x, int y) const {
            return redChannel[y * width + x];
        }

        float GetGreen(int x, int y) const {
            return greenChannel[y * width + x];
        }

        float GetBlue(int x, int y) const {
            return blueChannel[y * width + x];
        }
    };

    /// <summary>
    /// Logging helper for debugging
    /// </summary>
    inline void LogDebug(const wchar_t* format, ...) {
#ifdef _DEBUG
        wchar_t buffer[1024];
        va_list args;
        va_start(args, format);
        vswprintf_s(buffer, format, args);
        va_end(args);
        OutputDebugStringW(L"[DisplayShaderHook] ");
        OutputDebugStringW(buffer);
        OutputDebugStringW(L"\n");
#endif
    }

    inline void LogError(const wchar_t* format, ...) {
        wchar_t buffer[1024];
        va_list args;
        va_start(args, format);
        vswprintf_s(buffer, format, args);
        va_end(args);
        OutputDebugStringW(L"[DisplayShaderHook ERROR] ");
        OutputDebugStringW(buffer);
        OutputDebugStringW(L"\n");
    }

} // namespace DisplayShader
