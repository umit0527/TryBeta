using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;

namespace TryBeta.Models
{

    public class EmailService
    {
        private static string SmtpHost = "smtp.gmail.com";
        private static int SmtpPort = 587;
        private static string SmtpUser = "trybeta0910@gmail.com";
        private static string SmtpPass = "jtrn xnvu khnh movu";

        //審核結果
        public static async Task SendReviewResultAsync(int participantId, string status, string comment, string participantEmail, string programTitle)
        {
            string subject;
            string body;

            // 根據審核結果，設定不同的郵件主旨和內文
            if (status == "Approved")
            {
                subject = $"【{programTitle}】審核通知";
                body = $"恭喜您！您的體驗申請已通過企業審核！\n\n" +
                       $"請準時參加本次體驗活動。\n\n" +
                       $"以下是公司給您的備註：\n{comment}\n\n" +
                       $"期待您的參與！";
            }
            else if (status == "Rejected")
            {
                subject = $"【{programTitle}】審核通知";
                body = $"您好，很遺憾地通知您，您的體驗申請本次未能通過企業審核！\n\n" +
                       $"以下是公司給您的備註：\n{comment}\n\n" +
                       $"感謝您的申請，期待未來能有機會再次看到您的參與！";
            }
            else
            {
                // 處理未知的狀態，避免程式碼出錯
                subject = $"您的體驗申請狀態更新：{status}";
                body = $"您好，您的體驗申請狀態已更新為：{status}\n\n" +
                       $"備註：{comment}";
            }

            using (var client = new SmtpClient(SmtpHost, SmtpPort))
            {
                client.Credentials = new NetworkCredential(SmtpUser, SmtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(SmtpUser, "Tryβ 短期職業體驗平台");
                mailMessage.To.Add(participantEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;

                await client.SendMailAsync(mailMessage);
            }
        }

        //可評價
        public static async Task<string> SendEvaluationAvailableEmail(
    TryBetaDbContext db,
    int userId,
    int participantId,
    int programPlanId,
    string serialNum,
    string participantEmail,
    string programTitle)
        {
            string subject = $"【{programTitle}】提交評價";
            string body = $"您好！\n\n" +
                          $"您參加的體驗「{programTitle}」已經結束，您現在可以提交您的體驗評價。\n\n" +
                          $"請點擊以下連結前往填寫評價（示範用連結，可替換為前端頁面 URL）：\n" +
                          $"https://trybeta.rocket-coding.com/users/{userId}/evaluations\n\n" +
                          $"感謝您的參與與回饋！";

            try
            {
                using (var client = new SmtpClient(SmtpHost, SmtpPort))
                {
                    client.Credentials = new NetworkCredential(SmtpUser, SmtpPass);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(SmtpUser, "Tryβ 短期職業體驗平台"),
                        Subject = subject,
                        Body = body
                    };
                    mailMessage.To.Add(participantEmail);

                    await client.SendMailAsync(mailMessage);
                }

                System.Diagnostics.Debug.WriteLine($"可評價 Email 已發送至 {participantEmail}");
                return "Email 已發送成功";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"發送可評價 Email 失敗: {ex.Message}");
                return $"發送可評價 Email 失敗: {ex.Message}";
            }
        }
    }
}