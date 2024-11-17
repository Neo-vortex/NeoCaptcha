using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

using NeoCaptcha.AspnetCore.Interfaces;


namespace NeoCaptcha.AspnetCore.Attributes;

public class VerifyNeoCaptchaAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        // Resolve ICaptchaGenerator from the service provider
        var captchaGenerator = serviceProvider.GetRequiredService<ICaptchaGenerator>();
        return new VerifyNeoCaptchaFilter(captchaGenerator);
    }
}