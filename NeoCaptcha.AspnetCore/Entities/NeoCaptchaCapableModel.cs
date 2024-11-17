namespace NeoCaptcha.AspnetCore.Entities;


    public abstract class NeoCaptchaCapableModel
    {
        public  Guid CaptchaId { get; set; }
        public  string CaptchaChallenge { get; set; } 
    }

