using Microsoft.AspNetCore.Mvc.Filters;
using NeoCaptcha.AspnetCore.Interfaces;

namespace NeoCaptcha.AspnetCore;

public class VerifyNeoCaptchaFilterFactory(ICaptchaGenerator captchaGenerator) : IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new VerifyNeoCaptchaFilter(captchaGenerator);
    }
}