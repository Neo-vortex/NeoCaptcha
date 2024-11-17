using Microsoft.AspNetCore.Mvc.Filters;
using NeoCaptcha.AspnetCore.Attributes;
using NeoCaptcha.AspnetCore.Interfaces;

namespace NeoCaptcha.AspnetCore;

public class VerifyNeoCaptchaFilterFactory(ICaptchaGenerator captchaGenerator) : IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new VerifyNeoCaptchaAttribute(captchaGenerator);
    }

    public bool IsReusable => false;
}