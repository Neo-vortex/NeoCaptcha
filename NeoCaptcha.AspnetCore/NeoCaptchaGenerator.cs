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
            // Register services for your middleware
            // You can register your own services here
            services.AddSingleton<ICaptchaGenerator>(new NeoCaptchaManager(expirationTime));
            return services;
        }
    }
}