using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

using NeoCaptcha.AspnetCore.Interfaces;


namespace NeoCaptcha.AspnetCore.Attributes;

public class VerifyNeoCaptchaAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var captchaGenerator = serviceProvider.GetRequiredService<ICaptchaGenerator>();
        return new VerifyNeoCaptchaFilter(captchaGenerator);
    }
}