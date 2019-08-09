using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageComposeEditorAutomation
{
    public class ComposeAppService
    {
        Action<string> onEvent;
        Action<int> onProgress;

        public void Compose(string[] images, Action<string> onEvent = null, Action<int> onProgress = null)
        {
            this.onEvent = onEvent;
            this.onProgress = onProgress;
            var appStr = ConfigurationManager.AppSettings["ICE-app-path"];
            var appName = Path.GetFileName(appStr);
            var exportBtnLabel = ConfigurationManager.AppSettings["Export-btn-label"];
            var exportToDiskBtnLabel = ConfigurationManager.AppSettings["ExportToDisk-btn-label"];
            var exportPanoramaBtnLabel = ConfigurationManager.AppSettings["ExportPanorama-btn-label"];
            var saveBtnLabel = ConfigurationManager.AppSettings["Save-btn-label"];
            int saveWait = int.Parse(ConfigurationManager.AppSettings["Save-wait"]);

            var imgStr = string.Join(" ", images);
            var processStartInfo = new ProcessStartInfo(fileName: appStr, arguments: imgStr);
            var app = FlaUI.Core.Application.Launch(processStartInfo);
            //Thread.Sleep(4000);
            //var app =  FlaUI.Core.Application.Attach("ICE.exe");
            using (var automation = new UIA3Automation())
            {
                string title = null;
                Window window = null;
                do
                {
                    app = FlaUI.Core.Application.Attach(appName);
                    window = app.GetMainWindow(automation);
                    title = window.Title;
                    OnEvent("Opened :" + title);
                } while (string.IsNullOrWhiteSpace(title));
                                
                OnEvent("files :" + imgStr);
                try
                {
                    AutomationElement button1 = null;
                    do
                    {
                        button1 = window.FindFirstDescendant(cf => cf.ByText(exportBtnLabel));
                        if (button1 == null) {
                            OnEvent(".");
                        }                        
                    } while (button1 == null);
                    
                    if (button1.ControlType != ControlType.Button)
                        button1 = button1.AsButton().Parent;

                    button1?.AsButton().Invoke();
                    bool finished = false;
                    OnEvent("composing.");
                    do
                    {
                        var window2 = app.GetMainWindow(automation);
                        var button2 = window.FindFirstDescendant(cf => cf.ByText(exportToDiskBtnLabel));
                        
                        title = window2.Title;
                        finished = button2 != null && title.StartsWith("U");
                        int percent = 0;
                        if (!finished)
                        {
                            var percentStr = title.Substring(0, 2);
                            var numStr = percentStr[1] == '%' ? percentStr.Substring(0, 1) : percentStr;
                            if (int.TryParse(numStr, out percent))                            
                                onProgress?.Invoke(percent);                                                            
                        }
                    } while (!finished);

                }
                catch (Exception ex)
                {
                    OnEvent(ex.Message);
                }

                try
                {
                    var button2 = window.FindFirstDescendant(cf => cf.ByText(exportToDiskBtnLabel));
                    if (button2 != null && button2.ControlType != ControlType.Button)
                        button2 = button2.AsButton().Parent;

                    button2?.AsButton().Invoke();
                    OnEvent("exporting to disk...");
                }
                catch (Exception ex)
                {
                    OnEvent(ex.Message);
                }
                try
                {
                    Thread.Sleep(1000);
                    var saveDlg = window.ModalWindows.Length == 1 
                        ? window.ModalWindows[0]
                        : window.ModalWindows.FirstOrDefault(w => w.Name == exportPanoramaBtnLabel);
                    var buttonSave = saveDlg.FindFirstDescendant(cf => cf.ByText(saveBtnLabel)).AsButton();
                    buttonSave?.Invoke();
                    
                    Thread.Sleep(saveWait);
                }
                catch (Exception ex)
                {
                    OnEvent(ex.Message);
                }

            }
            app.Kill();
        }

        private void OnEvent(string message)
        {
            this.onEvent?.Invoke(message);
        }
    }
}
