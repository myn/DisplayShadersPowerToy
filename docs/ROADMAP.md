# Roadmap: Implementing Actual Display Shaders

## Current Status (v1.x)

? ClearType settings helper  
? OLED-optimized presets  
? No actual shader support  
? Cannot truly fix WOLED/QD-OLED  

## Phase 1: Honest Rebranding (Immediate)

**Goal**: Remove misleading claims, be transparent about limitations

**Tasks**:
- [x] Add technical limitations documentation
- [x] Update README with disclaimers
- [x] Add UI warning about limitations
- [x] Update code comments
- [ ] Consider renaming to "ClearType OLED Helper" or similar
- [ ] Update all documentation
- [ ] Release v1.1 with honest branding

**Timeline**: 1-2 weeks

## Phase 2: Research & Proof of Concept (2-3 months)

**Goal**: Determine if DirectX shader approach is viable

### 2.1 Research DirectX Text Hooking

**Investigate**:
- ReShade architecture and how it hooks D3D
- Microsoft Detours for API hooking
- DirectWrite text rendering pipeline
- Where in the pipeline to inject shader

**Resources**:
- [ReShade Source Code](https://github.com/crosire/reshade)
- [Microsoft Detours](https://github.com/microsoft/Detours)
- [DirectWrite Documentation](https://docs.microsoft.com/en-us/windows/win32/directwrite/)

**Deliverable**: Technical feasibility report

### 2.2 Create Minimal Shader POC

**Goal**: Prove we can intercept and modify text rendering

**Approach**:
```cpp
// Hook IDWriteTextRenderer::DrawGlyphRun
HRESULT STDMETHODCALLTYPE DrawGlyphRun(
    void* clientDrawingContext,
    FLOAT baselineOriginX,
    FLOAT baselineOriginY,
    DWRITE_MEASURING_MODE measuringMode,
    DWRITE_GLYPH_RUN const* glyphRun,
    // ... more params
) {
    // 1. Let original rendering happen to buffer
    // 2. Apply custom subpixel shader
    // 3. Output modified buffer
}
```

**Test**: Show we can modify text appearance in Notepad

**Deliverable**: Working prototype that changes text color

### 2.3 Implement Subpixel-Aware Shader

**Goal**: Apply custom subpixel logic

**Shader Logic** (HLSL):
```hlsl
// Input: RGB text texture from DirectWrite
// Input: Subpixel mask (loaded from PNG)
// Output: Resampled RGB values

Texture2D<float4> textTexture : register(t0);
Texture2D<float4> subpixelMask : register(t1);

float4 main(PSInput input) : SV_TARGET
{
    // Get original RGB values
    float4 original = textTexture.Sample(sampler, input.uv);
    
    // Read subpixel layout for this pixel
    float4 mask = subpixelMask.Sample(sampler, fmod(input.uv * screenSize, maskSize));
    
    // Redistribute color based on actual subpixel positions
    float3 adjusted;
    adjusted.r = original.r * mask.r; // Red channel where red subpixels are
    adjusted.g = original.g * mask.g; // Green channel where green subpixels are  
    adjusted.b = original.b * mask.b; // Blue channel where blue subpixels are
    
    return float4(adjusted, original.a);
}
```

**Deliverable**: Shader that improves WOLED text

## Phase 3: Production Implementation (3-4 months)

### 3.1 Robust Hooking System

**Requirements**:
- Safe injection that doesn't crash apps
- Whitelist/blacklist for apps (avoid games, fullscreen)
- Detect and disable if conflicts detected
- Clean shutdown without hanging

**Architecture**:
```
DisplayShadersPowerToy.exe (Manager)
??? Config UI (existing WPF app)
??? Service Manager
?   ??? Injects DLL into processes
?   ??? Monitors for new windows
??? DisplayShaderHook.dll (new)
    ??? DirectWrite hooks
    ??? Shader engine
    ??? Subpixel logic
```

### 3.2 Subpixel Mask System

**Features**:
- Load 32x32 or 64x64 PNG masks
- Built-in masks for common displays:
  - WOLED WRGB stripe
  - QD-OLED triangular
  - Pentile diamond
- Custom mask editor (optional)

**File Format** (JSON config):
```json
{
  "name": "LG WOLED 27GR95QE",
  "type": "WRGB_STRIPE",
  "maskFile": "woled_wrgb.png",
  "intensity": 0.8,
  "description": "LG UltraGear OLED"
}
```

### 3.3 Per-Monitor Configuration

**Features**:
- Detect connected displays
- Read EDID data (manufacturer, model)
- Auto-select appropriate mask
- Per-monitor intensity settings
- Switch on window focus change

**API**:
```csharp
// Query attached displays
var displays = DisplayDetector.GetAttachedDisplays();

foreach (var display in displays)
{
    // Try to auto-detect layout
    var layout = SubpixelDetector.DetectLayout(display.EdidData);
    
    // Load or prompt for configuration
    var config = ConfigManager.GetConfigForDisplay(display.DeviceId);
}
```

### 3.4 Performance Optimization

**Goals**:
- < 5% CPU overhead
- < 100 MB memory for shader system
- No perceivable latency
- Minimal battery impact on laptops

**Techniques**:
- Cache shader compilations
- Only process text regions (not images/video)
- Adaptive quality based on system load
- Suspend during fullscreen games

### 3.5 Compatibility & Safety

**Test Matrix**:
- Windows 10 21H2, 22H2
- Windows 11 21H2, 22H2, 23H2
- Different DPI settings (100%, 125%, 150%, 200%)
- Multiple monitors
- Various apps (Office, VS Code, Chrome, Firefox, etc.)

**Safety Features**:
- Automatic disable if app crashes
- Whitelist mode (only hook approved apps)
- Easy emergency disable (hotkey)
- Uninstall removes all hooks cleanly

## Phase 4: Testing & Refinement (2-3 months)

### 4.1 Beta Testing Program

**Recruit**:
- 50+ users with LG WOLED displays
- 50+ users with Samsung QD-OLED displays  
- Various Windows versions

**Collect**:
- Screenshot comparisons (before/after)
- Subjective ratings (readability, eye strain)
- Performance metrics
- Crash reports

### 4.2 Objective Measurements

**Metrics**:
- Color fringing reduction (image analysis)
- Subpixel accuracy (compare to expected)
- Text clarity scores
- Performance benchmarks

**Tools**:
- Macro photography of actual screen
- Color meter measurements
- RMSE between ideal and actual rendering

### 4.3 Refinement

**Based on feedback**:
- Adjust shader algorithms
- Tune default intensities
- Fix compatibility issues
- Improve performance
- Enhance UI/UX

## Phase 5: Release & Maintenance (Ongoing)

### 5.1 Official Release (v2.0)

**Features**:
- ? Actual display shader support
- ? WOLED RBG-equivalent rendering
- ? QD-OLED vertical fringing fix
- ? Per-monitor configuration
- ? Auto-detection of displays
- ? Custom subpixel masks

**Documentation**:
- Installation guide
- Troubleshooting
- Advanced configuration
- Developer API

### 5.2 Maintenance

**Ongoing tasks**:
- Windows Update compatibility
- New display support (masks for new models)
- Bug fixes
- Performance improvements
- Community support

### 5.3 Integration with PowerToys

**Goal**: Get official Microsoft adoption

**Steps**:
1. Prove technology works
2. Open PowerToys PR
3. Work with MS team on integration
4. Eventually deprecate standalone version

## Technical Challenges

### Challenge 1: Security Software

**Problem**: Antivirus/EDR may flag hooking as malicious

**Solutions**:
- Code signing certificate
- Transparency about hooking mechanism  
- Open source code for audit
- Whitelist requests with AV vendors

### Challenge 2: Game Anticheats

**Problem**: EasyAntiCheat, BattlEye may ban users

**Solutions**:
- Automatic disable for known games
- Whitelist-only mode option
- Clear warnings in documentation

### Challenge 3: Windows Updates

**Problem**: Major updates may break hooks

**Solutions**:
- Version detection and compatibility checks
- Graceful degradation to ClearType mode
- Rapid response team for updates
- Beta testing on Windows Insider builds

### Challenge 4: Performance

**Problem**: Shader on every text draw could be slow

**Solutions**:
- GPU acceleration (run shader on GPU)
- Caching rendered glyphs
- Only process visible text
- Adaptive quality levels

## Resources Required

### Development

**Skills needed**:
- C++ (DirectX, Win32 API hooking)
- HLSL (shader programming)
- C# (existing WPF app)
- Windows internals knowledge

**Team** (estimated):
- 1-2 senior developers (4-6 months full-time)
- OR 1 developer part-time (12-18 months)
- Beta testers (volunteers)

### Hardware

**For testing**:
- LG WOLED monitor (27GR95QE or similar)
- Samsung QD-OLED monitor (G80SD or similar)
- Standard LCD for comparison
- Multiple Windows 10/11 machines
- Macro camera for pixel-level photography

### Software

**Tools**:
- Visual Studio 2022
- RenderDoc (graphics debugging)
- PIX (DirectX profiler)
- Microsoft Detours (hooking library)

## Alternative: Simplified Approach

If full shader implementation is too complex, consider:

### Option A: Browser Extension

**Scope**: Only fix text in browsers (Chrome, Firefox, Edge)

**Approach**:
- Browser extension injects custom CSS
- Uses SVG filters for subpixel control
- Much simpler than system-wide hooks

**Limitation**: Only works in browsers

### Option B: PowerToys Contribution

**Scope**: Contribute idea/prototype to PowerToys

**Approach**:
- Work with Microsoft team directly
- They have resources and expertise
- Better integration with Windows

**Trade-off**: Less control, dependent on MS timeline

### Option C: Monitor Manufacturers

**Scope**: Lobby manufacturers to provide driver fixes

**Approach**:
- Contact LG, Samsung, ASUS
- Provide technical specification
- Request driver-level implementation

**Trade-off**: Slow, uncertain outcome

## Conclusion

**Implementing actual display shaders is:**
- ? Technically possible
- ?? Quite complex (6-12 months development)
- ?? Has compatibility risks (anticheat, security software)
- ?? Requires ongoing maintenance
- ? Would actually solve the OLED text problem
- ? Could become part of Windows/PowerToys

**Recommendation**:
1. Start with Phase 1 (honest rebranding) immediately
2. Begin Phase 2 research in parallel
3. Decide on full implementation after POC results
4. Consider partnering with PowerToys team

**Alternative path**:
- Focus on Phase 1 (honest tool)
- Contribute research to PowerToys
- Support Microsoft's implementation
- Maintain simple ClearType helper until then

---

**Status**: Draft roadmap  
**Last updated**: 2024  
**Next steps**: Community discussion on approach
