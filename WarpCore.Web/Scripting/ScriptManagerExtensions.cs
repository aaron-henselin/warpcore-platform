using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace WarpCore.Web.Scripting
{


    
        public static class ScriptManagerExtensions
        {
            private static string _runOnFullOrPartialPostback =
                @"
                    (function(){         
                     var isFirstBoot = true;;
                        var scriptToExecute = function(){
                            [scripttext]

                            isFirstBoot = false;
                        };

                        Sys.Application.add_init(scriptToExecute);

                        var prm = Sys.WebForms.PageRequestManager.getInstance();
                        prm.add_pageLoaded(function(){
                            if (!isFirstBoot)
                            scriptToExecute();
                        });
                    })();
                  ";
            public static void RegisterScriptToRunEachFullOrPartialPostback(Page page, string scriptText)
            {
                var processedScript = _runOnFullOrPartialPostback.Replace("[scripttext]", scriptText);
                var scriptManager = ScriptManager.GetCurrent(page);
                if (scriptManager == null)
                    throw new Exception("ScriptManager is not avaiable in the current context.");

                if (!scriptManager.IsInAsyncPostBack)
                    ScriptManager.RegisterStartupScript(page, typeof(ScriptManagerExtensions), scriptText, processedScript, true);
            }


            private static string _runOnInit =
                @"Sys.Application.add_init(function () {
                    [scripttext]
              });";

            public static void RegisterScriptToRunOnApplicationInitialized(Page page, string scriptText)
            {
                var processedScript = _runOnInit.Replace("[scripttext]", scriptText);
                ScriptManager.RegisterStartupScript(page, typeof(ScriptManagerExtensions), scriptText, processedScript, true);
            }
        }
    
}