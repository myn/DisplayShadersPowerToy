#pragma once

#include "Common.h"
#include <d3d11.h>
#include <dwrite.h>

namespace DisplayShader {

    /// <summary>
    /// Implements custom subpixel rendering for different OLED layouts
    /// This is where the actual "shader magic" happens
    /// </summary>
    class SubpixelShader {
    public:
        static SubpixelShader& Instance();

        /// <summary>
        /// Initialize D3D11 resources for shader rendering
        /// </summary>
        bool Initialize(ID3D11Device* device, ID3D11DeviceContext* context);

        /// <summary>
        /// Shutdown and release resources
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Update configuration
        /// </summary>
        void UpdateConfig(const ShaderConfig& config);

        /// <summary>
        /// Render a glyph run with custom subpixel logic
        /// This is called from the DirectWrite hook
        /// </summary>
        HRESULT RenderGlyphRun(
            void* clientDrawingContext,
            FLOAT baselineOriginX,
            FLOAT baselineOriginY,
            DWRITE_MEASURING_MODE measuringMode,
            const DWRITE_GLYPH_RUN* glyphRun,
            const DWRITE_GLYPH_RUN_DESCRIPTION* glyphRunDescription,
            IUnknown* clientDrawingEffect);

    private:
        SubpixelShader() = default;
        ~SubpixelShader() = default;

        SubpixelShader(const SubpixelShader&) = delete;
        SubpixelShader& operator=(const SubpixelShader&) = delete;

        bool m_initialized = false;
        ShaderConfig m_config;
        std::unique_ptr<SubpixelMask> m_currentMask;

        // D3D11 resources
        ID3D11Device* m_device = nullptr;
        ID3D11DeviceContext* m_context = nullptr;
        ID3D11PixelShader* m_pixelShader = nullptr;
        ID3D11Texture2D* m_maskTexture = nullptr;
        ID3D11ShaderResourceView* m_maskSRV = nullptr;

        // Shader creation and compilation
        bool CreateShaders();
        bool CreateMaskTexture();
        bool CompileSubpixelShader(const char* hlslCode, ID3D11PixelShader** outShader);
        bool InitializeD3D11();

        // Layout-specific mask generation
        void GenerateRgbStripeMask();
        void GenerateWrgbStripeMask();
        void GenerateRgbTriangularMask();
        void GeneratePentileMask();

        // Apply subpixel correction to rendered glyphs
        void ApplySubpixelCorrection(
            ID3D11Texture2D* glyphTexture,
            int x, int y, int width, int height);
        
        void ApplySubpixelEffect();
    };

} // namespace DisplayShader
