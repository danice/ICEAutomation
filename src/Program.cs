using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace ImageComposeEditorAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            var appStr = @"C:\Program Files\Microsoft Research\Image Composite Editor\ICE.exe";
            var imgStr = string.Join(" ", args);
            var processStartInfo = new ProcessStartInfo(fileName: appStr, arguments: imgStr);
            var app = FlaUI.Core.Application.Launch(processStartInfo);
            //var app =  FlaUI.Core.Application.Attach("ICE.exe");
            using (var automation = new UIA3Automation())
            {
                var window = app.GetMainWindow(automation);
                var title = window.Title;
                Console.WriteLine("Opened :"+ title);
                Console.WriteLine("files :" + imgStr);
                try
                {
                    var button1 = window.FindFirstDescendant(cf => cf.ByText("EXPORT"));
                    if (button1.ControlType != ControlType.Button)
                        button1 = button1.AsButton().Parent;

                    button1?.AsButton().Invoke();
                    bool finished = false;
                    Console.Write("composing.");
                    do
                    {
                        var window2 = app.GetMainWindow(automation);

                        var button2 = window.FindFirstDescendant(cf => cf.ByText("Export to disk..."));
                        title = window2.Title;
                        finished = button2 != null && title.StartsWith("U");
                        int percent = 0;
                        if (!finished)
                        {
                            var percentStr = title.Substring(0, 2);
                            var numStr = percentStr[1] == '%' ? percentStr.Substring(0,1) : percentStr;
                            if (int.TryParse(numStr, out percent))
                            {
                                drawTextProgressBar(percent, 100);
                            }
                        }                        
                    } while (!finished);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    var button2 = window.FindFirstDescendant(cf => cf.ByText("Export to disk..."));
                    if (button2 != null && button2.ControlType != ControlType.Button)
                        button2 = button2.AsButton().Parent;

                    button2?.AsButton().Invoke();                    
                    Console.WriteLine("exporting to disk...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);                
                }
                try
                {
                    var saveDlg = window.ModalWindows.FirstOrDefault(w => w.Name == "Export Panorama");
                    var buttonSave = saveDlg.FindFirstDescendant(cf => cf.ByText("Save")).AsButton();
                    buttonSave?.Invoke();

                    int milliseconds = 5000;
                    Thread.Sleep(milliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            app.Kill();
            Console.WriteLine("Finished.");
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            ////draw empty progress bar
            //Console.CursorLeft = 0;
            //Console.Write("["); //start
            //Console.CursorLeft = 32;
            //Console.Write("]"); //end
            //Console.CursorLeft = 1;
            //float onechunk = 30.0f / total;

            ////draw filled part
            //int position = 1;
            //for (int i = 0; i < onechunk * progress; i++)
            //{
            //    Console.BackgroundColor = ConsoleColor.Gray;
            //    Console.CursorLeft = position++;
            //    Console.Write(" ");
            //}

            ////draw unfilled part
            //for (int i = position; i <= 31; i++)
            //{
            //    Console.BackgroundColor = ConsoleColor.Green;
            //    Console.CursorLeft = position++;
            //    Console.Write(" ");
            //}

            ////draw totals
            //Console.CursorLeft = 35;
            //Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorLeft = 35;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}
