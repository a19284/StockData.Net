using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace TestData
{

    /// <summary>  
    /// 发送邮件的类  
    /// </summary>  
    public class EmailHelper
    {
        /// <summary>
        /// 发件人密码
        /// </summary>
        private readonly string password;

        /// <summary>
        /// The mail message.
        /// </summary>
        private readonly MailMessage mailMessage;

        /// <summary>
        /// The smtp client.
        /// </summary>
        private SmtpClient smtpClient;
        /// <summary>
        /// Initializes a new instance of the <see cref="SendEMail"/> class.   
        /// 处审核后类的实例  
        /// </summary>
        /// <param name="to">
        /// 收件人地址
        /// </param>
        /// <param name="from">
        /// 发件人地址
        /// </param>
        /// <param name="body">
        /// 邮件正文
        /// </param>
        /// <param name="title">
        /// 邮件的主题
        /// </param>
        /// <param name="password">
        /// </param>
        public EmailHelper()
        {
        }

        public EmailHelper(string to, string from, string body, string title, string password, List<string> files)
        {
            this.mailMessage = new MailMessage();
            string[] toAddress = to.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ad in toAddress)
            {
                this.mailMessage.To.Add(ad);
            }
            this.mailMessage.From = new MailAddress(@from);
            this.mailMessage.Subject = title;
            this.mailMessage.Body = body;
            this.mailMessage.IsBodyHtml = true;
            this.mailMessage.Bcc.Add(new MailAddress(@from));
            this.mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            this.mailMessage.Priority = MailPriority.Normal;
            this.mailMessage.IsBodyHtml = true;
            this.password = password;
            //附件
            if (files != null)
            {
                Attachments(string.Join(",", files.ToArray()));
            }
        }
        public void SendNoticeEmail(string title, string msg)
        {
            EmailHelper email = new EmailHelper("350966707@qq.com", "liqc_518@163.com", msg, title, "moroney100200", new List<string>());
            email.Send();
        }
        /// <summary>  
        /// 添加附件  
        /// </summary>  
        public void Attachments(string paths)
        {
            try
            {
                string[] path = paths.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string t in path)
                {
                    using (var data = new Attachment(t, MediaTypeNames.Application.Octet))
                    {
                        ContentDisposition disposition = data.ContentDisposition;

                        ////获取 附件的创建日期
                        //disposition.CreationDate = System.IO.File.GetCreationTime(t);

                        //// 获取附件的修改日期 
                        //disposition.ModificationDate = System.IO.File.GetLastWriteTime(t);

                        ////获取附 件的读取日期
                        //disposition.ReadDate = System.IO.File.GetLastAccessTime(t);

                        //解决附件乱码问题
                        var atta = new Attachment(t);
                        string name = Path.GetFileName(t);
                        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
                        atta.ContentDisposition.FileName = string.Format("=?utf-8?B?{0}?=", base64);   //指定附件的filename
                        atta.Name = "attachment";           //指定MimePart的Name，不包含中文，这样就不会被BUG影响
                        atta.NameEncoding = Encoding.UTF8;
                        this.mailMessage.Attachments.Add(atta);
                        //添加到附件中
                        //this.mailMessage.Attachments.Add(data);  
                    }
                } 
            }
            catch(Exception exp)
            {
                throw exp;
            } 
        }

        /// <summary>  
        /// 异步发送邮件  
        /// </summary>  
        /// <param name="completedMethod">处理完成委托</param>  
        public void SendAsync(SendCompletedEventHandler completedMethod)
        {
            if (this.mailMessage != null)
            {
                try
                {
                    this.smtpClient = new SmtpClient();

                    //设置发件人身份的票据  
                    smtpClient.Credentials = new System.Net.NetworkCredential(
                        this.mailMessage.From.Address,
                        this.password);
                    smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    if (mailMessage.From.Host.IndexOf("gffunds", System.StringComparison.Ordinal) > -1)
                    {
                        smtpClient.Host = "mail." + this.mailMessage.From.Host;
                    }
                    else
                    {
                        smtpClient.Host = "smtp." + this.mailMessage.From.Host;
                    }

                    //注册异步发送邮件完成时的事件  
                    smtpClient.SendCompleted += completedMethod;
                    smtpClient.SendAsync(this.mailMessage, this.mailMessage.Body);
                }
                catch (Exception exp)
                {
                    throw exp;
                } 
            }
        }

        /// <summary>  
        /// 发送邮件  
        /// </summary>  
        public void Send()
        {
            if (this.mailMessage != null)
            {
                try
                {
                    this.smtpClient = new SmtpClient
                    {
                        Credentials =
                            new System.Net.NetworkCredential(
                            this.mailMessage.From.Address,
                            this.password),
                        DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
                    };

                    this.smtpClient.Host = "smtp." + this.mailMessage.From.Host;
                    this.smtpClient.Send(this.mailMessage);
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    for (int i = 0; i < this.mailMessage.Attachments.Count; i++)
                    {
                        this.mailMessage.Attachments[i].Dispose();
                    }
                }
            }
        }

        /// <summary>  
        /// 发送邮件  
        /// </summary>  
        public void Send(string cc, string bcc)
        {
            if (this.mailMessage != null)
            {
                try
                {
                    mailMessage.Bcc.Clear();
                    if (!string.IsNullOrEmpty(bcc))
                    {
                        string[] bccs = bcc.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < bccs.Length; i++)
                        {
                            if (!mailMessage.Bcc.Contains(new MailAddress(bccs[i])))
                                mailMessage.Bcc.Add(bccs[i]);
                        }
                    }
                    if (!string.IsNullOrEmpty(cc))
                    {
                        string[] ccs = cc.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < ccs.Length; i++)
                        {
                            if (!mailMessage.CC.Contains(new MailAddress(ccs[i])))
                                mailMessage.CC.Add(ccs[i]);
                        }
                    }
                    this.smtpClient = new SmtpClient
                    {
                        Credentials =
                            new System.Net.NetworkCredential(
                            this.mailMessage.From.Address,
                            this.password),
                        DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
                    };

                    this.smtpClient.Host = "smtp." + this.mailMessage.From.Host;
                    this.smtpClient.Send(this.mailMessage);

                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                { 
                    for (int i = 0; i < this.mailMessage.Attachments.Count; i++)
                    {
                        this.mailMessage.Attachments[i].Dispose();
                    }
                }
            }
        }

        /// <summary>  
        /// 发送邮件  
        /// </summary>  
        public void Send(string smtp, int port, string cc, string bcc)
        {
            if (this.mailMessage != null)
            {
                try
                {
                    mailMessage.Bcc.Clear();
                    if (!string.IsNullOrEmpty(bcc))
                    {
                        string[] bccs = bcc.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < bccs.Length; i++)
                        {
                            if (!mailMessage.Bcc.Contains(new MailAddress(bccs[i])))
                                mailMessage.Bcc.Add(bccs[i]);
                        }
                    }
                    if (!string.IsNullOrEmpty(cc))
                    {
                        string[] ccs = cc.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < ccs.Length; i++)
                        {
                            if (!mailMessage.CC.Contains(new MailAddress(ccs[i])))
                                mailMessage.CC.Add(ccs[i]);
                        }
                    }
                    this.smtpClient = new SmtpClient
                    {
                        Credentials =
                            new System.Net.NetworkCredential(
                            this.mailMessage.From.Address,
                            this.password),
                        DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
                    };
                    //if (this.mailMessage.From.Host.IndexOf("gffunds", System.StringComparison.Ordinal) > -1)
                    //{
                    //    this.smtpClient.Host = "mail." + this.mailMessage.From.Host;
                    //}
                    //else
                    //{
                    this.smtpClient.Host = smtp;
                    this.smtpClient.Port = port;
                    //}
                    this.smtpClient.Send(this.mailMessage);
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    for (int i = 0; i < this.mailMessage.Attachments.Count; i++)
                    {
                        this.mailMessage.Attachments[i].Dispose();
                    }
                }
            }
        }
    }
}
