# Production Deployment Guide

## Overview

This guide covers deploying Display Shaders PowerToy to production, including build verification, testing, and distribution.

## Prerequisites

- Windows 10/11 (x64)
- Visual Studio 2022 with C++ Desktop Development workload
- .NET 8.0 SDK
- Administrator privileges (for DLL injection testing)

## Build Process

### Step 1: Clean Build

```powershell
# Clean all build artifacts
.\build-production.ps1 -Clean

# Build both C# and Native projects
.\build-production.ps1 -Configuration Release -RunTests
```

Expected output:
```
[1/3] Building Native C++ Hook DLL...
  ? Native DLL built successfully
  
[2/3] Building C# Application...
  ? C# project built successfully
  ? Native DLL copied to output directory
  
[3/3] Running Tests...
  ... (test results)

? Build completed successfully!
Mode: Full Shader Mode
```

### Step 2: Verify Build Artifacts

Check these files exist:

```
bin\Release\net8.0-windows\
??? DisplayShadersPowerToy.exe    ? Main application
??? DisplayShaderHook.dll         ? Native hooks (critical!)
??? Hardcodet.NotifyIcon.Wpf.dll  ? System tray support
??? SubpixelMasks\
    ??? README.md                  ? Mask documentation
```

If `DisplayShaderHook.dll` is missing:
- Native C++ project didn't build
- Application will run in ClearType fallback mode
- Rebuild with Visual Studio 2022 C++ workload

### Step 3: Run Production Tests

```powershell
# Comprehensive test suite
.\test-production.ps1 -Configuration Release

# Integration tests (requires admin)
.\test-integration.ps1
```

All tests should pass:
```
? All tests passed!
System is ready for production deployment with full shader support!
```

## Deployment Options

### Option 1: Standalone Executable (Recommended)

Create a self-contained deployment:

```powershell
# Publish as single-file executable
dotnet publish -c Release -r win-x64 `
  -p:PublishSingleFile=false `
  -p:SelfContained=true `
  -p:IncludeNativeLibrariesForSelfExtract=true

# Output: bin\Release\net8.0-windows\win-x64\publish\
```

**Note**: `DisplayShaderHook.dll` must remain separate (cannot be embedded due to injection requirements).

### Option 2: MSI Installer

Use WiX Toolset or Advanced Installer:

1. **Include Files**:
   - `DisplayShadersPowerToy.exe`
   - `DisplayShaderHook.dll`
   - All dependencies from publish folder
   - `SubpixelMasks\` folder

2. **Installation Path**:
   ```
   C:\Program Files\DisplayShadersPowerToy\
   ```

3. **Registry Entries**:
   - App settings: `HKCU\SOFTWARE\DisplayShadersPowerToy`
   - Startup (optional): `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`

4. **Shortcuts**:
   - Start Menu: `Display Shaders PowerToy`
   - Desktop (optional)

### Option 3: MSIX Package

For Microsoft Store distribution:

```powershell
# Create MSIX package
dotnet publish -c Release -r win-x64 `
  -p:PublishProfile=win-x64 `
  -p:PackageType=MSIX
```

**Challenges**:
- DLL injection may be restricted in MSIX sandbox
- Consider AppX capabilities carefully
- May require special permissions

## Pre-Deployment Checklist

### Code Quality

- [ ] All tests pass (`test-production.ps1`)
- [ ] No compiler warnings
- [ ] No memory leaks (test with Application Verifier)
- [ ] Code signing certificate obtained
- [ ] Version number updated in project file

### Documentation

- [ ] README.md updated with release notes
- [ ] BUILD_INSTRUCTIONS.md verified
- [ ] User documentation complete
- [ ] Known issues documented

### Security

- [ ] Code signed with valid certificate
- [ ] Antivirus false positive testing done
- [ ] Security audit completed
- [ ] No hardcoded credentials or secrets

### Functionality

- [ ] Shader mode works on test system
- [ ] ClearType fallback works
- [ ] Settings persist across restarts
- [ ] System tray integration works
- [ ] All subpixel layouts tested

## Code Signing

### Obtain Certificate

Purchase from trusted CA (DigiCert, Sectigo, etc.):
- Standard Code Signing Certificate
- EV Code Signing (recommended for driver-like components)

### Sign DLL and EXE

```powershell
# Sign native DLL
signtool sign /f certificate.pfx /p PASSWORD `
  /t http://timestamp.digicert.com `
  /fd SHA256 `
  bin\x64\Release\DisplayShaderHook.dll

# Sign main executable  
signtool sign /f certificate.pfx /p PASSWORD `
  /t http://timestamp.digicert.com `
  /fd SHA256 `
  bin\Release\net8.0-windows\DisplayShadersPowerToy.exe

# Verify signatures
signtool verify /pa DisplayShadersPowerToy.exe
signtool verify /pa DisplayShaderHook.dll
```

## Antivirus Whitelisting

