
# NeoCaptcha - A Captcha Generation Library

## Overview

**NeoCaptcha** is a lightweight, efficient captcha generation library designed for use across multiple platforms. The library leverages SkiaSharp for rendering and provides a flexible solution for captcha creation in both .NET Core and ASP.NET Core environments.

NeoCaptcha generates captchas that can be used to validate user inputs, and includes noise elements like random lines to enhance the complexity of the captcha images.

## Features
- Multiplatform support via SkiaSharp.
- Customizable captcha text generation.
- Optional multi-color text for visual diversity.
- Simple API for integration with ASP.NET Core.
- Image format options (PNG/JPG).
- Noise generation in the form of random lines.

## Getting Started

To use NeoCaptcha in your project, you can follow the steps outlined for both the base usage and integration with ASP.NET Core.

---

## 1. Base Usage (Non-ASP.NET Core)

### Installing the Package

1. Add **NeoCaptcha** to your project via NuGet or from the source.

### Captcha Class Usage

The core of the library is the `Captcha` class, which handles captcha creation. Below is how to generate a captcha image:

```csharp
using NeoCaptcha;

var captchaOptions = new CaptchaOptions
{
    CharacterCount = 6,
    Width = 220,
    Height = 70,
    ImageFormat = CaptchaImageFormat.PNG
};

var captcha = new Captcha(captchaOptions);

Console.WriteLine($"Captcha Text: {captcha.Text}");
Console.WriteLine($"Captcha Image Bytes Length: {captcha.ImageAsByteArray.Length}");
```

This code snippet generates a captcha image with random text, which is returned as a byte array that can be rendered in a UI.

---

## 2. ASP.NET Core Integration

You can easily integrate NeoCaptcha into your ASP.NET Core project by registering it as a service. This allows you to generate and validate captchas in your web API.

### Installing the Package

1. Add **NeoCaptcha** to your ASP.NET Core project via NuGet.

### Registering the Captcha Service

In the `Startup.cs` or `Program.cs` file, register NeoCaptcha as a service in the `ConfigureServices` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNeoCaptchaGenerator(TimeSpan.FromMinutes(5));
}
```

### Using the Captcha in a Controller

In your controller, you can now use the `ICaptchaGenerator` service to generate and validate captchas:

```csharp
using NeoCaptcha;
using Microsoft.AspNetCore.Mvc;

public class CaptchaController : ControllerBase
{
    private readonly ICaptchaGenerator _captchaGenerator;

    public CaptchaController(ICaptchaGenerator captchaGenerator)
    {
        _captchaGenerator = captchaGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> GenerateCaptcha()
    {
        var captcha = await _captchaGenerator.GenerateNewCaptcha();
        Console.WriteLine(captcha.CaptchaId);
        return File(captcha.CaptchaImage, "image/png");
    }

    [HttpPost]
    public async Task<IActionResult> VerifyCaptcha([FromQuery] Guid captchaId, [FromBody] string text)
    {
        var result = await _captchaGenerator.ValidateCaptcha(captchaId, text);
        return Ok(result.ToString());
    }
}
```

### Explanation:
- **GenerateCaptcha**: This endpoint generates a new captcha and returns the image as a PNG file.
- **VerifyCaptcha**: This endpoint verifies the user's input against the captcha.

---

## CaptchaOptions Class

You can configure the captcha generation with the `CaptchaOptions` class:

```csharp
public class CaptchaOptions
{
    public int CharacterCount { get; set; } = 6;
    public int Width { get; set; } = 220;
    public int Height { get; set; } = 70;
    public CaptchaImageFormat ImageFormat { get; set; } = CaptchaImageFormat.PNG;
    public bool IsMultiColorText { get; set; } = false;
}
```

### Properties:
- **CharacterCount**: The number of characters in the captcha text.
- **Width**: The width of the captcha image.
- **Height**: The height of the captcha image.
- **ImageFormat**: The format of the generated image (PNG or JPG).
- **IsMultiColorText**: Whether the captcha text should be multi-colored.

---

## Captcha Generation & Validation

- The **GenerateNewCaptcha** method returns a captcha image and its associated text.
- The **ValidateCaptcha** method checks if the user's input matches the captcha text.

---

## CaptchaManager Class

The `NeoCaptchaManager` is responsible for managing captcha instances, storing them temporarily, and validating user inputs:

```csharp
public class NeoCaptchaManager : ICaptchaGenerator
{
    private readonly FastCache<Guid, string> _captchaResultHolder = new ();

    public Task<CaptchaGeneratorResult> GenerateNewCaptcha(CaptchaOptions captchaOptions)
    {
        var captcha = new Captcha(captchaOptions);
        var id = Guid.NewGuid();
        var result = new CaptchaGeneratorResult(id, captcha.ImageAsByteArray);
        _captchaResultHolder.TryAdd(id, captcha.Text, expirationTime);
        return Task.FromResult(result);
    }

    public Task<CaptchaValidationResult> ValidateCaptcha(Guid captchaId, string challenge)
    {
        if (!_captchaResultHolder.TryRemove(captchaId, out var text))
        {
            return Task.FromResult(CaptchaValidationResult.EXPIRED);
        }

        return Task.FromResult(challenge == text ? CaptchaValidationResult.OK : CaptchaValidationResult.INVALID);
    }
}
```

---

## Conclusion

NeoCaptcha provides an easy-to-use, cross-platform solution for captcha generation in your application. It supports ASP.NET Core integration, provides a range of customizable options, and helps protect your site from bots and automated abuse.

For further customization and advanced usage, feel free to extend or modify the `Captcha` and `CaptchaManager` classes.
