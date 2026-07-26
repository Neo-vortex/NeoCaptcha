using SkiaSharp;

namespace NeoCaptcha;

public class Captcha
{
    public Captcha(CaptchaOptions options)
    {
        ValidateOptions(options);
        Text = GenerateRandomText(options.CharacterCount).ToUpperInvariant();
        ImageAsByteArray = CreateCaptchaImage(Text, options);
    }

    private static void ValidateOptions(CaptchaOptions options)
    {
        if (options.CharacterCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "CharacterCount must be greater than zero.");
        if (options.Width <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Width must be greater than zero.");
        if (options.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Height must be greater than zero.");
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
        if (options.UseRandomLineNoise) AddNoise(canvas, options);

        using var image = surface.Snapshot();
        using var data = image.Encode(
            options.ImageFormat == CaptchaImageFormat.JPG ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png,
            100);
        return data.ToArray();
    }

    private static void DrawBackground(SKCanvas canvas, CaptchaOptions options)
    {
        using var gradientPaint = new SKPaint
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

        // SaveLayer's paint (with the blur mask filter) only affects content
        // drawn between SaveLayer and the matching Restore. Previously nothing
        // was drawn in between, so the blur had no visible effect at all.
        int? layerCount = null;
        SKPaint? blurPaint = null;
        if (options.IsBlurringEnabled)
        {
            blurPaint = new SKPaint { MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5) };
            layerCount = canvas.SaveLayer(blurPaint);
        }

        canvas.DrawRect(new SKRect(0, 0, options.Width, options.Height), gradientPaint);

        if (options.IsBackgroundNoiseEnabled)
        {
            AddBackgroundNoise(canvas, options);
        }

        if (layerCount.HasValue)
        {
            canvas.RestoreToCount(layerCount.Value);
            blurPaint!.Dispose();
        }
    }

    private static void AddBackgroundNoise(SKCanvas canvas, CaptchaOptions options)
    {
        using var paint = new SKPaint();

        for (var y = 0; y < options.Height; y++)
        {
            for (var x = 0; x < options.Width; x++)
            {
                if ((x % 3 != 0) || (y % 3 != 0)) continue; // Apply noise every 3 pixels
                paint.Color = GetRandomGrayColor();
                canvas.DrawPoint(x, y, paint);
            }
        }
    }

    private static void DrawText(SKCanvas canvas, string text, CaptchaOptions options)
    {
        var charSpacing = options.Width / (float)text.Length;

        for (var i = 0; i < text.Length; i++)
        {
            using var paint = new SKPaint
            {
                IsAntialias = true,
                TextSize = Random.Shared.Next(22, 30),
                Typeface = GetCachedTypeface(GetRandomFont()),
                Color = options.IsMultiColorText ? GetRandomColor() : SKColors.Gray
            };

            var x = 10 + i * charSpacing;
            var y = options.Height / 2 + paint.TextSize / 2 - 5;

            // If random rotation is enabled, rotate each character
            if (options.IsRandomRotation)
            {
                var rotationAngle = Random.Shared.Next(-30, 31); // Random rotation between -30 and +30 degrees
                canvas.Save();
                canvas.RotateDegrees(rotationAngle, x + paint.TextSize / 2, y);
            }

            canvas.DrawText(text[i].ToString(), x, y, paint);

            // Restore the canvas state after drawing the rotated character
            if (options.IsRandomRotation)
            {
                canvas.Restore();
            }

            // Apply blur to text if enabled
            if (options.IsBlurringEnabled)
            {
                paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2);
                canvas.DrawText(text[i].ToString(), x, y, paint);
            }
        }
    }

    private static void AddNoise(SKCanvas canvas, CaptchaOptions options)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            StrokeWidth = 1.5f
        };

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

    // SKTypeface.FromFamilyName hits the underlying font system (fontconfig on
    // Linux) on every call. That was happening once per character, per captcha,
    // which adds unnecessary latency under load. Typefaces are immutable and
    // safe to share/cache for the process lifetime.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SKTypeface> TypefaceCache = new();

    private static SKTypeface GetCachedTypeface(string fontFamily)
    {
        return TypefaceCache.GetOrAdd(fontFamily, static family => SKTypeface.FromFamilyName(
            family,
            SKFontStyleWeight.SemiBold,
            SKFontStyleWidth.ExtraCondensed,
            SKFontStyleSlant.Italic
        ));
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
    public bool IsBlurringEnabled { get; set; } = false;
    public bool UseRandomLineNoise { get; set; } = true;
}

public enum CaptchaImageFormat
{
    PNG,
    JPG
}
