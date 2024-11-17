using Microsoft.AspNetCore.Mvc;
using NeoCaptcha;
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
        IsMultiColorText = true
    });
    Console.WriteLine(captcha.CaptchaId);
    return File(captcha.CaptchaImage, "image/png");
   }


   [HttpPost]
   public async Task<IActionResult> VerifyCaptcha( [FromQuery] Guid captchaId, [FromBody] string text)
   {
       var result = await _captchaGenerator.ValidateCaptcha(captchaId, text);
       return Ok(result.ToString());
   }
}