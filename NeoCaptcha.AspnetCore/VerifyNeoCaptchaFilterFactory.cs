using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NeoCaptcha.AspnetCore;
using NeoCaptcha.AspnetCore.Interfaces;

public class VerifyNeoCaptchaFilterFactory : IFilterFactory
{
    private readonly ICaptchaGenerator _captchaGenerator;

    public VerifyNeoCaptchaFilterFactory(ICaptchaGenerator captchaGenerator)
    {
        _captchaGenerator = captchaGenerator;
    }

    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        // Create the filter instance, passing the required service
        return new VerifyNeoCaptchaFilter(_captchaGenerator);
    }
}