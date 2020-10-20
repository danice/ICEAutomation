using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageComposeEditorAutomation
{
    public enum CameraMotion
    {
        [Description("Auto-detect")]
        autoDetect,
        [Description("Planar motion")]
        planarMotion,
        [Description("Planar motion with skew")]
        planarMotionWithSkew,
        [Description("Planar motion with perspective")]
        planarMotionWithPerspective,
        [Description("Rotating motion")]
        rotatingMotion
    }

    public class ComposeAppService
    {
        Action<string> onEvent;
        Action<int> onProgress;

        public void Compose(string[] images, CameraMotion cameraMotion, Action<string> onEvent = null, Action<int> onProgress = null, bool saveProject = false)
        {
            this.onEvent = onEvent;
            this.onProgress = onProgress;
            var appStr = ConfigurationManager.AppSettings["ICE-app-path"];
            var appName = Path.GetFileName(appStr);
            var exportBtnLabel = ConfigurationManager.AppSettings["Export-btn-label"];
            var exportToDiskBtnLabel = ConfigurationManager.AppSettings["ExportToDisk-btn-label"];
            var cameraMotionLabel = ConfigurationManager.AppSettings["CameraMotion-btn-label"];
            var exportPanoramaBtnLabel = ConfigurationManager.AppSettings["ExportPanorama-btn-label"];
            var saveBtnLabel = ConfigurationManager.AppSettings["Save-btn-label"];
            var saveProjectLabel = ConfigurationManager.AppSettings["Save-project-label"];
            int saveWait = int.Parse(ConfigurationManager.AppSettings["Save-wait"]);

            var imgStr = string.Join(" ", images);
            var processStartInfo = new ProcessStartInfo(fileName: appStr, arguments: imgStr);
            var app = FlaUI.Core.Application.Launch(processStartInfo);
            
            using (var automation = new UIA3Automation())
            {
                string title = null;
                Window window = null;
                do
                {
                    try
                    {
                        app = FlaUI.Core.Application.Attach(appName);
                        window = app.GetMainWindow(automation);
                        title = window.Title;
                        OnEvent("Opened :" + title);
                    }
                    catch (Exception)
                    {
                        title = null;
                    }

                } while (string.IsNullOrWhiteSpace(title));

                OnEvent("files :" + imgStr);
                if (cameraMotion != CameraMotion.autoDetect)
                {
                    var window2 = app.GetMainWindow(automation);
                    var comboBoxes = window.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox)).Select(x => x.AsComboBox());
                    var cameraActionSelect = comboBoxes.FirstOrDefault(x => x.Items.Length > 0 && x.Items[0].Name == "Auto-detect");
                    if (cameraActionSelect != null)
                    {
                         var descAttr = GetAttribute<DescriptionAttribute>(cameraMotion);
                        cameraActionSelect.Select(descAttr.Description);
                    }
                }

                try
                {
                    AutomationElement button1 = null;
                    do
                    {
                        button1 = window.FindFirstDescendant(cf => cf.ByText(exportBtnLabel));
                        if (button1 == null)
                        {
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

                    if (saveProject)
                    {
                        window.Close();
                        var onCloseDlg = window.ModalWindows[0];
                        var buttonOnCloseSave = onCloseDlg.FindFirstDescendant(cf => cf.ByText(saveBtnLabel)).AsButton();
                        buttonOnCloseSave?.Invoke();

                        var saveProjectDlg = window.ModalWindows[0];

                        var projectName = saveProjectDlg.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox)).AsComboBox();
                        projectName.EditableText = Path.GetFileNameWithoutExtension(images[0]);
                        var buttonSaveProjectSave = saveProjectDlg.FindFirstDescendant(cf => cf.ByText(saveBtnLabel)).AsButton();
                        buttonSaveProjectSave?.Invoke();


                        //var buttonSaveProj = window.FindFirstDescendant(cf => cf.ByText(saveProjectLabel));
                        //if (buttonSaveProj != null && buttonSaveProj.ControlType != ControlType.Button)
                        //    buttonSaveProj = buttonSaveProj.AsButton().Parent;
                        //var saveProj = buttonSaveProj?.AsButton();
                        //if (saveProj.IsEnabled)
                        //    buttonSaveProj?.AsButton().Invoke();
                        OnEvent("saving project...");
                    }

                }
                catch (Exception ex)
                {
                    OnEvent(ex.Message);
                }

            }
            app.Kill();
            app = null;
        }

        public static T GetAttribute<T>(Enum enumeration) where T : Attribute
        {
            var type = enumeration.GetType();

            var memberInfo = type.GetMember(enumeration.ToString());

            if (!memberInfo.Any())
                throw new ArgumentException($"No public members for the argument '{enumeration}'.");

            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);

            if (attributes == null || attributes.Length != 1)
                throw new ArgumentException($"Can't find an attribute matching '{typeof(T).Name}' for the argument '{enumeration}'");

            return attributes.Single() as T;
        }



        private void OnEvent(string message)
        {
            this.onEvent?.Invoke(message);
        }
    }
}
