using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/payments")]
    public class PaymentController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Payment
        public IQueryable<CompanyPlanOrder> GetCompanyPlanOrders()
        {
            return db.CompanyPlanOrders;
        }

        // GET: api/Payment/5
        [ResponseType(typeof(CompanyPlanOrder))]
        public IHttpActionResult GetCompanyPlanOrder(int id)
        {
            CompanyPlanOrder companyPlanOrder = db.CompanyPlanOrders.Find(id);
            if (companyPlanOrder == null)
            {
                return NotFound();
            }

            return Ok(companyPlanOrder);
        }

        // PUT: api/Payment/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCompanyPlanOrder(int id, CompanyPlanOrder companyPlanOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != companyPlanOrder.Id)
            {
                return BadRequest();
            }

            db.Entry(companyPlanOrder).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyPlanOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        //POST: api/v1/payments  藍星金流：付款建立訂單
        [HttpPost]
        [Route("")]
        [JwtAuthFilter]
        public IHttpActionResult SetChargeData(ChargeRequestDto chargeData)
        {
            // 1️ 驗證 DTO 與資料
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var plan = db.Plan.Find(chargeData.PlanId);
            if (plan == null)
                return BadRequest("方案不存在");

            var company = db.Companyinfoes.Find(chargeData.CompanyId);
            if (company == null)
                return BadRequest("企業不存在或無權限");

            // 2️ 建立待付款訂單 (PaymentStatus = Pending)
            string merchantOrderNo = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{chargeData.CompanyId}_{chargeData.PlanId}";

            var order = new CompanyPlanOrder
            {
                OrderNum = merchantOrderNo,
                CompanyId = chargeData.CompanyId,
                PlanId = chargeData.PlanId,
                Price = plan.Price,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(plan.DurationDays),
                PaymentStatus = "Pending",    // 尚未付款
                OrderStatus = "Created",      // 訂單已建立
                PaymentMethod = string.IsNullOrEmpty(chargeData.PaymentMethod) ? "CreditCard" : chargeData.PaymentMethod, // 預設 CreditCard
                Card4No = "",             // 付款完成後填入
            };
            db.CompanyPlanOrders.Add(order);
            db.SaveChanges();

            // 3️ 生成藍新金流 TradeInfo / TradeSha
            string hashKey = "1y2UG7Em4yNu1FJsCLOzzRCnvhpckhxd";  // 填入生成的 HashKey
            string hashIV = "CBT9cVlEWmmktdzP";    // 填入生成的 HashIV
            string merchantID = "MS156604019";
            string tradeInfo = "";
            string tradeSha = "";
            string version = "2.0";          // 參考文件串接程式版本

            // tradeInfo 內容，導回的網址都需為 https 
            var tradeData = new List<KeyValuePair<string, string>>()
        {
             new KeyValuePair<string, string>("MerchantID", merchantID),
        new KeyValuePair<string, string>("RespondType", "JSON"),
        new KeyValuePair<string, string>("TimeStamp", ((int)(DateTime.Now - new DateTime(1970,1,1)).TotalSeconds).ToString()),
        new KeyValuePair<string, string>("Version", version),
        new KeyValuePair<string, string>("MerchantOrderNo", merchantOrderNo),
        new KeyValuePair<string, string>("Amt", plan.Price.ToString("0")),
        new KeyValuePair<string, string>("ItemDesc", plan.Name),
        new KeyValuePair<string, string>("TradeLimit", "600"),
        new KeyValuePair<string, string>("NotifyURL", chargeData.NotifyUrl),
        new KeyValuePair<string, string>("ReturnURL", chargeData.ReturnUrl),
        new KeyValuePair<string, string>("Email", chargeData.Email),
        new KeyValuePair<string, string>("LoginType", "0"),
        new KeyValuePair<string, string>("CREDIT", order.PaymentMethod == "CreditCard" ? "1" : "0"), // 如果藍新文件需要這個參數，可以依方式帶值
        };

            // 將 List<KeyValuePair> 轉換成 key1=Value1&key2=Value2 格式
            string tradeQueryPara = string.Join("&", tradeData.Select(x => $"{x.Key}={x.Value}"));
            // AES 加密
            tradeInfo = CryptoUtil.EncryptAESHex(tradeQueryPara, hashKey, hashIV);
            // SHA256 加密
            tradeSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");

            // 4️ 回傳給前端，前端用這些資料導向藍新付款頁面
            return Ok(new
            {
                Status = true,
                OrderNum = merchantOrderNo,
                PaymentData = new
                {
                    MerchantID = merchantID,
                    TradeInfo = tradeInfo,
                    TradeSha = tradeSha,
                    Version = version
                }
            });
        }

        // POST: api/v1/payments/callback 藍星金流：實際付款後更改付款狀態
        [HttpPost]
        [Route("callback")]
        public IHttpActionResult PaymentCallback()
        {
            // 1. 直接從表單中讀取藍新金流回傳的資料
            var form = HttpContext.Current.Request.Form;
            string tradeInfo = form["TradeInfo"];
            string tradeSha = form["TradeSha"];

            if (string.IsNullOrEmpty(tradeInfo) || string.IsNullOrEmpty(tradeSha))
            {
                return BadRequest("缺少必要的回傳參數");
            }

            // 1. 取得金流設定（藍新金流的 Key / IV）
            string hashKey = ConfigurationManager.AppSettings["NewebPay_HashKey"];
            string hashIV = ConfigurationManager.AppSettings["NewebPay_HashIV"];

            // 2. 驗證 TradeSha 是否正確
            string checkValue = $"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}";
            string checkSha = CryptoUtil.EncryptSHA256(checkValue).ToUpper();

            if (checkSha != tradeSha.ToUpper())
            {
                return BadRequest("TradeSha 驗證失敗，資料可能遭竄改");
            }

            // 3. 解密 TradeInfo
            string decryptInfo = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);
            if (string.IsNullOrEmpty(decryptInfo))
            {
                return BadRequest("TradeInfo 解密失敗");
            }

            // 4. 轉成物件（依藍新回傳格式）
            var tradeInfoDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(decryptInfo);

            // 5. 依照交易狀態更新訂單 / 方案
            string status = tradeInfoDict.ContainsKey("Status") ? tradeInfoDict["Status"].ToString() : "";
            string merchantOrderNo = tradeInfoDict.ContainsKey("Result")
                                   ? JsonConvert.DeserializeObject<Dictionary<string, string>>(tradeInfoDict["Result"].ToString())["MerchantOrderNo"]
                                   : "";

            // 6. 更新訂單狀態
            var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == merchantOrderNo);
            if (order == null)
                return BadRequest("訂單不存在");

            if (status == "SUCCESS")
            {
                order.PaymentStatus = "Paid";
                order.OrderStatus = "Active";
                db.SaveChanges();

                return Ok(new { success = true, message = "付款成功", orderNo = merchantOrderNo });
            }
            else
            {
                order.PaymentStatus = "Failed";
                order.OrderStatus = "Cancelled";
                db.SaveChanges();

                return Ok(new { success = false, message = "付款失敗", orderNo = merchantOrderNo });
            }
        }

        // DELETE: api/Payment/5
        [ResponseType(typeof(CompanyPlanOrder))]
        public IHttpActionResult DeleteCompanyPlanOrder(int id)
        {
            CompanyPlanOrder companyPlanOrder = db.CompanyPlanOrders.Find(id);
            if (companyPlanOrder == null)
            {
                return NotFound();
            }

            db.CompanyPlanOrders.Remove(companyPlanOrder);
            db.SaveChanges();

            return Ok(companyPlanOrder);
        }

        // 累加方案使用額度或新增一筆新的使用紀錄
        private void AddOrUpdatePlanUsage(int companyId, int planId, int purchasedPeople, int durationDays)
        {
            // 找出還在有效期的相同方案
            var currentUsage = db.PlanUsage
                .Where(p => p.CompanyId == companyId && p.PlanId == planId && p.EndDate >= DateTime.Now)
                .OrderByDescending(p => p.EndDate)
                .FirstOrDefault();

            if (currentUsage != null)
            {
                // 已有方案 → 累加剩餘體驗人數，延長方案結束日期
                currentUsage.RemainingPeople += purchasedPeople;
                currentUsage.EndDate = currentUsage.EndDate.Value.AddDays(durationDays);
                currentUsage.UpdatedAt = DateTime.Now;
            }
            else
            {
                // 沒有方案 → 新增一筆使用紀錄
                var planUsage = new PlanUsage
                {
                    CompanyId = companyId,
                    PlanId = planId,
                    RemainingPeople = purchasedPeople,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(durationDays),
                    StatusId = 1, // Active
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.PlanUsage.Add(planUsage);
            }

            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CompanyPlanOrderExists(int id)
        {
            return db.CompanyPlanOrders.Count(e => e.Id == id) > 0;
        }
    }
}