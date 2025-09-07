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

        //寄給體驗者審核結果
        public static async Task SendReviewResultAsync(int participantId, int programId, string status, string comment, string participantEmail, string programTitle)
        {
            string subject;
            string body;
            bool isHtml = true; 

            if (status == "Approved")
            {
                subject = $"【{programTitle}】審核通知";
                body = $@"
                        <p>恭喜您！您的體驗申請已通過企業審核！</p>
                        <p>請準時參加本次體驗活動。</p>
                        <p><b>公司備註：</b><br>{comment}</p>
                        <p>參與體驗前請確認行前準備清單與注意事項。</p>
                        <p><a href='https://try-b.vercel.app/users/programs/{programId}' target='_blank'>點擊前往體驗計畫頁面</a></p>
                        <p>期待您的參與！</p>";
            }
            else if (status == "Rejected")
            {
                subject = $"【{programTitle}】審核通知";
                body = $@"
                        <p>您好，很遺憾地通知您，您的體驗申請本次未能通過企業審核！</p>
                        <p><b>公司備註：</b><br>{comment}</p>
                        <p>感謝您的申請，期待未來能有機會再次看到您的參與！</p>";
            }
            else
            {
                subject = $"您的體驗申請狀態更新：{status}";
                body = $@"
                        <p>您好，您的體驗申請狀態已更新為：{status}</p>
                        <p><b>備註：</b><br>{comment}</p>";
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
                mailMessage.IsBodyHtml = isHtml; // 使用 HTML

                await client.SendMailAsync(mailMessage);
            }
        }

        // 寄給體驗者可評價
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
            string body = $@"
                            <p>您好！</p>
                            <p>您參加的體驗已經結束，現在可以提交您的體驗評價。</p>
                            <p>
                                <a href='https://try-b.vercel.app/users/comments' target='_blank'>
                                    點擊前往填寫評價
                                </a>
                            </p>
                            <p>感謝您的參與與回饋！</p>";

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
                        Body = body,
                        IsBodyHtml = true
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

        // 寄給企業收到評價
        public static async Task SendEvaluationSubmittedEmailToCompany(
            string companyEmail,
            string participantName,
            string programTitle,
            int score,
            string comment)
        {
            string subject = $"【{programTitle}】收到新的體驗評價";
            string body = $@"
        <p>您好！</p>
        <p>體驗者 <b>{participantName}</b> 已提交對「<b>{programTitle}</b>」的評價。</p>
        <p>
            <b>評分：</b> {score} / 5<br/>
            <b>留言：</b><br/>
            <blockquote style='border-left:3px solid #ccc;padding-left:10px;color:#555;'>
                {System.Net.WebUtility.HtmlEncode(comment).Replace("\n", "<br/>")}
            </blockquote>
        </p>
        <p>
            請登入後台查看完整評價內容：<br/>
            <a href='https://try-b.vercel.app/company/comments' target='_blank'>
                點擊前往查看評價
            </a>
        </p>";

            using (var client = new SmtpClient(SmtpHost, SmtpPort))
            {
                client.Credentials = new NetworkCredential(SmtpUser, SmtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SmtpUser, "Tryβ 短期職業體驗平台"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true 
                };
                mailMessage.To.Add(companyEmail);

                await client.SendMailAsync(mailMessage);
            }
        }

        // 寄給企業：有新的體驗申請
        public static async Task SendNewApplicationEmailToCompany(
            string companyEmail,
            string programTitle,
            string participantName,
            string participantEmail,
            string applicationNumber,
            DateTime submitAt)
        {
            string subject = $"【{programTitle}】收到新的體驗申請";
            string body = $@"
        <p>您好！</p>
        <p>您的體驗計畫「<b>{programTitle}</b>」收到一筆新的體驗申請：</p>
        <table style='border-collapse:collapse;'>
            <tr>
                <td style='padding:4px 8px;'><b>申請人：</b></td>
                <td style='padding:4px 8px;'>{participantName}</td>
            </tr>
            <tr>
                <td style='padding:4px 8px;'><b>Email：</b></td>
                <td style='padding:4px 8px;'><a href='mailto:{participantEmail}'>{participantEmail}</a></td>
            </tr>
            <tr>
                <td style='padding:4px 8px;'><b>申請編號：</b></td>
                <td style='padding:4px 8px;'>{applicationNumber}</td>
            </tr>
            <tr>
                <td style='padding:4px 8px;'><b>申請時間：</b></td>
                <td style='padding:4px 8px;'>{submitAt:yyyy/MM/dd HH:mm}</td>
            </tr>
        </table>
        <p>
            請登入後台查看申請詳情並進行審核：<br/>
            <a href='https://try-b.vercel.app/company' target='_blank'>
                點擊前往後台審核
            </a>
        </p>";

            using (var client = new SmtpClient(SmtpHost, SmtpPort))
            {
                client.Credentials = new NetworkCredential(SmtpUser, SmtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SmtpUser, "Tryβ 短期職業體驗平台"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true 
                };
                mailMessage.To.Add(companyEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}