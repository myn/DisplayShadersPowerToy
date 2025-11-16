# System Tray Icon

The Display Shaders PowerToy system tray icon is generated programmatically using the `IconGenerator` helper class.

## Icon Design

The icon features:
- **Blue circular background** - Matches Microsoft's design language (#0078D4)
- **White monitor/screen** - Represents a display
- **RGB subpixel stripes** - Red, Green, and Blue vertical bars representing ClearType subpixel rendering
- **Monitor stand** - Simple white stand to complete the monitor icon

## Technical Details

- **Size**: 32x32 pixels
- **Format**: ICO (Icon)
- **Generation**: Created at runtime using System.Drawing
- **Location**: `Helpers/IconGenerator.cs`

## Features

- Anti-aliased graphics for smooth appearance
- Gradient background for depth
- High contrast for visibility in both light and dark system trays
- Represents the core functionality (display/ClearType optimization)

## Usage

The icon is automatically generated when the application starts. If generation fails, the app falls back to looking for `icon.ico` in the application directory.

To manually save the icon to a file:

```csharp
IconGenerator.SaveIconToFile("path/to/icon.ico");
```

## Customization

To modify the icon design, edit the `GenerateTrayIcon()` method in `Helpers/IconGenerator.cs`. The icon uses standard System.Drawing graphics primitives.
