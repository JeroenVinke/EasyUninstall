using System;
using System.Diagnostics;
using EasyUninstall.Interfaces;
using EasyUninstall.Models;

namespace EasyUninstall.Helpers
{
    public class UninstallHelper : IUninstallHelper
    {
        public event EventHandler<ExtendedEventArgs> Error;

        public void Uninstall(WindowsApplication application, bool trySilent = true)
        {
            var path = application.UninstallString;
            var guid = application.Guid;

            try
            {


                var command = path;


                // Try to run msiexec.exe with silent options
                if (trySilent)
                {
                    if (path.ToLower().Contains("msiexec"))
                    {
                        command = "MsiExec.exe /X " + guid + " /qb";
                    }
                }
                



                var process = Process.Start("cmd.exe", "/C \"" + command + "\"");

                if (process != null)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception error)
            {
                if (Error != null)
                {
                    Error(this, new ExtendedEventArgs()
                    {
                        Message = "Failed to remove application " + application.Name + " -> " + error
                    });
                }
            }
        }
    }

    public class ExtendedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
