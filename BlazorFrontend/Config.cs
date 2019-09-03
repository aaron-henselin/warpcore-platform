using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorComponents.Client
{
    public class ConfigurationManager
    {
        private readonly IJSRuntime _jsRuntime;

        public ConfigurationManager(IJSRuntime jsRuntime)
        {
            this._jsRuntime = jsRuntime;
        }

        public  async Task<T> GetAppConfig<T>()
        {
            var appConfig = await _jsRuntime.InvokeAsync<string>("window.__getBlazorAppConfig");
            return JsonSerializer.Deserialize<T>(appConfig);
        }

        public  async Task<string> GetAppHomeUrl()
        {
           
            return await _jsRuntime.InvokeAsync<string>("window.__getBlazorStartPage");
            
        }
    }
}
