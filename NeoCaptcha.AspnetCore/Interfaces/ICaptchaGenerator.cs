using MyAspNetCoreExtensions.Enitities;
using NeoCaptcha.AspnetCore.Entities;

namespace NeoCaptcha.AspnetCore.Interfaces;

public interface ICaptchaGenerator
{
    public Task<CaptchaGeneratorResult> GenerateNewCaptcha(NeoCaptcha.CaptchaOptions captchaOptions);
    public Task<CaptchaGeneratorResult> GenerateNewCaptcha();
    public Task<CaptchaValidationResult> ValidateCaptcha(Guid captchaId, string challenge);
}