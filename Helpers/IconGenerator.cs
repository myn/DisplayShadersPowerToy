using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace DisplayShadersPowerToy.Helpers;

/// <summary>
/// Generates icons for the application
/// </summary>
public static class IconGenerator
{
    /// <summary>
    /// Generate a system tray icon programmatically
    /// </summary>
    public static Icon GenerateTrayIcon()
    {
        // Create a 32x32 bitmap for the icon
        using var bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        
        // Enable anti-aliasing for smoother graphics
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        
        // Draw a modern monitor/display icon
        // Background circle with gradient
        using (var bgBrush = new LinearGradientBrush(
            new Rectangle(0, 0, 32, 32),
            Color.FromArgb(0, 120, 212),  // #0078D4 - Microsoft Blue
            Color.FromArgb(0, 90, 158),    // Darker blue
            LinearGradientMode.Vertical))
        {
            graphics.FillEllipse(bgBrush, 2, 2, 28, 28);
        }
        
        // Draw monitor screen (rectangle)
        using (var screenBrush = new SolidBrush(Color.White))
        {
            graphics.FillRectangle(screenBrush, 8, 8, 16, 12);
        }
        
        // Draw RGB subpixels (3 small colored rectangles to represent ClearType)
        graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0)), 10, 10, 2, 8);   // Red
        graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 255, 0)), 13, 10, 2, 8);   // Green
        graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 255)), 16, 10, 2, 8);   // Blue
        graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0)), 19, 10, 2, 8);   // Red
        
        // Draw monitor stand
        using (var standPen = new Pen(Color.White, 2))
        {
            graphics.DrawLine(standPen, 16, 20, 16, 24);
            graphics.DrawLine(standPen, 12, 24, 20, 24);
        }
        
        // Draw border around the circle
        using (var borderPen = new Pen(Color.FromArgb(0, 60, 100), 2))
        {
            graphics.DrawEllipse(borderPen, 2, 2, 28, 28);
        }
        
        // Convert bitmap to icon
        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        
        return icon;
    }
    
    /// <summary>
    /// Save the icon to a file
    /// </summary>
    public static void SaveIconToFile(string filePath)
    {
        using var icon = GenerateTrayIcon();
        using var fileStream = new FileStream(filePath, FileMode.Create);
        icon.Save(fileStream);
    }
}
