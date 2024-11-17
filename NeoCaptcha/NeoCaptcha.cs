using SkiaSharp;

namespace NeoCaptcha;

public class Captcha
{
    public Captcha(CaptchaOptions options)
    {
        Text = GenerateRandomText(options.CharacterCount).ToUpperInvariant();
        ImageAsByteArray = CreateCaptchaImage(Text, options);
    }

    public string Text { get; }
    public byte[] ImageAsByteArray { get; }

    private static string GenerateRandomText(int characterCount)
    {
        const string chars = "ABCEFGHJKNPRSTUVXYZabcdefhkmnrstuvwxz123456789";
        return new string(Enumerable.Range(0, characterCount)
            .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }

    private static byte[] CreateCaptchaImage(string text, CaptchaOptions options)
    {
        using var surface = SKSurface.Create(new SKImageInfo(options.Width, options.Height));
        var canvas = surface.Canvas;

        DrawBackground(canvas, options);
        DrawText(canvas, text, options);
        AddNoise(canvas, options);

        using var image = surface.Snapshot();
        using var data = image.Encode(
            options.ImageFormat == CaptchaImageFormat.JPG ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png,
            100);
        return data.ToArray();
    }

    private static void DrawBackground(SKCanvas canvas, CaptchaOptions options)
    {
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(options.Width, options.Height),
                new[] { SKColors.LightGray, SKColors.White },
                null,
                SKShaderTileMode.Clamp
            )
        };
        canvas.Clear(SKColors.White);
        canvas.DrawRect(new SKRect(0, 0, options.Width, options.Height), paint);

        // Add background noise if enabled
        if (options.IsBackgroundNoiseEnabled)
        {
            AddBackgroundNoise(canvas, options);
        }
    }

    private static void AddBackgroundNoise(SKCanvas canvas, CaptchaOptions options)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            StrokeWidth = 1.0f
        };

        // Add tiny multicolor dots to the background
        for (var i = 0; i < options.Width * options.Height / 500; i++) // Adjust the density
        {
            paint.Color = GetRandomColor(); // Random color for each dot
            float x = Random.Shared.Next(options.Width);
            float y = Random.Shared.Next(options.Height);
            canvas.DrawCircle(new SKPoint(x,y),1,paint); // Draw small dots
        }
    }

    private static void DrawText(SKCanvas canvas, string text, CaptchaOptions options)
    {
        var charSpacing = options.Width / (float)text.Length;

        for (var i = 0; i < text.Length; i++)
        {
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.TextSize = Random.Shared.Next(22, 30);
            paint.Typeface = SKTypeface.FromFamilyName(
                GetRandomFont(),
                SKFontStyleWeight.SemiBold,
                SKFontStyleWidth.ExtraCondensed,
                SKFontStyleSlant.Italic
            );
            paint.Color = options.IsMultiColorText ? GetRandomColor() : SKColors.Gray;

            var x = 10 + i * charSpacing;
            var y = options.Height / 2 + paint.TextSize / 2 - 5;

            // If random rotation is enabled, rotate each character
            if (options.IsRandomRotation)
            {
                var rotationAngle = Random.Shared.Next(-30, 31); // Random rotation between -30 and +30 degrees
                canvas.Save();
                canvas.RotateDegrees(rotationAngle, x + paint.TextSize / 2, y); // Rotate around the character's center
            }

            canvas.DrawText(text[i].ToString(), x, y, paint);

            // Restore the canvas state after drawing the rotated character
            if (options.IsRandomRotation)
            {
                canvas.Restore();
            }
        }
    }

    private static void AddNoise(SKCanvas canvas, CaptchaOptions options)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.StrokeWidth = 1.5f;

        for (var i = 0; i < options.CharacterCount * 3; i++)
        {
            paint.Color = GetRandomGrayColor();
            canvas.DrawLine(
                Random.Shared.Next(options.Width),
                Random.Shared.Next(options.Height),
                Random.Shared.Next(options.Width),
                Random.Shared.Next(options.Height),
                paint);
        }
    }

    private static SKColor GetRandomColor()
    {
        return new SKColor(
            (byte)Random.Shared.Next(0, 256),
            (byte)Random.Shared.Next(0, 256),
            (byte)Random.Shared.Next(0, 256));
    }

    private static SKColor GetRandomGrayColor()
    {
        var gray = (byte)Random.Shared.Next(50, 200);
        return new SKColor(gray, gray, gray);
    }

    private static string GetRandomFont()
    {
        string[] fonts = { "Arial", "Courier New", "Calibri", "Tahoma" };
        return fonts[Random.Shared.Next(fonts.Length)];
    }

    public static class ImageHelper
    {
        public static void SaveImageToFile(byte[] imageBytes, string filePath)
        {
            if (imageBytes is null || imageBytes.Length == 0)
                throw new ArgumentException("Image bytes cannot be null or empty", nameof(imageBytes));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            try
            {
                File.WriteAllBytes(filePath, imageBytes);
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to save the image file.", ex);
            }
        }
    }
}

public record CaptchaOptions
{
    public int CharacterCount { get; set; } = 6;
    public int Width { get; set; } = 220;
    public int Height { get; set; } = 70;
    public CaptchaImageFormat ImageFormat { get; set; } = CaptchaImageFormat.PNG;
    public bool IsMultiColorText { get; set; } = false;
    public bool IsRandomRotation { get; set; } = false; 
    public bool IsBackgroundNoiseEnabled { get; set; } = false; 
}

public enum CaptchaImageFormat
{
    PNG,
    JPG
}
