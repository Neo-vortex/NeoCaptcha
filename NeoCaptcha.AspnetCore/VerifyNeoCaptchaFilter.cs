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
        // Locate the model that implements NeoCaptchaCapableModel
        if (context.ActionArguments.Values.FirstOrDefault(arg => arg is NeoCaptchaCapableModel) is not NeoCaptchaCapableModel model)
        {
            context.Result = new UnauthorizedObjectResult("Invalid captcha model");
            return;
        }

        // Validate the captcha
        var validationResult = await captchaGenerator.ValidateCaptcha(model.CaptchaId, model.CaptchaChallenge);

        if (validationResult != CaptchaValidationResult.OK)
        {
            context.Result = new UnauthorizedObjectResult($"Captcha validation failed: {validationResult}");
            return;
        }

        await next(); // Proceed if validation succeeds
    }
}