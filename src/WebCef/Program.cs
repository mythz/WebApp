using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.CefGlue;
using WebApp;

namespace WebCef
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var host = Startup.CreateWebHost(args);
                host.Build().StartAsync();
                
                var config = new CefConfig
                {
                    Args = args,
                    StartUrl = host.StartUrl,
                    Icon = "icon".GetAppSettingPath(host.AppDir) ?? "favicon.ico",
                };

                if ("name".TryGetAppSetting(out var name))
                    config.WindowTitle = name;

                if ("CefConfig".TryGetAppSetting(out var cefConfigString))
                {
                    var cefConfig = JS.eval(cefConfigString);
                    if (cefConfig is Dictionary<string, object> objDictionary)
                        objDictionary.PopulateInstance(config);
                }
                if ("CefConfig.CefSettings".TryGetAppSetting(out var cefSettingsString))
                {
                    var cefSettings = JS.eval(cefSettingsString);
                    if (cefSettings is Dictionary<string, object> objDictionary)
                        objDictionary.PopulateInstance(config.CefSettings);
                }

                return CefPlatformWindows.Start(config);
            } 
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}
