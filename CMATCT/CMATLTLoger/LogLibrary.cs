using System;
using System.IO;

namespace CMATLTLoger
{
    public static class LogLibrary
    {
        private static readonly object _lock = new object();
        public static readonly string FolderPath;
        private static readonly string ServiceLogPath;
        private static readonly string ServiceHealthLogPath;
        private static readonly string UILogPath;

        //Constructor
        static LogLibrary()
        {
            // 1. Define folder
            FolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CMATLTData", "Logs");

            // 2. Create folder if it doesn't exist
            Directory.CreateDirectory(FolderPath);

            // 3. Define two files in the same folder
            ServiceLogPath = Path.Combine(FolderPath, "Service.log");
            ServiceHealthLogPath = Path.Combine(FolderPath, "ServiceHealth.log");
            UILogPath = Path.Combine(FolderPath, "UI.log");

            // 4. Ensure all files exist
            EnsureFileExists(ServiceLogPath);
            EnsureFileExists(ServiceHealthLogPath);
            EnsureFileExists(UILogPath);
        }

        private static void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
                using (File.Create(filePath)) { }
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

        //public static void LogUI(string message, string level = "INFO")
        //{
        //    lock (_lock)
        //    {
        //        File.AppendAllText(UILogPath,
        //            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}]: {message}{Environment.NewLine}");
        //    }
        //}

        //public static void LogUIDebug(string message)
        //{
        //    LogUI(message, "DEBUG");
        //}

        //public static void LogUIInfo(string message)
        //{
        //    LogUI(message, "INFO");
        //}

        //public static void LogUIWarning(string message)
        //{
        //    LogUI(message, "WARNING");
        //}

        //public static void LogUIError(string message)
        //{
        //    LogUI(message, "ERROR");
        //}

        //public static void LogUIError(Exception ex)
        //{
        //    LogUI($"{ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}", "ERROR");
        //}

        public static void LogError(Exception ex)
        {
            LogError($"{ex.Source?.Trim()}; {ex.Message?.Trim()}");
        }

        // Legacy methods (keep for backward compatibility)
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

        public static string GetLogPath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "MyService", "Logs");

            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "service.log");
        }
    }
}
