using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorComponents.Client
{
    public static class Config
    {
        public static async Task<T> GetAppConfig<T>()
        {
            var appConfig = await JSRuntime.Current.InvokeAsync<string>("window.__getBlazorAppConfig");
            return Json.Deserialize<T>(appConfig);
        }

        public static async Task<string> GetAppHomeUrl()
        {
           
            return await JSRuntime.Current.InvokeAsync<string>("window.__getBlazorStartPage");
            
        }
    }
}
