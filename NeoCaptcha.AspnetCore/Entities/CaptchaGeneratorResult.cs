namespace NeoCaptcha.AspnetCore.Entities;

public record CaptchaGeneratorResult(Guid CaptchaId, byte[] CaptchaImage)
{
}