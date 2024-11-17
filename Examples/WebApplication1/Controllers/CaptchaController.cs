using Microsoft.AspNetCore.Mvc;
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
        IsMultiColorText = false,
        CharacterCount = 5,
        IsRandomRotation = true,
        IsBackgroundNoiseEnabled = true
    });
    Console.WriteLine(captcha.CaptchaId);
    return File(captcha.CaptchaImage, "image/png");
   }


   [HttpPost]
   [ServiceFilter(typeof(VerifyNeoCaptchaFilterFactory))]
   public async Task<IActionResult> VerifyCaptchaAttrib( [FromBody] TestModel model)
   {
       return Ok("It works!");
   }

   [HttpPost]
   public async Task<IActionResult> VerifyCaptchaManual([FromBody] TestModel model)
   {
       var result = await _captchaGenerator.ValidateCaptcha(model.CaptchaId, model.CaptchaChallenge);
       return Ok(result);
   }

   public class  TestModel : NeoCaptchaCapableModel
   {
       public required string Payload { get; set; }
   }

 
}