### Submit to AV Vendors

Before public release:

1. **Microsoft Defender**
   - https://www.microsoft.com/en-us/wdsi/filesubmission
   - Explain: "Legitimate DirectWrite hook for text rendering"

2. **Other Vendors**
   - Submit through their developer portals
   - Provide:
     - Signed binaries
     - Description of DLL injection purpose
     - Your contact information

### Common False Positives

Expected triggers:
- `CreateRemoteThread` API call
- DLL injection behavior
- Process memory modification

Mitigation:
- Code signing
- Detailed documentation
- Vendor whitelisting

## Distribution

### GitHub Releases

1. Create release tag:
   ```bash
   git tag -a v2.0.0 -m "Production release with real shaders"
   git push origin v2.0.0
   ```

2. Upload artifacts:
   - `DisplayShadersPowerToy-v2.0.0-x64.zip`
   - `DisplayShadersPowerToy-v2.0.0-Setup.msi` (if created)
   - `CHANGELOG.md`
   - `README.md`

3. Release notes template:
   ```markdown
   ## Display Shaders PowerToy v2.0.0
   
   ### New Features
   - ? Real DirectWrite/D3D shader hooks
   - ? WOLED WRGB fix (Blue in middle)
   - ? QD-OLED triangular fix
   - ? Dual-mode operation (shaders + ClearType fallback)
   
   ### Requirements
   - Windows 10/11 (x64)
   - Administrator privileges (for DLL injection)
   - .NET 8.0 Runtime (included in self-contained build)
   
   ### Installation
   1. Download zip file
   2. Extract to desired location
   3. Run DisplayShadersPowerToy.exe as Administrator
   4. Select your display type
   5. Click Apply
   
   ### Known Issues
   - Antivirus may flag DLL injection as suspicious (whitelist if needed)
   - Some applications may need restart for changes to take effect
   ```

### Website/Documentation

Host on GitHub Pages:
- Installation guide
- Quick start tutorial
- FAQ
- Screenshots/videos
- Support information

## Post-Deployment

### Monitoring

Set up monitoring for:
- Download statistics
- Issue reports
- User feedback
- Crash reports (if telemetry enabled)

### Updates

Establish update mechanism:
- Auto-update check on startup
- Manual update via GitHub releases
- Update notifications

### Support

Provide support channels:
- GitHub Issues (bug reports)
- GitHub Discussions (questions)
- Discord/Slack (community)
- Email (critical issues)

## Troubleshooting Deployment Issues

### DLL Not Loading

**Symptom**: App shows "Shader Mode: Not Available"

**Causes**:
1. Native DLL not copied to output
2. Missing dependencies (d3d11.dll, dwrite.dll)
3. Architecture mismatch (x86 vs x64)

**Fix**:
```powershell
# Verify DLL exists
Test-Path bin\Release\net8.0-windows\DisplayShaderHook.dll

# Check dependencies
dumpbin /DEPENDENTS DisplayShaderHook.dll
```

### Injection Fails

**Symptom**: DLL doesn't inject into target processes

**Causes**:
1. Not running as Administrator
2. Target process is protected
3. Antivirus blocking

**Fix**:
- Always run as Administrator
- Check antivirus logs
- Test with notepad.exe first

### Crashes on Startup

**Symptom**: Application crashes immediately

**Causes**:
1. Missing .NET 8.0 Runtime
2. Corrupted binaries
3. Incompatible Windows version

**Fix**:
```powershell
# Check .NET version
dotnet --list-runtimes

# Verify EXE integrity
signtool verify /pa DisplayShadersPowerToy.exe
```

## Rollback Procedure

If deployment fails:

1. **User Side**:
   ```powershell
   # Uninstall
   Remove-Item "C:\Program Files\DisplayShadersPowerToy" -Recurse
   
   # Clean registry
   Remove-Item "HKCU:\SOFTWARE\DisplayShadersPowerToy" -Recurse
   
   # Restore ClearType defaults
   # (built into app - click "Reset")
   ```

2. **Developer Side**:
   ```bash
   # Rollback to previous version
   git revert HEAD
   
   # Delete bad release
   gh release delete v2.0.0
   ```

## Performance Benchmarks

Establish baseline metrics:

- **CPU Usage**: < 1% idle, < 5% during rendering
- **Memory**: < 50 MB working set
- **Startup Time**: < 2 seconds
- **Hook Overhead**: < 100 ?s per glyph run
- **Battery Impact**: < 1% additional drain

Monitor these post-deployment and optimize as needed.

## Success Criteria

Deployment is successful when:

? Installers work on clean Windows 10/11
? No antivirus false positives (after whitelisting)
? Shader mode activates correctly
? Text rendering improves on WOLED/QD-OLED
? No crashes in first 24 hours of use
? Positive user feedback
? < 5% bug report rate

---

**Next Steps**: See `PRODUCTION_CHECKLIST.md` for final pre-release verification.
