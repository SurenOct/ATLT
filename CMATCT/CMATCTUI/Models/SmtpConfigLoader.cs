using System.Xml.Linq;

public static class SmtpConfigLoader
{
    public static SmtpSettings Load(string filePath)
    {
        var doc = XDocument.Load(filePath);
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
}
