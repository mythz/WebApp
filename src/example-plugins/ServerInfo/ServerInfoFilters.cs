using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using ServiceStack;
using ServiceStack.Templates;

namespace CustomFilters
{
    public class ServerInfoFilters : TemplateFilter
    {
        public static ServerInfoFilters Instance = new ServerInfoFilters();

        bool HasAccess(Process process)
        {
            try { return process.TotalProcessorTime >= TimeSpan.Zero; } 
            catch (Exception) { return false; }
        }

        public IEnumerable<Process> processes() => Process.GetProcesses().Where(HasAccess);
        public Process processById(int processId) => Process.GetProcessById(processId);
        public Process currentProcess() => Process.GetCurrentProcess();

        public DriveInfo[] drives() => DriveInfo.GetDrives();
    }
}