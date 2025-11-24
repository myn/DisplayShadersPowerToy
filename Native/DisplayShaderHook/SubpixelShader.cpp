#include "SubpixelShader.h"
#include <d3dcompiler.h>
#include <fstream>

#pragma comment(lib, "d3dcompiler.lib")

namespace DisplayShader {

    // HLSL shader code for subpixel rendering
    // This shader resamples RGB channels based on actual subpixel positions
    const char* SUBPIXEL_SHADER_HLSL = R"(
        // Input texture from DirectWrite text rendering
        Texture2D<float4> textTexture : register(t0);
        
        // Subpixel mask texture (32x32 or 64x64)
        // R channel = where red subpixels are
        // G channel = where green subpixels are  
        // B channel = where blue subpixels are
        Texture2D<float4> subpixelMask : register(t1);
        
        SamplerState linearSampler : register(s0);
        
        cbuffer ShaderParams : register(b0)
        {
            float2 screenSize;      // Screen resolution
            float2 maskSize;        // Subpixel mask size (e.g., 32x32)
            float intensity;        // Shader intensity (0.0 - 1.0)
            int layoutType;         // SubpixelLayout enum value
            float2 padding;
        };
        
        struct PSInput
        {
            float4 position : SV_POSITION;
            float2 texCoord : TEXCOORD0;
        };
        
        // Get subpixel mask for current pixel
        float3 GetSubpixelMask(float2 screenPos)
        {
            // Calculate which part of the repeating mask we're in
            float2 maskUV = frac(screenPos / maskSize);
            return subpixelMask.Sample(linearSampler, maskUV).rgb;
        }
        
        // RBG orientation for WOLED (Blue in middle)
        float3 ApplyWrgbLayout(float3 originalRGB, float2 screenPos)
        {
            float3 mask = GetSubpixelMask(screenPos);
            
            // For WRGB stripe, we want R-B-G order (Blue in middle)
            // This compensates for the white subpixel interference
            float3 adjusted;
            adjusted.r = originalRGB.r * mask.r;  // Red where it should be
            adjusted.g = originalRGB.g * mask.b;  // Green shifted right
            adjusted.b = originalRGB.b * mask.g;  // Blue in middle
            
            return adjusted;
        }
        
        // Triangular layout for QD-OLED
        float3 ApplyTriangularLayout(float3 originalRGB, float2 screenPos)
        {
            float3 mask = GetSubpixelMask(screenPos);
            
            // For triangular layout:
            // - Green at top
            // - Red and Blue at bottom
            // We sample vertically to reduce top/bottom fringing
            
            float3 adjusted;
            adjusted.r = originalRGB.r * mask.r;
            adjusted.g = originalRGB.g * mask.g * 1.1; // Boost green slightly (it's larger)
            adjusted.b = originalRGB.b * mask.b;
            
            return adjusted;
        }
        
        // Standard RGB stripe
        float3 ApplyRgbStripe(float3 originalRGB, float2 screenPos)
        {
            // Fix for Issue #3: Standard LCD/LED fonts appear rough and jagged
            // Windows ClearType is already optimized for RGB stripe.
            // Applying a hard subpixel mask interferes with ClearType's subpixel rendering,
            // causing information loss and aliasing. We should pass through the original.
            return originalRGB;
        }
        
        // Pentile diamond pattern
        float3 ApplyPentileLayout(float3 originalRGB, float2 screenPos)
        {
            float3 mask = GetSubpixelMask(screenPos);
            
            // Pentile has shared green subpixels
            // Reduce green sharpness to avoid artifacts
            float3 adjusted;
            adjusted.r = originalRGB.r * mask.r;
            adjusted.g = originalRGB.g * mask.g * 0.9;
            adjusted.b = originalRGB.b * mask.b;
            
            return adjusted;
        }
        
