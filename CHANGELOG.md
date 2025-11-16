# Changelog

All notable changes to Display Shaders PowerToy will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-24

### Added
- Initial release of Display Shaders PowerToy
- Support for multiple subpixel layouts:
  - RGB Stripe (Standard LCD)
  - WRGB Stripe (WOLED - LG OLED)
  - RGB Triangular (QD-OLED - Samsung)
  - PenTile (AMOLED displays)
  - None (Disable ClearType)
- Adjustable shader intensity slider (0-100%)
- System tray integration with minimize to tray support
- Start with Windows functionality
- Settings persistence via Windows Registry
- Automatic ClearType adjustment based on subpixel layout
- Support for minimized startup (--minimized command line arg)
- Settings saved per user in registry
- Context menu in system tray (Open, Exit)

### Technical Details
- Built with .NET 8.0 and WPF
- Uses Windows SystemParametersInfo API for ClearType control
- Registry-based settings storage
- Optimized ClearType parameters for each display type:
  - WRGB Stripe: Reduced contrast (800) to minimize color fringing
  - RGB Triangular: Conservative settings (600) for triangular layout
  - PenTile: Balanced settings (700) for diamond pattern
  - RGB Stripe: Standard ClearType (1400)

### Known Issues
- Application icon not included in initial release (uses default)
- Some applications may require restart for changes to take effect
- Settings are user-specific, not system-wide

### Addresses
- PowerToys Issue #25595: Improved subpixel text rendering for OLED displays
- 783+ upvotes on GitHub issue
- Community-requested feature for WOLED and QD-OLED support

## [Unreleased]

### Planned Features
- Custom application icon
- Per-monitor settings for multi-display setups
- Real-time text rendering preview
- Import/export settings profiles
- Automatic display detection
- Advanced shader customization options
- Windows 11 settings integration
- Installer package
- Auto-update functionality

### Under Consideration
- GPU-based display shaders (DirectX overlay)
- Gamma curve adjustments
- Custom subpixel pattern support
- Third-party application integration
- Telemetry for usage statistics (opt-in)
