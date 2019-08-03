using AspNetCore.FTPHelper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.FTPHelper
{
    public static class RegistrarServices
    {
        public static void AddFTPHelper<TOption>(this IServiceCollection services, Action<TOption> settings) where TOption : class
        {
            services.Configure<TOption>(settings);  
            services.AddSingleton<IFTPFileHelpers, FTPFileHelpers>(); 
        } 
    }
}
