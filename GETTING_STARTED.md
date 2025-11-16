# ?? Getting Started with Display Shaders PowerToy

Welcome! This guide will help you get your OLED display looking its best in just a few minutes.

## ? Quick Start (30 seconds)

1. **Run the application** - Double-click `DisplayShadersPowerToy.exe`
2. **Select your display type:**
   - LG OLED? ? Choose "WRGB Stripe"
   - Samsung OLED? ? Choose "RGB Triangular"  
   - Regular LCD? ? Choose "RGB Stripe"
3. **Click "Apply"**
4. **Done!** Text should look better immediately

## ?? Which Display Do I Have?

### LG OLED Monitors (Choose WRGB Stripe)
- Alienware AW3423DW / AW3423DWF
- LG 27GR95QE-B
- LG 32GS95UE
- ASUS PG42UQ / PG48UQ
- Any monitor with "LG WOLED panel"

### Samsung QD-OLED Monitors (Choose RGB Triangular)
- Alienware AW3225QF / AW2725DF
- Samsung Odyssey OLED G8 (G85SB, G80SD)
- Samsung Odyssey OLED G9 (G95SC, G93SC)
- MSI MPG 321URX / MPG 271QRX
- ASUS PG32UCDM / PG27AQDP
- Dell UltraSharp 32 Plus 4K QD-OLED
- Any monitor with "Samsung QD-OLED panel"

### Standard LCD Monitors (Choose RGB Stripe)
- Most IPS, TN, VA panels
- If you're not sure, choose this

## ?? Recommended Settings

### For LG OLED (WOLED):
```
Subpixel Layout: WRGB Stripe
Enable Shader: ? Checked
Intensity: 80%
Start with Windows: ? Checked (recommended)
Minimize to tray: ? Checked (optional)
```

### For Samsung QD-OLED:
```
Subpixel Layout: RGB Triangular
Enable Shader: ? Checked
Intensity: 70%
Start with Windows: ? Checked (recommended)
Minimize to tray: ? Checked (optional)
```

### For Standard LCD:
```
Subpixel Layout: RGB Stripe
Enable Shader: ? Checked
Intensity: 100%
Start with Windows: ? Checked (optional)
Minimize to tray: ? Checked (optional)
```

## ?? What Should I See?

### Before (with color fringing):
```
Text has rainbow-colored edges
Letters look slightly blurry
Colors "bleed" around text
Eye strain after long reading
```

### After (optimized):
```
Clean, sharp text edges
No rainbow effect
Better readability
Less eye strain
```

## ?? Fine-Tuning

If text still doesn't look perfect:

### Text has color fringing?
- **Lower the intensity** - Try 60%, 50%, 40%
- Click "Apply" after each change
- Give your eyes 10 minutes to adjust

### Text looks too soft/blurry?
- **Raise the intensity** - Try 90%, 100%
- Or check monitor's sharpness setting
- Ensure Windows scaling is at 100%

### Some apps look fine, others don't?
- **Restart the app** - Many apps cache text rendering
- Browsers: Close all tabs and restart
- Office: Close and reopen documents
- Code editors: Restart the IDE

## ?? Pro Tips

1. **Restart apps** - Most apps need restart to see changes
2. **Give it time** - Your eyes may take 1-2 hours to adjust
3. **Test with real content** - Read actual documents, not just test screens
4. **Use 100% scaling** - Windows display scaling can affect clarity
5. **Check monitor settings** - Ensure monitor's sharpness isn't too high

## ?? Daily Use

### First Time Setup:
1. Configure your settings
2. Click "Apply"
3. Enable "Start with Windows"
4. Enable "Minimize to tray"
5. Click "Apply" again
6. Minimize the window (goes to tray)

### After Setup:
- Application runs in background
- Settings apply automatically on boot
- Double-click tray icon to adjust settings
- Right-click tray icon ? Exit to close

## ?? Common Tasks

### Change Settings:
1. Double-click system tray icon
2. Adjust settings
3. Click "Apply"

### Disable Temporarily:
1. Open application
2. Uncheck "Enable Shader"
3. Click "Apply"

### Reset to Defaults:
1. Open application
2. Select "RGB Stripe"
3. Set intensity to 100%
4. Click "Apply"

## ? Troubleshooting

### "I don't see any difference"
- ? Did you click "Apply"?
- ? Did you restart the application you're testing in?
- ? Is your monitor an OLED? Standard LCDs won't see much change
- ? Try adjusting intensity slider while viewing text

### "Text looks worse now"
- Try different subpixel layout
- Lower intensity to 50%
- Click "Apply" and test

### "Application won't start"
- Install .NET 8 Desktop Runtime from microsoft.com/dotnet
- Run as Administrator once
- Check antivirus isn't blocking it

### "Settings don't persist"
- Enable "Start with Windows"
- Run as Administrator once to set startup
- Check if another app is changing ClearType

## ?? More Information

- **Quick Reference:** See `QUICKSTART.md`
- **Detailed Config:** See `CONFIGURATION.md`
- **FAQ:** See `FAQ.md`
- **For Developers:** See `DEVELOPER.md`

## ?? Success Checklist

After setup, you should have:
- ? Selected correct subpixel layout for your monitor
- ? Adjusted intensity to your preference
- ? Clicked "Apply" to save settings
- ? Enabled "Start with Windows"
- ? Tested in your most-used applications
- ? Restarted those applications
- ? Given your eyes time to adjust

## ?? Display-Specific Quick Guides

### Alienware AW3423DW / DWF (WOLED)
```
Layout: WRGB Stripe
Intensity: 80%
Monitor Setting: Creator Mode or sRGB
Windows Scaling: 100%
```

### Alienware AW3225QF (QD-OLED)
```
Layout: RGB Triangular  
Intensity: 70%
Monitor Setting: Creator Mode
Windows Scaling: 100% or 150%
```

### Samsung Odyssey OLED G8
```
Layout: RGB Triangular
Intensity: 70%
Monitor Setting: sRGB mode
Windows Scaling: 100% or 125%
```

### LG 27GR95QE-B
```
Layout: WRGB Stripe
Intensity: 80%
Monitor Setting: sRGB or Gamer 1
Windows Scaling: 100% or 125%
```

## ?? Important Notes

### What This App Does:
? Optimizes Windows ClearType for OLED displays
? Reduces color fringing on text
? Improves text clarity and readability
? Applies settings automatically on startup

### What This App Doesn't Do:
? Fix monitor burn-in
? Improve game graphics
? Affect video playback
? Change monitor's physical characteristics
? Work miracles (it's just optimizing existing features!)

## ?? Understanding the Improvements

**What is ClearType?**
- Windows' text smoothing technology
- Uses colored subpixels to sharpen text
- Assumes standard RGB stripe layout

**Why OLED needs special settings:**
- WOLED adds white subpixel (WRGB instead of RGB)
- QD-OLED arranges subpixels in triangles
- Standard ClearType causes color fringing
- This app applies OLED-specific optimizations

**What the app changes:**
- ClearType contrast levels
- Gamma curves
- Orientation settings
- All via standard Windows registry and API

## ?? You're All Set!

Your OLED display should now render text beautifully. Enjoy your improved reading experience!

### Need Help?
- Check `FAQ.md` for common questions
- See `CONFIGURATION.md` for advanced tweaking
- Report issues on GitHub

### Want to Help?
- Share with other OLED users
- Report bugs or suggest features
- Contribute to the project

---

**Welcome to better text rendering!** ???

Made with ?? for the OLED community
Addressing PowerToys Issue #25595
