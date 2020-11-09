using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ShamanClases_CSharp;
using AndroidTransferUploads.Service.Core;


namespace AndroidTransferUploads.Service.Core.Helpers
{
    public class EmailHelpers
    {
        public static bool Send(List<string> To, string Subject, string Body, List<string> PathFiles, Attachment atachment = null)
        {
            //Logger log = LogManager.GetCurrentClassLogger();

            if(To.Count > 0)
            //if (!string.IsNullOrEmpty(To) && new MailAddress(To).Address == To)
            {
                //log.Info("Preparando para el envio a: " + To);
                
                if (ConfigurationManager.AppSettings.Count > 0)
                {
                    //Preparo el cliente SMTP
                    SmtpClient smtpParamedic = new SmtpClient();

                    if (ConfigurationManager.AppSettings["SourceSenderEmail"].ToString() == "webconfig")
                    {
                        smtpParamedic.Host = ConfigurationManager.AppSettings["MailServer"];
                        smtpParamedic.Port = int.Parse(ConfigurationManager.AppSettings["MailPort"]);
                        smtpParamedic.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["MailSSL"]);
                        smtpParamedic.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpParamedic.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MailAddress"], ConfigurationManager.AppSettings["MailPassword"]);
                    }
                    else
                    {
                        AndroidTranferUploads.Core.WSWebApps.WebAppsSoapClient wsClient = new AndroidTranferUploads.Core.WSWebApps.WebAppsSoapClient();

                        DataSet ds = wsClient.GetSenders(3);
                        DataRow dr = ds.Tables[0].Rows[0];
                        smtpParamedic.Host = dr["SmtpServer"].ToString();
                        smtpParamedic.Port = Convert.ToInt32(dr["SmtpPort"]);
                        smtpParamedic.EnableSsl = Convert.ToBoolean(dr["SmtpEnabledSSL"]);
                        smtpParamedic.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpParamedic.Credentials = new NetworkCredential(dr["UsuarioId"].ToString(), dr["Password"].ToString());
                    }

                    //Preparo el EMAIL
                    string FromAdrress = ConfigurationManager.AppSettings["MailAddress"];
                    string FromName = ConfigurationManager.AppSettings["MailFrom"];
                    MailMessage eMail = new MailMessage();
                    foreach (var item in To)
                    {
                        if (!string.IsNullOrEmpty(item) && new MailAddress(item).Address == item)
                            eMail.To.Add(new MailAddress(item));
                    }
                    
                    eMail.From = new MailAddress(FromAdrress, FromName, Encoding.UTF8);
                    eMail.Subject = Subject;
                    eMail.SubjectEncoding = Encoding.UTF8;
                    eMail.Body = Body;
                    eMail.BodyEncoding = Encoding.UTF8;
                    eMail.IsBodyHtml = true;
                    eMail.Priority = MailPriority.High;

                    //Adjunto los archivos que son de los comprobantes
                    if (PathFiles != null)
                    {
                        foreach (string item in PathFiles)
                            eMail.Attachments.Add(new Attachment(item));
                    }
                    else if (atachment != null)
                        eMail.Attachments.Add(atachment);


                    smtpParamedic.Send(eMail);
                    //log.Info("Envio OK");
                    return true;
                }
            }
            return false;
        }
    }
}
