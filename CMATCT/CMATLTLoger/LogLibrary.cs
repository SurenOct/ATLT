using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMATLTLoger
{
    public static class LogLibrary
    {
        private static readonly object _lock = new object();
        public static readonly string FolderPath;
        private static readonly string ServiceLogPath;
        private static readonly string ServiceHealthLogPath;
        //TEst
        static LogLibrary()
        {
            // 1. Define folder
            FolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CMATLTData", "Logs");

            // 2. Create folder if it doesn't exist
            Directory.CreateDirectory(FolderPath);

            // 3. Define two files in the same folder
            ServiceLogPath = Path.Combine(FolderPath, "service.log");
            ServiceHealthLogPath = Path.Combine(FolderPath, "ServiceHealthLog.txt");

            // 4. Ensure both files exist
            if (!File.Exists(ServiceLogPath))
                using (File.Create(ServiceLogPath)) { }

            if (!File.Exists(ServiceHealthLogPath))
                using (File.Create(ServiceHealthLogPath)) { }
        }
        public static string GetLogPath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "MyService", "Logs");

            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "service.log");
        }
        public static void LogService(string message)
        {
            lock (_lock)
            {
                File.AppendAllText(ServiceLogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}{Environment.NewLine}");
            }
        }

        public static void LogError(string message)
        {
            lock (_lock)
            {
                File.AppendAllText(ServiceHealthLogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}{Environment.NewLine}");
            }
        }

        public static void LogError(Exception ex)
        {
            LogError($"{ex.Source?.Trim()}; {ex.Message?.Trim()}");
        }
        public static void Log(string message)
        {
            File.AppendAllText(GetLogPath(), $"{DateTime.Now}: {message}\r\n");
        }
        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() + "; " + ex.Message.ToString().Trim());
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }
        public static void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }

    }
}