        float4 main(PSInput input) : SV_TARGET
        {
            // Sample original text rendering
            float4 original = textTexture.Sample(linearSampler, input.texCoord);
            
            // Calculate screen position
            float2 screenPos = input.position.xy;
            
            // Apply subpixel correction based on layout type
            float3 corrected;
            
            if (layoutType == 1) // WrgbStripe
            {
                corrected = ApplyWrgbLayout(original.rgb, screenPos);
            }
            else if (layoutType == 2) // RgbTriangular
            {
                corrected = ApplyTriangularLayout(original.rgb, screenPos);
            }
            else if (layoutType == 3) // Pentile
            {
                corrected = ApplyPentileLayout(original.rgb, screenPos);
            }
            else // RgbStripe or None
            {
                corrected = ApplyRgbStripe(original.rgb, screenPos);
            }
            
            // Blend between original and corrected based on intensity
            float3 final = lerp(original.rgb, corrected, intensity);
            
            return float4(final, original.a);
        }
    )";

    SubpixelShader& SubpixelShader::Instance() {
        static SubpixelShader instance;
        return instance;
    }

    bool SubpixelShader::Initialize(ID3D11Device* device, ID3D11DeviceContext* context) {
        LogDebug(L"Initializing SubpixelShader...");

        if (m_initialized) {
            LogDebug(L"SubpixelShader already initialized");
            return true;
        }

        if (!device || !context) {
            LogError(L"Invalid D3D11 device or context");
            return false;
        }

        m_device = device;
        m_context = context;

        // Create shaders
        if (!CreateShaders()) {
            LogError(L"Failed to create shaders");
            return false;
        }

        // Create default mask texture
        if (!CreateMaskTexture()) {
            LogError(L"Failed to create mask texture");
            return false;
        }

        m_initialized = true;
        LogDebug(L"SubpixelShader initialized successfully");
        return true;
    }

    void SubpixelShader::Shutdown() {
        LogDebug(L"Shutting down SubpixelShader...");

        if (m_pixelShader) {
            m_pixelShader->Release();
            m_pixelShader = nullptr;
        }

        if (m_maskSRV) {
            m_maskSRV->Release();
            m_maskSRV = nullptr;
        }

        if (m_maskTexture) {
            m_maskTexture->Release();
            m_maskTexture = nullptr;
        }

        m_device = nullptr;
        m_context = nullptr;
        m_initialized = false;

        LogDebug(L"SubpixelShader shutdown complete");
    }

    void SubpixelShader::UpdateConfig(const ShaderConfig& config) {
        LogDebug(L"Updating SubpixelShader config");

        m_config = config;

        // Regenerate mask texture for new layout
        if (m_initialized) {
            CreateMaskTexture();
        }
    }

    HRESULT SubpixelShader::RenderGlyphRun(
        void* clientDrawingContext,
        FLOAT baselineOriginX,
        FLOAT baselineOriginY,
        DWRITE_MEASURING_MODE measuringMode,
        const DWRITE_GLYPH_RUN* glyphRun,
        const DWRITE_GLYPH_RUN_DESCRIPTION* glyphRunDescription,
        IUnknown* clientDrawingEffect)
    {
        if (!m_config.enabled) {
            return E_FAIL;
        }

        // Lazy initialization of D3D11 device
        if (!m_initialized) {
            if (!InitializeD3D11()) {
                LogError(L"Failed to initialize D3D11 for rendering");
                return E_FAIL;
            }
        }

        if (!m_device || !m_context) {
            LogError(L"D3D11 device not available");
            return E_FAIL;
        }

        // For POC, we'll log that we intercepted the call
        // Full implementation would:
        // 1. Render glyphs to a texture using DirectWrite/D2D
        // 2. Apply our pixel shader to the texture
        // 3. Composite the result back to the screen
        
        LogDebug(L"RenderGlyphRun called: %d glyphs at (%.1f, %.1f)",
            glyphRun->glyphCount, baselineOriginX, baselineOriginY);

        // Apply subpixel shader effect (simplified for POC)
        ApplySubpixelEffect();

        return S_OK;
    }

    bool SubpixelShader::InitializeD3D11() {
        LogDebug(L"Initializing D3D11 device...");

        // Create D3D11 device
        D3D_FEATURE_LEVEL featureLevels[] = {
            D3D_FEATURE_LEVEL_11_1,
            D3D_FEATURE_LEVEL_11_0,
            D3D_FEATURE_LEVEL_10_1,
            D3D_FEATURE_LEVEL_10_0
        };

        D3D_FEATURE_LEVEL featureLevel;
        UINT createDeviceFlags = 0;
#ifdef _DEBUG
        createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

        HRESULT hr = D3D11CreateDevice(
            nullptr,                    // Use default adapter
            D3D_DRIVER_TYPE_HARDWARE,   // Hardware acceleration
            nullptr,                    // No software rasterizer
            createDeviceFlags,
            featureLevels,
            ARRAYSIZE(featureLevels),
            D3D11_SDK_VERSION,
            &m_device,
            &featureLevel,
            &m_context);

        if (FAILED(hr)) {
            LogError(L"Failed to create D3D11 device: 0x%08X", hr);
            
            // Try WARP (software) as fallback
            hr = D3D11CreateDevice(
                nullptr,
                D3D_DRIVER_TYPE_WARP,
                nullptr,
                createDeviceFlags,
                featureLevels,
                ARRAYSIZE(featureLevels),
                D3D11_SDK_VERSION,
                &m_device,
                &featureLevel,
                &m_context);

            if (FAILED(hr)) {
                LogError(L"Failed to create D3D11 WARP device: 0x%08X", hr);
                return false;
            }

            LogDebug(L"Using WARP device (software rendering)");
        }

        LogDebug(L"D3D11 device created with feature level: 0x%X", featureLevel);

        // Create shaders
        if (!CreateShaders()) {
            LogError(L"Failed to create shaders");
            return false;
        }

        // Create mask texture
        if (!CreateMaskTexture()) {
            LogError(L"Failed to create mask texture");
            return false;
        }

        m_initialized = true;
        LogDebug(L"D3D11 initialization complete");
        return true;
    }

    void SubpixelShader::ApplySubpixelEffect() {
        if (!m_context || !m_pixelShader) {
            return;
        }

        // Set pixel shader
        m_context->PSSetShader(m_pixelShader, nullptr, 0);

        // Bind subpixel mask texture
        if (m_maskSRV) {
            m_context->PSSetShaderResources(1, 1, &m_maskSRV);
        }

        // In a full implementation, we would:
        // 1. Render glyphs to texture
        // 2. Apply this shader
        // 3. Output result
        
        // For now, just verify shader is set
        LogDebug(L"Subpixel shader applied");
    }

    bool SubpixelShader::CreateShaders() {
        LogDebug(L"Creating HLSL shaders...");

        // Compile pixel shader
        if (!CompileSubpixelShader(SUBPIXEL_SHADER_HLSL, &m_pixelShader)) {
            LogError(L"Failed to compile subpixel shader");
            return false;
        }

        LogDebug(L"Shaders created successfully");
        return true;
    }

    bool SubpixelShader::CompileSubpixelShader(const char* hlslCode, ID3D11PixelShader** outShader) {
        ID3DBlob* shaderBlob = nullptr;
        ID3DBlob* errorBlob = nullptr;

        HRESULT hr = D3DCompile(
            hlslCode,
            strlen(hlslCode),
            "SubpixelShader.hlsl",
            nullptr,
            nullptr,
            "main",
            "ps_5_0",
            D3DCOMPILE_ENABLE_STRICTNESS,
            0,
            &shaderBlob,
            &errorBlob);

        if (FAILED(hr)) {
            if (errorBlob) {
                LogError(L"Shader compilation error: %S", 
                    (char*)errorBlob->GetBufferPointer());
                errorBlob->Release();
            }
            return false;
        }

        hr = m_device->CreatePixelShader(
            shaderBlob->GetBufferPointer(),
            shaderBlob->GetBufferSize(),
            nullptr,
            outShader);

        shaderBlob->Release();

        if (FAILED(hr)) {
            LogError(L"Failed to create pixel shader: 0x%08X", hr);
            return false;
        }

        return true;
    }

    bool SubpixelShader::CreateMaskTexture() {
        LogDebug(L"Creating mask texture for layout: %d", static_cast<int>(m_config.layout));

        // Release old texture if exists
        if (m_maskTexture) {
            m_maskTexture->Release();
            m_maskTexture = nullptr;
        }
        if (m_maskSRV) {
            m_maskSRV->Release();
            m_maskSRV = nullptr;
        }

        // Generate appropriate mask based on layout
        switch (m_config.layout) {
        case SubpixelLayout::WrgbStripe:
            GenerateWrgbStripeMask();
            break;
        case SubpixelLayout::RgbTriangular:
            GenerateRgbTriangularMask();
            break;
        case SubpixelLayout::Pentile:
            GeneratePentileMask();
            break;
        case SubpixelLayout::RgbStripe:
        default:
            GenerateRgbStripeMask();
            break;
        }

        if (!m_currentMask) {
            LogError(L"Failed to generate mask");
            return false;
        }

        // Create D3D11 texture from mask data
        D3D11_TEXTURE2D_DESC texDesc = {};
        texDesc.Width = m_currentMask->width;
        texDesc.Height = m_currentMask->height;
        texDesc.MipLevels = 1;
        texDesc.ArraySize = 1;
        texDesc.Format = DXGI_FORMAT_R32G32B32A32_FLOAT;
        texDesc.SampleDesc.Count = 1;
        texDesc.Usage = D3D11_USAGE_DEFAULT;
        texDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;

        // Prepare initial data (RGBA float32)
        std::vector<float> textureData(m_currentMask->width * m_currentMask->height * 4);
        for (int i = 0; i < m_currentMask->width * m_currentMask->height; i++) {
            textureData[i * 4 + 0] = m_currentMask->redChannel[i];
            textureData[i * 4 + 1] = m_currentMask->greenChannel[i];
            textureData[i * 4 + 2] = m_currentMask->blueChannel[i];
            textureData[i * 4 + 3] = 1.0f; // Alpha
        }

        D3D11_SUBRESOURCE_DATA initData = {};
        initData.pSysMem = textureData.data();
        initData.SysMemPitch = m_currentMask->width * 4 * sizeof(float);

        HRESULT hr = m_device->CreateTexture2D(&texDesc, &initData, &m_maskTexture);
        if (FAILED(hr)) {
            LogError(L"Failed to create mask texture: 0x%08X", hr);
            return false;
        }

        // Create shader resource view
        D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
        srvDesc.Format = texDesc.Format;
        srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
        srvDesc.Texture2D.MipLevels = 1;

        hr = m_device->CreateShaderResourceView(m_maskTexture, &srvDesc, &m_maskSRV);
        if (FAILED(hr)) {
            LogError(L"Failed to create SRV: 0x%08X", hr);
            return false;
        }

        LogDebug(L"Mask texture created successfully");
        return true;
    }

    void SubpixelShader::GenerateRgbStripeMask() {
        // Standard RGB stripe: R G B | R G B | R G B
        // 3 pixels wide (one for each subpixel)
        m_currentMask = std::make_unique<SubpixelMask>(3, 1);

        // Red subpixel at position 0
        m_currentMask->redChannel[0] = 1.0f;
        
        // Green subpixel at position 1
        m_currentMask->greenChannel[1] = 1.0f;
        
        // Blue subpixel at position 2
        m_currentMask->blueChannel[2] = 1.0f;

        LogDebug(L"Generated RGB stripe mask");
    }

    void SubpixelShader::GenerateWrgbStripeMask() {
        // WOLED WRGB stripe: W R G B | W R G B
        // We want effective RBG order (Red-Blue-Green)
        // 4 pixels wide for W-R-G-B pattern
        m_currentMask = std::make_unique<SubpixelMask>(4, 1);

        // Position 0: White (ignore, we can't control it)
        // Position 1: Red
        m_currentMask->redChannel[1] = 1.0f;
        
        // Position 2: Green - but we want it to act like it's at position 3
        // Position 3: Blue - but we want it to act like it's at position 2
        // This creates the RBG effect
        m_currentMask->blueChannel[2] = 1.0f;  // Blue in middle
        m_currentMask->greenChannel[3] = 1.0f; // Green at right

        LogDebug(L"Generated WRGB stripe mask (RBG equivalent)");
    }

    void SubpixelShader::GenerateRgbTriangularMask() {
        // QD-OLED triangular layout:
        //     G
        //   R   B
        // 2x2 repeating pattern
        m_currentMask = std::make_unique<SubpixelMask>(2, 2);

        // Top row: Green at center-top
        m_currentMask->greenChannel[0 * 2 + 0] = 0.5f; // Left side of green
        m_currentMask->greenChannel[0 * 2 + 1] = 0.5f; // Right side of green

        // Bottom row: Red left, Blue right
        m_currentMask->redChannel[1 * 2 + 0] = 1.0f;
        m_currentMask->blueChannel[1 * 2 + 1] = 1.0f;

        LogDebug(L"Generated RGB triangular mask");
    }

    void SubpixelShader::GeneratePentileMask() {
        // Pentile diamond pattern (RGBG)
        // 2x2 repeating pattern
        m_currentMask = std::make_unique<SubpixelMask>(2, 2);

        // Pentile layout:
        // R G
        // G B
        m_currentMask->redChannel[0 * 2 + 0] = 1.0f;
        m_currentMask->greenChannel[0 * 2 + 1] = 1.0f;
        m_currentMask->greenChannel[1 * 2 + 0] = 1.0f;
        m_currentMask->blueChannel[1 * 2 + 1] = 1.0f;

        LogDebug(L"Generated Pentile mask");
    }

} // namespace DisplayShader
