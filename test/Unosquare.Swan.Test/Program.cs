using MimeKit;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // create an image attachment for the file located at path
                var attachment = new MimePart("image", "png")
                {
                    ContentObject = new ContentObject(File.OpenRead("c:\\ESD\\aada.png")),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = "da.png"
                };

                var sampleMailMessage = new MimeMessage
                {
                    Subject = "MailerIO relay testing",
                    Body = new Multipart("mixed")
                    {
                        new TextPart("plain")
                        {
                            Text = "This is super cool!\r\nAnd this is more message content. Content <EOF>"
                        },
                        attachment
                    }
                };

                sampleMailMessage.From.Add(new MailboxAddress("Sender Name", "timecore@unosquare.net"));
                sampleMailMessage.To.Add(new MailboxAddress("Recipient Name", "geovanni.perez@unosquare.com"));

                var state = new SmtpSessionState()
                {
                    SenderAddress = (sampleMailMessage.From[0] as MailboxAddress)?.Address
                };

                state.Recipients.AddRange(sampleMailMessage.To.Select(x => (x as MailboxAddress)?.Address));

                using (var memStream = new MemoryStream())
                {
                    sampleMailMessage.WriteTo(memStream);

                    state.DataBuffer.AddRange(memStream.ToArray());
                }

                //var relay = new SmtpClient("in.mailjet.com", 25)
                //{
                //    Credentials = new NetworkCredential("d457138afc12010427d600e4bdfe44c2", "43d19820b22d74bf5d94ea690c7dbcb5"),
                //    EnableSsl = true
                //};

                //#if NET452
                //                var message = new System.Net.Mail.MailMessage("noreply@unosquare.net", "geovanni.perez@unosquare.com");
                //                message.Attachments.Add(new System.Net.Mail.Attachment("c:\\ESD\\da.png"));

                //                new System.Net.Mail.SmtpClient("localhost", 587)
                //                {
                //                    Credentials = new NetworkCredential("timecore", "givemethemoney"),
                //                    EnableSsl = false
                //                }.Send(message); "unocorp-svc-01.ad.unosquare.com"
                //#else
                var relay = new SmtpClient("unocorp-svc-01.ad.unosquare.com", 587)
                {
                    Credentials = new NetworkCredential("timecore", "givemethemoney"),
                    EnableSsl = false
                };

                relay.SendMailAsync(state).Wait();
                //#endif
            }
            catch (Exception ex)
            {
                ex.Log("Program");
            }

            Terminal.ReadKey(true, true);
        }
    }
}