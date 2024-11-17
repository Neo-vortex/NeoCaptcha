using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyAspNetCoreExtensions.Enitities;
using NeoCaptcha.AspnetCore.Entities;
using NeoCaptcha.AspnetCore.Interfaces;

namespace NeoCaptcha.AspnetCore;

public class VerifyNeoCaptchaFilter(ICaptchaGenerator captchaGenerator) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.Values.FirstOrDefault(arg => arg is NeoCaptchaCapableModel) is not NeoCaptchaCapableModel model)
        {
            context.Result = new BadRequestObjectResult("Invalid captcha model");
            return;
        }

        var validationResult = await captchaGenerator.ValidateCaptcha(model.CaptchaId, model.CaptchaChallenge);

        if (validationResult != CaptchaValidationResult.OK)
        {
            context.Result = new BadRequestObjectResult($"Captcha validation failed: {validationResult}");
            return;
        }

        await next();
    }
}