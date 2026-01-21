using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CMATCTUI.Generic
{
    public class GenericXmlLoader
    {
        public static XDocument LoadingXML(string filepath, string folderName)
        {
            XDocument doc = null;
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
            string configPath = Path.Combine(projectRoot, folderName, filepath);

            if (File.Exists(configPath))
            {
                doc = XDocument.Load(configPath);

            }
            return doc;
        }
    }
}
