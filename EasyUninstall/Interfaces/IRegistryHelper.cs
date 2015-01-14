using System.Collections.Generic;
using EasyUninstall.Models;

namespace EasyUninstall.Interfaces
{
    public interface IRegistryHelper
    {
        List<Publisher> Publishers { get; set; }
        List<WindowsApplication> Applications { get; set; }
        void Refresh();
    }
}