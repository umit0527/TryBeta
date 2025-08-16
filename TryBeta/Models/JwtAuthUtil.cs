using Jose;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Web.Configuration;
using TryBeta.Models;

namespace TryBeta.Models
{
    /// <summary>
    /// JwtToken 生成功能
    /// </summary>
    public class JwtAuthUtil
    {
        private static readonly string secretKey = "ILoveCoding";

        //登入時已經確認過帳號密碼，那時候就有連線資料庫，不必再連線
        //private readonly ApplicationDbContext db = new ApplicationDbContext(); // DB 連線

        /// <summary>
        /// 生成 JwtToken
        /// </summary>
        /// <param name="id">會員id</param>
        /// <returns>JwtToken</returns>
        public string GenerateToken(int id, string account, string name)
        {
            // 私鑰：自訂字串，驗證用，用來加密送出的 key (放在 Web.config 的 appSettings)

            //浩哥：不寫在webconfig
            //string secretKey = WebConfigurationManager.AppSettings["TokenKey"]; // 從 appSettings 取出
            //var user = db.User.Find(id); // 進 DB 取出想要夾帶的基本資料

            // payload作法1 需透過 token 傳遞的資料 (可夾帶常用且不重要的資料)
            var payload = new Dictionary<string, object>
            {
                { "Id", id },
                { "Account", account },
                { "NickName", name },
                { "Exp", DateTime.Now.AddMinutes(30).ToString() } // JwtToken 時效設定 30 分
            };

            ////payload作法2
            //Dictionary<string, Object> claim = new Dictionary<string, Object>();//payload 需透過token傳遞的資料
            //claim.Add("Id", user.Id);
            //claim.Add("Account", user.Account);
            //claim.Add("Exp", DateTime.Now.AddSeconds(Convert.ToInt32("100")).ToString());//Token 時效設定100秒
            //var payload = claim;

            // 產生 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 更新：生成只刷新效期的 JwtToken，每有request、就經過這支api，再產生token覆蓋原有的時效
        /// </summary>
        /// <returns>JwtToken</returns>
        public string ExpRefreshToken(Dictionary<string, object> tokenData)
        {
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
            // payload 從原本 token 傳遞的資料沿用，並刷新效期
            var payload = new Dictionary<string, object>
            {
                { "Id", (int)tokenData["Id"] },
                { "Account", tokenData["Account"].ToString() },
                { "NickName", tokenData["NickName"].ToString() },
                { "Exp", DateTime.Now.AddDays(365).ToString() } // JwtToken 時效刷新設定 1年
            };

            //產生刷新時效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 覆蓋原有：生成無效 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string RevokeToken()
        {
            string secretKeyNew = "RevokeToken"; // 故意用不同的 key 生成
            var payload = new Dictionary<string, object>
            {
                { "Id", 0 },
                { "Account", "None" },
                { "NickName", "None" },
                { "Image", "None" },
                { "Exp", DateTime.Now.AddDays(-15).ToString() } // 使 JwtToken 過期 失效
            };

            // 產生失效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKeyNew), JwsAlgorithm.HS512);
            return token;
        }
        /// <summary>
        /// 將 Token 解密取得夾帶的資料
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetPayload(string token)
        {
            return JWT.Decode<Dictionary<string, object>>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
        }
    }
}