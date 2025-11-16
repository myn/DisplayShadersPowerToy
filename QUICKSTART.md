# Quick Start Guide - Display Shaders PowerToy

## What This Application Does

This application fixes text rendering issues on OLED displays that use non-standard subpixel layouts. Windows ClearType is designed for standard RGB stripe LCD displays, which causes color fringing and blurry text on:

- **WOLED displays** (LG OLED monitors) - Use WRGB Stripe setting
- **QD-OLED displays** (Samsung OLED monitors) - Use RGB Triangular setting

## How to Use

### First-Time Setup

1. **Launch the application**
   - Run `DisplayShadersPowerToy.exe`
   - The main window will appear

2. **Select Your Display Type**
   - **RGB Stripe (Standard LCD)** - For normal LCD monitors
   - **WRGB Stripe (WOLED - LG OLED)** - For LG OLED monitors (AW3423DWF, etc.)
   - **RGB Triangular (QD-OLED - Samsung)** - For Samsung QD-OLED monitors (AW3225QF, etc.)
   - **PenTile** - For some AMOLED displays
   - **None** - Disables ClearType completely

3. **Adjust Settings**
   - Enable/disable the shader
   - Adjust intensity slider (100% = full effect, lower = more subtle)

4. **Configure Startup**
   - Check "Start with Windows" to apply settings automatically on boot
   - Check "Minimize to system tray" to keep it running in the background

5. **Click "Apply"**
   - Settings will be saved and applied immediately
   - You may need to restart some applications for full effect

### Finding Your Display Type

**LG OLED Monitors (WOLED)**
- Models: AW3423DW, AW3423DWF, 27GR95QE-B, 32GS95UE, etc.
- Use: **WRGB Stripe**

**Samsung QD-OLED Monitors**
- Models: AW3225QF, Odyssey OLED G8/G9, S90C, etc.
- Use: **RGB Triangular**

**Standard LCD Monitors**
- Most IPS, TN, VA panels
- Use: **RGB Stripe**

### Checking Results

After applying settings:

1. Open a text editor or web browser
2. Look at small text (8-12pt)
3. You should see:
   - Sharper text edges
   - Reduced color fringing (rainbow effect)
   - Better overall clarity

### System Tray Icon

When minimized to tray:
- **Double-click** the tray icon to open settings
- **Right-click** for menu:
  - Open - Show settings window
  - Exit - Close the application

### Recommended Settings by Display Type

**LG OLED (WOLED/WRGB Stripe)**
- Subpixel Layout: WRGB Stripe
- Enable Shader: Yes
- Intensity: 70-100% (adjust to preference)

**Samsung QD-OLED (RGB Triangular)**
- Subpixel Layout: RGB Triangular
- Enable Shader: Yes
- Intensity: 60-80% (adjust to preference)

**Standard LCD**
- Subpixel Layout: RGB Stripe
- Enable Shader: Yes
- Intensity: 100%

## Troubleshooting

### Text looks blurry after applying
- Try lowering the shader intensity
- Make sure you selected the correct subpixel layout for your display
- Restart the application you're viewing text in

### Settings don't seem to apply
- Make sure to click "Apply" button
- Try logging out and back in to Windows
- Some applications cache font rendering - restart them

### How to reset to defaults
- Select "RGB Stripe (Standard LCD)"
- Set intensity to 100%
- Click "Apply"
- Or run Windows ClearType Tuner: search "ClearType" in Windows Start menu

### Application won't start with Windows
- Run the app as Administrator once and enable "Start with Windows"
- Check Windows Task Manager > Startup tab

## Technical Notes

The application modifies these Windows registry keys:
- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothing`
- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothingType`
- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothingOrientation`
- `HKEY_CURRENT_USER\Control Panel\Desktop\FontSmoothingGamma`

All changes are user-specific and can be reverted.

## Advanced Usage

### Command Line Arguments

Start minimized (useful for startup):
```
DisplayShadersPowerToy.exe --minimized
```

### Settings Location

Settings are stored in the Windows Registry:
```
HKEY_CURRENT_USER\SOFTWARE\DisplayShadersPowerToy
```

## Support

For issues, improvements, or questions:
- See README.md for more details
- Check PowerToys Issue #25595 on GitHub

## Version

Version 1.0.0 - Initial Release
Addresses PowerToys Issue #25595
