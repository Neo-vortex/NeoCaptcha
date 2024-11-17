using NeoCaptcha;
using NUnit.Framework;
using SkiaSharp;
using System.IO;
using static NUnit.Framework.Assert;

namespace NeoCaptcha.Tests
{
    [TestFixture]
    public class CaptchaTests
    {
        private static CaptchaOptions GetDefaultOptions()
        {
            return new CaptchaOptions
            {
                CharacterCount = 6,
                Width = 170,
                Height = 50,
                ImageFormat =CaptchaImageFormat.PNG,
                IsMultiColorText = false
            };
        }

        [Test]
        public void Captcha_GeneratesText_WithCorrectCharacterCount()
        {
            var options = GetDefaultOptions();
            options.CharacterCount = 6;
            var captcha = new Captcha(options);
            
            Captcha.ImageHelper.SaveImageToFile(captcha.ImageAsByteArray,"./png.png");

            That(captcha.Text, Has.Length.EqualTo(options.CharacterCount), 
                "Captcha text length does not match the specified character count.");
        }

        [Test]
        public void Captcha_GeneratesImage_WithNonNullByteArray()
        {
            var options = GetDefaultOptions();

            var captcha = new Captcha(options);

            That(captcha.ImageAsByteArray, Is.Not.Null, 
                "Captcha image byte array is null.");
            That(captcha.ImageAsByteArray, Is.Not.Empty, 
                "Captcha image byte array is empty.");
        }

        [Test]
        public void Captcha_ProducesValidImageFile()
        {
            var options = GetDefaultOptions();
            var captcha = new Captcha(options);
            var filePath = Path.Combine(Path.GetTempPath(), "test_captcha.png");

            try
            {
                Captcha.ImageHelper.SaveImageToFile(captcha.ImageAsByteArray, filePath);

                Multiple(() =>
                {
                    That(File.Exists(filePath), Is.True,
                                    "Captcha image file was not saved.");
                });
                That(new FileInfo(filePath).Length > 0, Is.True, 
                    "Captcha image file is empty.");
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Test]
        public void Captcha_AllowsCustomDimensions()
        {
            var options = new CaptchaOptions
            {
                CharacterCount = 6,
                Width = 300,
                Height = 100,
                ImageFormat = CaptchaImageFormat.JPG,
            };

            var captcha = new Captcha(options);

            using var image = SKBitmap.Decode(captcha.ImageAsByteArray);
            That(image.Width, Is.EqualTo(options.Width), 
                "Captcha image width does not match specified width.");
            That(image.Height, Is.EqualTo(options.Height), 
                "Captcha image height does not match specified height.");
        }
        

        [Test]
        public void Captcha_SupportsDifferentImageFormats()
        {
            foreach (CaptchaImageFormat format in Enum.GetValues(typeof(CaptchaImageFormat)))
            {
                var options = new CaptchaOptions
                {
                    CharacterCount = 6,
                    ImageFormat = format
                };

                var captcha = new Captcha(options);

                IsNotNull(captcha.ImageAsByteArray, 
                    $"Captcha failed for ImageFormat: {format}");
                IsNotEmpty(captcha.ImageAsByteArray, 
                    $"Captcha image is empty for ImageFormat: {format}");
            }
        }

        [Test]
        public void Captcha_ThrowsException_WhenSavingInvalidFilePath()
        {
            var options = GetDefaultOptions();
            var captcha = new Captcha(options);

            Throws<ArgumentException>(() => 
                Captcha.ImageHelper.SaveImageToFile(captcha.ImageAsByteArray, string.Empty),
                "Expected exception for invalid file path.");
        }

        [Test]
        public void Captcha_ThrowsException_WhenSavingNullImageBytes()
        {
            Throws<ArgumentException>(() =>
                Captcha.ImageHelper.SaveImageToFile(null!, "test.png"),
                "Expected exception for null image bytes.");
        }
    }
}
