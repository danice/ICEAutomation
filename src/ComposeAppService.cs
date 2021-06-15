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

    public class ComposeAppService
    {
        Action<string> onEvent;
        Action<int> onProgress;
        Application app;

        public void Compose(string[] images, BaseOptions options, Action<string> onEvent = null, Action<int> onProgress = null, bool saveProject = false)
        {
            var cameraMotion = options.Motion;
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
            this.app = FlaUI.Core.Application.Launch(processStartInfo);


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
                if (options is StructurePanoramaOptions)
                    SetStructurePanorama(automation);

                SetCameraMotion(automation, options.Motion);

                if (options is StructurePanoramaOptions)
                    SetStructurePanoramaOptions(automation, (StructurePanoramaOptions)options);

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
                    if (buttonSave == null) {
                        OnEvent("Save button not found: "+saveBtnLabel);
                    } else 
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

        void SetCameraMotion(UIA3Automation automation, CameraMotion cameraMotion)
        {
            if (cameraMotion == CameraMotion.autoDetect)
                return;

            var window2 = app.GetMainWindow(automation);
            var comboBoxes = window2.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox)).Select(x => x.AsComboBox());
            var cameraActionSelect = comboBoxes.FirstOrDefault(x => x.Items.Length > 0 && x.Items[0].Name == "Auto-detect");
            if (cameraActionSelect != null)
            {
                var descAttr = GetAttribute<DescriptionAttribute>(cameraMotion);
                cameraActionSelect.Select(descAttr.Description);
            }


        }

        void SetStructurePanorama(UIA3Automation automation)
        {
            var window2 = app.GetMainWindow(automation);
            AutomationElement button2 = null;

            do
            {
                button2 = window2.FindFirstDescendant(cf => cf.ByText("Structured panorama"));
            } while (button2 == null);
            if (button2 != null && button2.ControlType != ControlType.Button)
                button2 = button2.AsButton().Parent;
            var list = button2.AsListBox().Select(1);


            //button2.Click();
        }


        void SetStructurePanoramaOptions(UIA3Automation automation, StructurePanoramaOptions options)
        {
            var window = app.GetMainWindow(automation);

            try
            {
                 // Initial corner and direction - top left
                var pos1 = GetPosition1(options.InitialCorner);
                var pos2 = GetPosition2(options.InitialCorner, options.Rows);
                var layout = window.FindFirstDescendant(cf => cf.ByName("Layout"));
                var b1 = layout.FindChildAt(pos1);
                //var str = layout.FindAllDescendants().Select(d => string.Format("id: {0} {1} [{2}]", d.AutomationId, d.HelpText, d.Name)).ToList();
                //Console.WriteLine(string.Join(",", str));
                b1.AsRadioButton().IsChecked = true;

                var b2 = layout.FindChildAt(pos2);
                b2.AsRadioButton().IsChecked = true;   
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
            }

            try
            {                
                // Number of columns - 3x3
                var numOfRowsOrColumns = options.Rows.HasValue ? options.Rows.Value : options.Columns.Value;
                var b3 = options.Rows.HasValue 
                    ? window.FindFirstDescendant(cf => cf.ByAutomationId("rowCountTextBox"))
                    : window.FindFirstDescendant(cf => cf.ByAutomationId("primaryDirectionImageCountTextBox"));
                b3.AsTextBox().Enter(numOfRowsOrColumns.ToString());

                //Serpentine
                var radiobutton = window.FindFirstDescendant(cf => cf.ByAutomationId("serpentineRadioButton")).AsRadioButton();
                radiobutton.IsChecked = true;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                     
            }

            try
            {                
                //Overlap percentage
                var overlap = window.FindFirstDescendant(cf => cf.ByName("Overlap"));
                // var str2 = overlap.FindAllDescendants().Select(d => string.Format("<{2}> id: {0} {1} [{3}]", d.AutomationId, d.HelpText, d.ControlType.ToString(), d.Name)).ToList();
                // Console.WriteLine(string.Join("\n\r", str2));

                var overlapH = options.HorizontalOverlap ?? 10;
                var overlapV = options.VerticalOverlap ?? 10;
                window.FindFirstDescendant(cf => cf.ByAutomationId("horizontalOverlapTextBox")).AsTextBox().Enter(overlapH.ToString());
                window.FindFirstDescendant(cf => cf.ByAutomationId("verticalOverlapTextBox")).AsTextBox().Enter(overlapV.ToString());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);            
            }           

        }
        int GetPosition1(Corner corner)
        {
            if (corner == Corner.topLeft)
                return 5;
            if (corner == Corner.topRight)
                return 6;
            if (corner == Corner.bottomLeft)
                return 7;
            return 8;  //"Start in bottom right corner"
        }

        int GetPosition2(Corner corner, int? rows)
        {
            if (corner == Corner.topLeft)
                return rows.HasValue 
                    ? 11  //Start moving down
                    : 9;  //Start moving right
            if (corner == Corner.topRight)
                return rows.HasValue 
                    ? 12  //Start moving down
                    : 10; //Start moving left
            if (corner == Corner.bottomLeft)
                return rows.HasValue 
                    ? 13  //Start moving up
                    : 15;  //Start moving right
            return rows.HasValue 
                    ? 14  //Start moving up
                    : 16; //Start moving left
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
