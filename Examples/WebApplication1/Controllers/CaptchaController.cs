using Microsoft.AspNetCore.Mvc;
using MyAspNetCoreExtensions.Enitities;
using NeoCaptcha;
using NeoCaptcha.AspnetCore;
using NeoCaptcha.AspnetCore.Attributes;
using NeoCaptcha.AspnetCore.Entities;
using NeoCaptcha.AspnetCore.Interfaces;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CaptchaController : ControllerBase
{
   private readonly ICaptchaGenerator _captchaGenerator;

   public CaptchaController(ICaptchaGenerator captchaGenerator)
   {
      _captchaGenerator = captchaGenerator;
   }


   [HttpGet]
   public async Task<IActionResult> GenerateCaptcha()
   {
    var captcha = await  _captchaGenerator.GenerateNewCaptcha(new CaptchaOptions() with
    {
        CharacterCount = 6,
        ImageFormat = CaptchaImageFormat.JPG,
        IsRandomRotation = true,
        IsBackgroundNoiseEnabled = true,
        IsBlurringEnabled = true,
        UseRandomLineNoise = false
    });
    //Console.WriteLine(captcha.CaptchaId);
    Response.Headers.Append("result", captcha.CaptchaResult);
    return File(captcha.CaptchaImage, "image/png");
   }


   [HttpPost]
   [VerifyNeoCaptcha]
   public async Task<IActionResult> VerifyCaptchaAttrib( [FromBody] TestModel model)
   {
       return Ok("It works!");
   }

   [HttpPost]
   [ServiceFilter(typeof(VerifyNeoCaptchaFilterFactory))]
   public async Task<IActionResult> VerifyCaptchaServiceFilter( [FromBody] TestModel model)
   {
       return Ok("It works!");
   }

   
   [HttpPost]
   public async Task<IActionResult> VerifyCaptchaManual([FromBody] TestModel model)
   {
       var result = await _captchaGenerator.ValidateCaptcha(model.CaptchaId, model.CaptchaChallenge);
       if (result == CaptchaValidationResult.OK)
       {
           return Ok("It works!");
       }
       return BadRequest("captcha error");
   }

   public class  TestModel : NeoCaptchaCapableModel
   {
       public required string Payload { get; set; }
   }

 
}