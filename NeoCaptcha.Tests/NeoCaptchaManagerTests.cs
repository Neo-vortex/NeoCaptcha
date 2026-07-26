using NeoCaptcha;
using NeoCaptcha.AspnetCore.Entities;
using NeoCaptcha.AspnetCore.Enums;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace NeoCaptcha.Tests
{
    [TestFixture]
    public class NeoCaptchaManagerTests
    {
        private static NeoCaptchaManager CreateManager(TimeSpan? expiration = null)
        {
            return new NeoCaptchaManager(expiration ?? TimeSpan.FromMinutes(5));
        }

        [Test]
        public async Task ValidateCaptcha_ReturnsOk_WhenChallengeMatches()
        {
            var manager = CreateManager();
            var generated = await manager.GenerateNewCaptcha();

            var result = await manager.ValidateCaptcha(generated.CaptchaId, generated.CaptchaResult);

            That(result, Is.EqualTo(CaptchaValidationResult.OK));
        }

        [Test]
        public async Task ValidateCaptcha_IsCaseInsensitive()
        {
            var manager = CreateManager();
            var generated = await manager.GenerateNewCaptcha();

            var result = await manager.ValidateCaptcha(generated.CaptchaId, generated.CaptchaResult.ToLowerInvariant());

            That(result, Is.EqualTo(CaptchaValidationResult.OK));
        }

        [Test]
        public async Task ValidateCaptcha_ReturnsInvalid_WhenChallengeDoesNotMatch()
        {
            var manager = CreateManager();
            var generated = await manager.GenerateNewCaptcha();

            var result = await manager.ValidateCaptcha(generated.CaptchaId, "definitely-wrong");

            That(result, Is.EqualTo(CaptchaValidationResult.INVALID));
        }

        [Test]
        public async Task ValidateCaptcha_ReturnsExpired_ForUnknownCaptchaId()
        {
            var manager = CreateManager();

            var result = await manager.ValidateCaptcha(Guid.NewGuid(), "anything");

            That(result, Is.EqualTo(CaptchaValidationResult.EXPIRED));
        }

        [Test]
        public async Task ValidateCaptcha_ReturnsExpired_OnSecondAttempt_EvenAfterCorrectFirstAttempt()
        {
            // Captcha IDs are single-use: ValidateCaptcha removes the entry
            // unconditionally on first use, so it can't be replayed or brute-forced.
            var manager = CreateManager();
            var generated = await manager.GenerateNewCaptcha();

            var first = await manager.ValidateCaptcha(generated.CaptchaId, generated.CaptchaResult);
            var second = await manager.ValidateCaptcha(generated.CaptchaId, generated.CaptchaResult);

            That(first, Is.EqualTo(CaptchaValidationResult.OK));
            That(second, Is.EqualTo(CaptchaValidationResult.EXPIRED));
        }

        [Test]
        public async Task ValidateCaptcha_ReturnsExpired_OnSecondAttempt_AfterWrongFirstAttempt()
        {
            // Even a failed guess consumes the captcha ID, so repeated guessing
            // against the same ID isn't possible.
            var manager = CreateManager();
            var generated = await manager.GenerateNewCaptcha();

            var first = await manager.ValidateCaptcha(generated.CaptchaId, "wrong-guess");
            var second = await manager.ValidateCaptcha(generated.CaptchaId, generated.CaptchaResult);

            That(first, Is.EqualTo(CaptchaValidationResult.INVALID));
            That(second, Is.EqualTo(CaptchaValidationResult.EXPIRED));
        }

        [Test]
        public async Task ValidateCaptcha_ReturnsExpired_AfterExpirationTimeElapses()
        {
            var manager = CreateManager(TimeSpan.FromMilliseconds(50));
            var generated = await manager.GenerateNewCaptcha();

            await Task.Delay(250);

            var result = await manager.ValidateCaptcha(generated.CaptchaId, generated.CaptchaResult);

            That(result, Is.EqualTo(CaptchaValidationResult.EXPIRED));
        }

        [Test]
        public async Task GenerateNewCaptcha_WithOptions_RespectsCharacterCount()
        {
            var manager = CreateManager();
            var options = new CaptchaOptions { CharacterCount = 8 };

            var generated = await manager.GenerateNewCaptcha(options);

            That(generated.CaptchaResult, Has.Length.EqualTo(8));
        }

        [Test]
        public async Task GenerateNewCaptcha_ProducesUniqueCaptchaIds()
        {
            var manager = CreateManager();

            var first = await manager.GenerateNewCaptcha();
            var second = await manager.GenerateNewCaptcha();

            That(first.CaptchaId, Is.Not.EqualTo(second.CaptchaId));
        }
    }
}
