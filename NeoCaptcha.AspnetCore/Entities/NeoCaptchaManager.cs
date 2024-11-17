using System.Collections.Concurrent;
using Jitbit.Utils;
using MyAspNetCoreExtensions.Enitities;
using NeoCaptcha.AspnetCore.Interfaces;

namespace NeoCaptcha.AspnetCore.Entities;

public class NeoCaptchaManager(TimeSpan expirationTime) : ICaptchaGenerator
{
    private readonly FastCache<Guid, string> _captchaResultHolder = new ();

    public Task<CaptchaGeneratorResult> GenerateNewCaptcha(CaptchaOptions captchaOptions)
    {
        var captcha = new Captcha(captchaOptions);
        var id = Guid.NewGuid();
        var result = new CaptchaGeneratorResult(id,captcha.ImageAsByteArray);
        _captchaResultHolder.TryAdd(id, captcha.Text, expirationTime);
        return Task.FromResult(result);
    }

    public Task<CaptchaGeneratorResult> GenerateNewCaptcha()
    {
        var captcha = new Captcha(new CaptchaOptions());
        var id = Guid.NewGuid();
        var result = new CaptchaGeneratorResult(id,captcha.ImageAsByteArray);
        _captchaResultHolder.TryAdd(id, captcha.Text,expirationTime);
        return Task.FromResult(result);
    }

    public Task<CaptchaValidationResult> ValidateCaptcha(Guid captchaId,  string challenge)
    {
        if (!_captchaResultHolder.TryRemove(captchaId, out var text))
        {
            return Task.FromResult(CaptchaValidationResult.EXPIRED);
        }

        return Task.FromResult(challenge == text ? CaptchaValidationResult.OK : CaptchaValidationResult.INVALID);
    }
}