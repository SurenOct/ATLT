//using MailKit.Net.Smtp;
//using MailKit.Security;
//using MimeKit;

//class Program
//{
//    static void Main()
//    {
//        // 1️⃣ Load SMTP config from XML
//        var smtpSettings = SmtpConfigLoader.Load("smtp.config.xml");

//        // 2️⃣ Build email
//        var message = new MimeMessage();
//        message.From.Add(new MailboxAddress(
//            "Mahesh Kulkarni",
//            smtpSettings.Username));

//        message.To.Add(MailboxAddress.Parse("recipient@company.com"));
//        message.Subject = "Test Email using XML Configuration";

//        message.Body = new TextPart("plain")
//        {
//            Text = "Hello,\n\nThis email was sent using SMTP settings loaded from XML.\n\nRegards,\nMahesh"
//        };

//        // 3️⃣ Map encryption string → SecureSocketOptions
//        SecureSocketOptions socketOptions = smtpSettings.Encryption.ToUpper() switch
//        {
//            "STARTTLS" => SecureSocketOptions.StartTls,
//            "SSL" => SecureSocketOptions.SslOnConnect,
//            "NONE" => SecureSocketOptions.None,
//            _ => SecureSocketOptions.StartTls
//        };

//        // 4️⃣ Send email
//        using var client = new SmtpClient();
//        client.Connect(
//            smtpSettings.Server,
//            smtpSettings.Port,
//            socketOptions);

//        client.Authenticate(
//            smtpSettings.Username,
//            smtpSettings.Password);

//        client.Send(message);
//        client.Disconnect(true);
//    }
//}
