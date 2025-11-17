# Subpixel Mask Files

This directory contains PNG mask files that define the subpixel structure for different display types.

## Format

Each mask file is a small PNG (typically 2x2, 3x1, or 4x1 pixels) where:
- **Red channel**: Where red subpixels are located (0-255)
- **Green channel**: Where green subpixels are located (0-255)
- **Blue channel**: Where blue subpixels are located (0-255)

The mask tiles/repeats across the screen.

## Files

### rgb_stripe.png (3x1 pixels)
Standard LCD RGB stripe layout:
```
Pixel 0: RGB(255, 0, 0)   ? Red subpixel
Pixel 1: RGB(0, 255, 0)   ? Green subpixel
Pixel 2: RGB(0, 0, 255)   ? Blue subpixel
```

### woled_wrgb.png (4x1 pixels)
WOLED WRGB stripe with RBG effect (Blue in middle):
```
Pixel 0: RGB(0, 0, 0)     ? White subpixel (ignored)
Pixel 1: RGB(255, 0, 0)   ? Red subpixel
Pixel 2: RGB(0, 0, 255)   ? Blue subpixel (MIDDLE!)
Pixel 3: RGB(0, 255, 0)   ? Green subpixel (RIGHT!)
```

### qdoled_triangular.png (2x2 pixels)
QD-OLED triangular layout:
```
Row 0: RGB(0,128,0) RGB(0,128,0)  ? Green at top
Row 1: RGB(255,0,0) RGB(0,0,255)  ? Red/Blue at bottom
```

### pentile.png (2x2 pixels)
Pentile diamond pattern:
```
Row 0: RGB(255,0,0) RGB(0,255,0)  ? R G
Row 1: RGB(0,255,0) RGB(0,0,255)  ? G B
```

## Creating Masks

Use any image editor (GIMP, Photoshop, Paint.NET):

1. Create new image with exact pixel dimensions
2. Set each pixel to the exact RGB values shown
3. Save as PNG-24 (no compression, no alpha)
4. Place in this directory

## Usage

The shader automatically loads the appropriate mask based on the selected subpixel layout.

## For Developers

To create a custom mask for a new display type:

1. Take macro photograph of display (1 pixel = multiple subpixels visible)
2. Identify subpixel pattern (where R, G, B subpixels are)
3. Create minimal repeating tile (e.g., 2x2, 4x1, 3x3)
4. Assign RGB values based on subpixel locations
5. Save as PNG in this directory
6. Update shader config to reference your mask

## Example: Custom Mask

If your display has this pattern:
```
  B
G   R
```

Create a 2x2 mask:
```
Row 0: RGB(0,0,128) RGB(0,0,128)   ? Blue at top center
Row 1: RGB(0,255,0) RGB(255,0,0)   ? Green left, Red right
```

---

**Note**: Actual PNG files need to be created using an image editor. This directory provides placeholders for the shader system to reference.
