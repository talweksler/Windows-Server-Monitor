using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.IO;
namespace Server_Monitor

{
    using Microsoft.VisualBasic.Devices;
    using System.Management;
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                StringBuilder slacklog = new StringBuilder();
                StringBuilder emailLog = new StringBuilder();
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                int hdThreshold = int.Parse(ConfigurationManager.AppSettings["AVAILABLE_FREE_SPACE_THRESHOLD_IN_MB"]);
                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady && drive.AvailableFreeSpace < hdThreshold)
                    {
                        string log = "Drive: " + drive.Name + " has less then " + hdThreshold + "MB left";
                        slacklog.Append(log + "\r");
                        emailLog.Append(log + "<br/>");
                    }
                }
                ComputerInfo computerInfo = new ComputerInfo();
                double memoryLeft = 100.0 * computerInfo.AvailablePhysicalMemory / computerInfo.TotalPhysicalMemory;
                int memoryThreshold = int.Parse(ConfigurationManager.AppSettings["AVAILABLE_FREE_MEMORY_THRESHOLD_IN_PERCENTAGE"]);
                if (memoryLeft < memoryThreshold)
                {
                    string log = "Memory is below threshold. " + (computerInfo.AvailablePhysicalMemory / (1000 * 1000)) + "MB out of " + (computerInfo.TotalPhysicalMemory / (1000 * 1000)) + "MB";
                    slacklog.Append(log + "\r");
                    emailLog.Append(log + "<br/>");
                }

                if (slacklog.Length > 0)
                {
                    postToSlack(slacklog.ToString());
                    sendEmail(emailLog.ToString());
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private static void postToSlack(string i_message)
        {
            string slackUrl = ConfigurationManager.AppSettings["SLACK_URL"];
            if (string.IsNullOrEmpty(slackUrl))
            {
                return;
            }

            string machine = ConfigurationManager.AppSettings["MACHINE"];
            var slackMessage = new SlackMessage
            {
                Text = machine + ": " + i_message,
                IconEmoji = Emoji.AlarmClock
            };

            SlackClient slackClient = new SlackClient(slackUrl);
            slackClient.Post(slackMessage);
        }

        private static void sendEmail(string i_message)
        {
            try
            {
                string server = ConfigurationManager.AppSettings["SMTP_HOST"];
                if (string.IsNullOrEmpty(server))
                {
                    return;
                }

                string username = ConfigurationManager.AppSettings["SMTP_USER_NAME"];
                string password = ConfigurationManager.AppSettings["SMTP_USER_PASSWORD"];
                int port = int.Parse(ConfigurationManager.AppSettings["SMTP_PORT"]);
                SmtpClient smtpClient = new SmtpClient(server, port);
                smtpClient.EnableSsl = false;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(username, password);

                MailMessage mail = new MailMessage();
                string toEmails = ConfigurationManager.AppSettings["TO_EMAILS"];
                if (string.IsNullOrEmpty(toEmails))
                {
                    return;
                }

                string emailFromAddress = ConfigurationManager.AppSettings["EMAIL_FROM_ADDRESS"];
                string emailFromDisplay = ConfigurationManager.AppSettings["EMAIL_FROM_DISPLAY"];
                mail.From = new MailAddress(emailFromAddress, emailFromDisplay);
                mail.To.Add(toEmails);
                string machine = ConfigurationManager.AppSettings["MACHINE"];
                string subject = ConfigurationManager.AppSettings["EMAIL_SUBJECT"] + machine;
                mail.Subject = "MSMQ Reader Alert";                
                mail.Body = machine + ": " + i_message;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                smtpClient.Send(mail);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
