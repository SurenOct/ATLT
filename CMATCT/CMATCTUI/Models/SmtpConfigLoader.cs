using System;
using System.IO;
using System.Xml.Linq;

public static class SmtpConfigLoader
{
    public static SmtpSettings Load(string mailSettingsPath)
    {

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        int binIndex = baseDir.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        string projectRoot;

        if (binIndex >= 0)
        {
            projectRoot = baseDir.Substring(0, binIndex);
        }
        else
        {
            projectRoot = baseDir; // if "bin" not found, return full baseDir
        }


        // 2️⃣ Build path to your Config folder
        string configPath = Path.Combine(projectRoot, "EmailConfiguration", mailSettingsPath);

        if (File.Exists(configPath))
        {
            var doc = XDocument.Load(configPath);
            var root = doc.Element("SmtpSettings");

            return new SmtpSettings
            {
                Server = root.Element("Server")?.Value,
                Port = int.Parse(root.Element("Port")?.Value ?? "0"),
                Encryption = root.Element("Encryption")?.Value,
                Username = root.Element("Username")?.Value,
                Password = root.Element("Password")?.Value
            };
        }
        else
        {
            return null;
        }
    }
}
