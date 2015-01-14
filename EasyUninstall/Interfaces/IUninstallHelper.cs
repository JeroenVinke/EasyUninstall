using System;
using EasyUninstall.Helpers;
using EasyUninstall.Models;

namespace EasyUninstall.Interfaces
{
    public interface IUninstallHelper
    {
        event EventHandler<ExtendedEventArgs> Error;
        void Uninstall(WindowsApplication application, bool trySilent = true);
    }
}