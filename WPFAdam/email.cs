using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WPFAdam
{
    public class email
    {
        public string MailAddress = "testmatthijs@outlook.com";
        public string Password = "hT04dYfIubab50otDM6V";
        public string lastcontent = "";
        private BackgroundWorker wrkr = new BackgroundWorker();
        public event MailreceivedEventHandler mailReceived;

        public email(string adress, string password)
        {
            this.MailAddress = adress;
            this.Password = password;
            wrkr.DoWork += Wrkr_DoWork;
            wrkr.RunWorkerCompleted += Wrkr_RunWorkerCompleted;
            wrkr.RunWorkerAsync();
        }

        private void Wrkr_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            wrkr.RunWorkerAsync();
            string res = (string)e.Result;
            mailReceived(res);
        }

        private void Wrkr_DoWork(object sender, DoWorkEventArgs e)
        {
            string res =  ontvang();
            if(res != "")
            {
                e.Result = res;
            }
        }

        public email()
        {
            wrkr.DoWork += Wrkr_DoWork;
            wrkr.RunWorkerCompleted += Wrkr_RunWorkerCompleted;
            wrkr.RunWorkerAsync();
        }
        public string ontvang()
        {

            using (Imap imap = new Imap())
            {
                try
                {
                    imap.ConnectSSL("imap-mail.outlook.com");   // or ConnectSSL for SSL
                    imap.UseBestLogin(MailAddress, Password);

                    imap.SelectInbox();
                    List<long> uids = imap.Search(Flag.All);

                    for (int i = uids.Count; i > 0; i--)
                    {
                        long uid = uids[i - 1];
                        var eml = imap.GetMessageByUID(uid);
                        IMail email = new MailBuilder()
                            .CreateFromEml(eml);
                        if (email.Subject == "Adam control")
                        {
                            if(email.Text != lastcontent)
                            {
                                lastcontent = email.Text;
                                return email.Text;
                            }
                            i = 0;
                        }
                        
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    imap.Close();
                    
                }
            }
            return "";

        }
        public void verzend(string mess)
        {
            var fromAddress = new MailAddress(MailAddress, "From Name");
            var toAddress = new MailAddress(MailAddress, "To Name");
            string fromPassword = Password;
            string subject = "Message from your adam module" + DateTime.Today.ToShortDateString();

            var smtp = new SmtpClient
            {
                Host = "smtp-mail.outlook.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = mess
            })
            {
                smtp.Send(message);
            }
        }

        public delegate void MailreceivedEventHandler(string content);
    }
}
