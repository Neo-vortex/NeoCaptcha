using Microsoft.Extensions.DependencyInjection;
using NeoCaptcha.AspnetCore.Entities;
using NeoCaptcha.AspnetCore.Interfaces;

namespace NeoCaptcha.AspnetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNeoCaptchaGenerator(this IServiceCollection services,
            TimeSpan expirationTime)
        {
            services.AddSingleton<ICaptchaGenerator>(new NeoCaptchaManager(expirationTime));
            services.AddScoped<VerifyNeoCaptchaFilterFactory>();
            return services;
        }
    }
}