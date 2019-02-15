using System;
using System.Collections.Generic;
using System.Text;

namespace Platform_WebPipeline
{

    public abstract class WebPipelineAction
    {
    }


    public class UnhandledRequest : WebPipelineAction
    {
    }

    public class RenderPage : WebPipelineAction
    {
    }


    public class RewriteUrl : WebPipelineAction
    {
        public RewriteUrl(string transferUrl)
        {
            TransferUrl = transferUrl;
        }

        public string TransferUrl { get; }
    }

    public class Redirect : WebPipelineAction
    {
        public Redirect(string redirectUrl)
        {
            RedirectUrl = redirectUrl;
        }

        public string RedirectUrl { get; }
    }

}
