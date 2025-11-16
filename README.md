# Display Shaders PowerToy

A Windows utility that improves text rendering for OLED displays with non-standard subpixel layouts (WOLED/WRGB stripe and QD-OLED/RGB triangular).

## Problem Statement

This application addresses [PowerToys Issue #25595](https://github.com/microsoft/PowerToys/issues/25595), which highlights the text rendering issues on modern OLED displays:

- **WOLED displays** (like LG OLED monitors) use WRGB stripe subpixel layout
- **QD-OLED displays** (like Samsung QD-OLED monitors) use RGB triangular subpixel layout
- Windows ClearType is optimized for standard RGB stripe LCD displays
- Using standard ClearType on OLED displays causes chromatic aberration and color fringing on text edges

## Screenshot

![Display Shaders PowerToy Interface](Screenshot%202025-11-15%20221459.png)

## Features

- **Subpixel Layout Support**:
  - RGB Stripe (Standard LCD) - Default ClearType
  - WRGB Stripe (WOLED - LG OLED) - Optimized for white subpixel layouts
  - RGB Triangular (QD-OLED - Samsung) - Optimized for triangular arrangements
  - PenTile (Some AMOLED displays) - Optimized for pentile layouts
  - None (Disable ClearType) - Turn off subpixel rendering completely

- **Shader Controls**:
  - Enable/disable display shader
  - Adjustable shader intensity (0-100%)

- **Application Settings**:
  - Start with Windows
  - Minimize to system tray
  - System tray icon with quick access

## How It Works

The application modifies Windows ClearType settings and registry values to optimize text rendering for different subpixel layouts:

1. **Standard RGB Stripe**: Uses default ClearType settings optimized for LCD displays
2. **WOLED WRGB Stripe**: Reduces ClearType contrast to minimize color fringing caused by the white subpixel
3. **QD-OLED RGB Triangular**: Uses conservative settings to account for the triangular subpixel arrangement
4. **PenTile**: Balances settings for the diamond-shaped subpixel pattern

## System Requirements

- Windows 10/11
- .NET 8.0 Runtime
- Administrator privileges (for modifying ClearType settings)

## Installation

1. Download the latest release
2. Extract the files to a folder
3. Run `DisplayShadersPowerToy.exe`
4. Select your display's subpixel layout
5. Click "Apply" to save settings

## Usage

1. **Select Subpixel Layout**: Choose the layout that matches your display
   - Check your monitor specifications or manufacturer documentation
   - LG OLED monitors typically use WRGB Stripe
   - Samsung QD-OLED monitors typically use RGB Triangular

2. **Adjust Shader Intensity**: Fine-tune the effect to your preference
   - 100% = Full effect
   - Lower values = More subtle adjustments

3. **Enable Auto-Start**: Check "Start with Windows" to apply settings automatically on boot

4. **Minimize to Tray**: Keep the app running in the background

## Technical Details

The application modifies the following Windows settings:

- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothing`
- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothingType`
- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothingOrientation`
- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothingGamma`

It uses the Windows SystemParametersInfo API to apply changes system-wide.

## Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/DisplayShadersPowerToy.git

# Navigate to the project directory
cd DisplayShadersPowerToy

# Build the project
dotnet build

# Run the application
dotnet run
```

## Contributing

This is a community-driven project addressing a real need for OLED display users. Contributions are welcome!

## Credits

- Inspired by discussions in [PowerToys Issue #25595](https://github.com/microsoft/PowerToys/issues/25595)
- Based on the Blur Busters Open Source Display Shaders specification

## License

This project is provided as-is for the benefit of the Windows community.

## Disclaimer

This application modifies Windows ClearType settings. While these changes are safe and reversible, use at your own risk. You can always restore default settings by selecting "RGB Stripe" and applying, or by using Windows ClearType Tuner.

## Known Limitations

- Some applications may need to be restarted for changes to take full effect
- Changes are user-specific (not system-wide)
- Requires Windows 10 or later

## Future Enhancements

- Per-monitor settings for multi-display setups
- Advanced shader customization
- Real-time preview of text rendering changes
- Import/export settings profiles
- Integration with display detection for automatic layout selection
