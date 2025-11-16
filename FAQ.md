# Frequently Asked Questions (FAQ)

## General Questions

### What is Display Shaders PowerToy?

Display Shaders PowerToy is a free utility that fixes text rendering issues on OLED monitors. It optimizes Windows ClearType for displays with non-standard subpixel layouts, eliminating color fringing and improving text clarity.

### Do I need this application?

You need this if you have:
- LG WOLED monitor (WRGB subpixels) - e.g., AW3423DW, 27GR95QE
- Samsung QD-OLED monitor (triangular RGB) - e.g., AW3225QF, Odyssey OLED
- Any display showing rainbow/color fringing on text edges

You probably don't need this if you have:
- Standard LCD monitor (IPS, TN, VA)
- No visible text rendering issues

### Is it safe to use?

Yes, completely safe. The application only modifies Windows ClearType settings, which are:
- User-level registry entries (not system-wide)
- Easily reversible
- The same settings you could change manually
- No system files are modified

### Will it slow down my computer?

No. The application:
- Uses ~15 MB of RAM
- < 1% CPU when running
- Only applies settings once, no continuous processing
- Can be closed after applying settings (if not using startup feature)

## Installation & Setup

### How do I install it?

1. Download the latest release
2. Extract to any folder
3. Run `DisplayShadersPowerToy.exe`
4. No installer needed - it's portable

### Do I need administrator rights?

No, for normal use. Administrator is only needed if:
- You want to enable "Start with Windows" (one-time)
- Registry permissions are restricted on your PC

### Can I run it from a USB drive?

Yes, it's fully portable. Settings are saved to Windows Registry, so they persist even if you move the application.

### How do I uninstall it?

1. Open the application
2. Select "RGB Stripe" (standard settings)
3. Click "Apply"
4. Uncheck "Start with Windows"
5. Click "Apply" again
6. Close the application
7. Delete the application folder

## Usage Questions

### Which setting should I choose?

**LG OLED monitors:**
- AW3423DW, AW3423DWF ? WRGB Stripe
- 27GR95QE, 32GS95UE ? WRGB Stripe
- Any LG WOLED ? WRGB Stripe

**Samsung QD-OLED monitors:**
- AW3225QF, AW2725DF ? RGB Triangular
- Odyssey OLED G8/G9 ? RGB Triangular
- Samsung S90C, S95C ? RGB Triangular

**Standard monitors:**
- Most IPS/TN/VA panels ? RGB Stripe
- Not sure? ? Try RGB Stripe first

### What's the best intensity setting?

**Start with these:**
- WOLED: 80%
- QD-OLED: 70%
- Standard LCD: 100%

**Then adjust:**
- See color fringing? Lower by 10%
- Text too soft? Raise by 10%
- Give your eyes 1-2 hours to adjust before changing

### Do I need to restart my computer?

No, but you should:
- Restart applications you're currently using
- Or log out and back in for best results
- Full system restart not required

### Changes don't seem to apply?

Try these steps in order:

1. **Restart the application** you're testing in
2. **Log out and back in** to Windows
3. **Check you clicked "Apply"** in Display Shaders PowerToy
4. **Verify correct layout selected** for your monitor
5. **Check Windows scaling** (100% works best)

### Can I use this with multiple monitors?

Yes, but with limitations:
- Settings apply to ALL monitors
- Cannot configure per-monitor (yet)
- Choose settings for your primary/main monitor

**Workaround for mixed setups:**
- Use settings for the monitor you use most
- Or choose a middle-ground setting (e.g., 80% intensity)

## Technical Questions

### What exactly does it change?

The application modifies these Windows registry values:
```
HKEY_CURRENT_USER\Control Panel\Desktop\
??? FontSmoothing
??? FontSmoothingType
??? FontSmoothingOrientation
??? FontSmoothingGamma
```

These control how Windows renders text with ClearType anti-aliasing.

### Does it affect gaming or video playback?

No. ClearType only affects text rendering. It does not impact:
- Game performance or graphics
- Video playback quality
- Image display
- 3D rendering

### Can I use this with Windows ClearType Tuner?

Yes, but:
- Display Shaders PowerToy should be applied LAST
- If you run ClearType Tuner after, it may override settings
- Best to use one or the other, not both

### Does it work on all Windows versions?

Tested and working on:
- ? Windows 11 (all versions)
- ? Windows 10 (all versions)
- ?? Windows 8.1 (should work, not tested)
- ? Windows 7 (may work but unsupported)

### Can I use this on a laptop?

Yes, if your laptop has an OLED screen:
- ASUS Zenbook OLED ? Depends on panel (usually RGB Stripe or WRGB)
- Dell XPS OLED ? Usually WRGB Stripe
- Samsung Galaxy Book ? Usually RGB Triangular (QD-OLED)

Check your laptop specifications for panel type.

## Troubleshooting

### Text is now blurry

**Causes:**
- Intensity too low
- Wrong subpixel layout selected
- Monitor sharpness setting too low

