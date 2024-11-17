using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyAspNetCoreExtensions.Enitities;
using NeoCaptcha.AspnetCore.Entities;
using NeoCaptcha.AspnetCore.Interfaces;

namespace NeoCaptcha.AspnetCore.Attributes;

public class VerifyNeoCaptchaAttribute(ICaptchaGenerator captchaGenerator) : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Find the model that implements RecaptchaCapableModel

        if (context.ActionArguments.Values
                .FirstOrDefault(arg => arg is NeoCaptchaCapableModel) is not NeoCaptchaCapableModel model)
        {
            context.Result = new BadRequestObjectResult("Invalid model for captcha validation.");
            return;
        }

        // Perform captcha validation
        var validationResult = await captchaGenerator.ValidateCaptcha(model.CaptchaId, model.CaptchaChallenge);

        if (validationResult != CaptchaValidationResult.OK)
        {
            context.Result = new BadRequestObjectResult("Captcha validation failed : captcha result :" + validationResult);
            return;
        }

        // Continue to the action if validation succeeds
        await next();
    }
}