**Solutions:**
1. Increase shader intensity to 90-100%
2. Try a different subpixel layout
3. Check monitor's built-in sharpness setting
4. Reset to defaults: RGB Stripe, 100%, then apply

### Text has weird colors/rainbow effect

**Causes:**
- Intensity too high for your display
- Wrong subpixel layout

**Solutions:**
1. Lower intensity to 50-60%
2. For WOLED: Use WRGB Stripe, not RGB Stripe
3. For QD-OLED: Use RGB Triangular, not RGB Stripe
4. Try "None" to disable ClearType completely

### Some apps look fine, others don't

**Explanation:**
Some applications use their own text rendering:
- Browsers usually respect ClearType
- Some code editors use custom rendering
- Games often render text differently

**Solutions:**
1. Restart the problematic application
2. Check app's font rendering settings
3. Some apps need to be configured separately

### Settings reset after Windows update

**Cause:**
Some Windows updates reset ClearType settings

**Solution:**
1. Re-open Display Shaders PowerToy
2. Your settings are saved, just click "Apply" again
3. Enable "Start with Windows" to reapply automatically

### Application won't start

**Troubleshooting:**
1. **Check .NET 8 is installed**
   - Download from microsoft.com/dotnet
   - Need "Desktop Runtime"

2. **Run as Administrator** (one time)
   - Right-click ? Run as administrator

3. **Check antivirus**
   - Add to exclusions if blocked

4. **Try different location**
   - Don't run from Downloads or temp folders
   - Extract to C:\Program Files\DisplayShadersPowerToy

### System tray icon doesn't appear

**Causes:**
- "Minimize to tray" not enabled
- Windows hiding the icon

**Solutions:**
1. Enable "Minimize to tray" in settings
2. Check Windows taskbar settings
3. Look in hidden icons (^ arrow in taskbar)
4. Restart the application

### "Start with Windows" doesn't work

**Causes:**
- Insufficient permissions
- Antivirus blocking registry changes

**Solutions:**
1. Run application as Administrator once
2. Enable the setting again
3. Check Windows Task Manager ? Startup tab
4. Manually add shortcut to startup folder:
   - Press Win+R
   - Type: shell:startup
   - Copy shortcut there

## Comparison Questions

### How is this different from Windows ClearType Tuner?

**Windows ClearType Tuner:**
- Built into Windows
- Only optimizes for RGB stripe
- Cannot fix OLED issues
- Wizard-based interface

**Display Shaders PowerToy:**
- Specifically for OLED displays
- Supports WRGB and triangular layouts
- Optimized settings per display type
- Simple toggle interface

### Should I use this or MacType?

**MacType:**
- More aggressive text rendering changes
- Can affect system stability
- Requires driver installation
- More complex setup

**Display Shaders PowerToy:**
- Only modifies ClearType settings
- Safe, native Windows approach
- Simple, lightweight
- Specific to OLED displays

Use Display Shaders PowerToy if you just want to fix OLED text rendering.

### Can I use this with Better ClearType Tuner?

They conflict. Choose one:
- **Display Shaders PowerToy**: For OLED-specific issues
- **Better ClearType Tuner**: For general ClearType customization

Don't use both simultaneously.

## Advanced Questions

### Can I edit settings manually?

Yes, settings are in:
```
HKEY_CURRENT_USER\SOFTWARE\DisplayShadersPowerToy
```

But using the application is recommended for safety.

### What if I have a different subpixel layout?

Currently supports:
- RGB Stripe
- WRGB Stripe  
- RGB Triangular
- PenTile

For other layouts:
- Try the closest match
- Or select "None" and use grayscale anti-aliasing
- Request support via GitHub issue

### Can I contribute or request features?

Yes! This is open source:
- Report issues on GitHub
- Request features via GitHub issues
- Submit pull requests
- See DEVELOPER.md for contribution guide

### Is there a command-line interface?

Currently only:
```
DisplayShadersPowerToy.exe --minimized
```

Full CLI planned for future version.

### How do I report bugs?

1. Check this FAQ first
2. Check GitHub issues
3. Create new issue with:
   - Your display model
   - Windows version
   - Steps to reproduce
   - Screenshots if applicable

## Performance Questions

### Does it use a lot of resources?

No:
- **RAM:** ~15 MB
- **CPU:** <1% (idle)
- **Disk:** ~5 MB
- **Startup:** <1 second

### Should I keep it running?

Options:

**Keep running if:**
- You use "Start with Windows"
- You want tray icon access
- You change settings frequently

**Can close if:**
- You only change settings rarely
- Settings are already applied
- You want to minimize resource usage

Settings persist even after closing.

### Does it affect battery life on laptops?

No measurable impact:
- Settings apply once, then idle
- No continuous processing
- No network activity
- Negligible power usage

## Still Have Questions?

- Check the README.md
- Read CONFIGURATION.md for detailed setup
- See DEVELOPER.md for technical details
- Open an issue on GitHub

---

**Last Updated:** January 24, 2025  
**Version:** 1.0.0
