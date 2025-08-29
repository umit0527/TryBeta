using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
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

        // GET: api/v1/payments/callback 藍新金流：回傳結果給前端
        // 用戶完成付款後，前端瀏覽器導向，方便展示付款結果或前端跳轉
        // 僅用來查詢付款結果並展示，不直接更新訂單
        [HttpGet]
        [Route("callback")]
        public IHttpActionResult PaymentCallbackGet()
        {
            try
            {
                // 1️ 讀取 query string
                var query = HttpContext.Current.Request.QueryString;
                string tradeInfo = query["TradeInfo"];
                string tradeSha = query["TradeSha"];
                string status = query["Status"];

                if (string.IsNullOrEmpty(tradeInfo) || string.IsNullOrEmpty(tradeSha))
                    return BadRequest("缺少必要的回傳參數");

                if (string.IsNullOrEmpty(status) || !status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                    return Ok("1|OK"); // 付款失敗也回傳 1|OK，藍新不重送

                // 2️ 設定 HashKey / HashIV
                string hashKey = ConfigurationManager.AppSettings["Newebpay:HashKey"];
                string hashIV = ConfigurationManager.AppSettings["Newebpay:HashIV"];

                // 3️ 驗證 TradeSha
                string calcSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&TradeInfo={tradeInfo}&HashIV={hashIV}");
                if (!string.Equals(calcSha, tradeSha, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("TradeSha 驗證失敗");

                // 4️ 解密 TradeInfo
                string decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);
                var tradeInfoObj = JsonConvert.DeserializeObject<PaymentTradeInfoDto>(decrypted);

                if (tradeInfoObj == null || string.IsNullOrEmpty(tradeInfoObj.MerchantOrderNo))
                    return BadRequest("TradeInfo 資料異常");

                // 5️ 查訂單
                var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == tradeInfoObj.MerchantOrderNo);
                if (order == null)
                    return BadRequest("訂單不存在");

                // 6️ 查方案
                var plan = db.Plan.Find(order.PlanId);
                if (plan == null)
                    return BadRequest($"方案不存在 (planId={order.PlanId})");

                // 7 判斷付款結果
                bool isPaid = status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);

                // 8 回傳訂單狀態給前端
                var result = new
                {
                    OrderNum = order.OrderNum,
                    CompanyId = order.CompanyId,
                    PlanId = order.PlanId,
                    PaymentStatus = "Paid",
                    OrderStatus = "Active",
                    PaymentMethod = tradeInfoObj.PaymentType ?? order.PaymentMethod,
                    Card4No = tradeInfoObj.Card4No
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/v1/payments/callback 藍新金流：回傳結果給前端 測試
        [HttpGet]
        [Route("callback/test")]
        public IHttpActionResult PaymentCallbackGetTest()
        {
            try
            {
                // ====== 測試用資料 ======
                bool isTest = string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["TradeInfo"]);
                string tradeInfo, tradeSha, status;

                string hashKey = ConfigurationManager.AppSettings["HashKey"];
                string hashIV = ConfigurationManager.AppSettings["HashIV"];

                if (isTest)
                {
                    // 模擬藍新回傳資料
                    var tradeInfoObj1 = new PaymentTradeInfoDto
                    {
                        MerchantOrderNo = "ORD-20250826-002",
                        PaymentType = "CREDIT",
                        Card4No = "1234"
                    };
                    string json = JsonConvert.SerializeObject(tradeInfoObj1 ?? new PaymentTradeInfoDto());
                    if (string.IsNullOrEmpty(json))
                        throw new Exception("TradeInfo JSON 為空");

                    tradeInfo = CryptoUtil.EncryptAESHex(json, hashKey, hashIV);
                    tradeSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&TradeInfo={tradeInfo}&HashIV={hashIV}").ToUpperInvariant();
                    status = "SUCCESS";
                }
                else
                {
                    // 真正藍新回傳
                    var query = HttpContext.Current.Request.QueryString;
                    tradeInfo = query["TradeInfo"];
                    tradeSha = query["TradeSha"];
                    status = query["Status"];
                }

                if (string.IsNullOrEmpty(tradeInfo) || string.IsNullOrEmpty(tradeSha))
                    return BadRequest("缺少必要的回傳參數");

                if (string.IsNullOrEmpty(status) || !status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                    return Ok("1|OK"); // 付款失敗也回傳 1|OK，藍新不重送

                //// 2️ 設定 HashKey / HashIV
                //string hashKey = ConfigurationManager.AppSettings["Newebpay:HashKey"];
                //string hashIV = ConfigurationManager.AppSettings["Newebpay:HashIV"];

                // 3️ 驗證 TradeSha
                string calcSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&TradeInfo={tradeInfo}&HashIV={hashIV}");
                if (!string.Equals(calcSha, tradeSha, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("TradeSha 驗證失敗");

                // 4️ 解密 TradeInfo
                string decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);
                var tradeInfoObj = JsonConvert.DeserializeObject<PaymentTradeInfoDto>(decrypted);

                if (tradeInfoObj == null || string.IsNullOrEmpty(tradeInfoObj.MerchantOrderNo))
                    return BadRequest("TradeInfo 資料異常");

                // 5️ 查訂單
                var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == tradeInfoObj.MerchantOrderNo);
                if (order == null)
                    return BadRequest("訂單不存在");

                // 6️ 查方案
                var plan = db.Plan.Find(order.PlanId);
                if (plan == null)
                    return BadRequest($"方案不存在 (planId={order.PlanId})");

                // 7 判斷付款結果
                bool isPaid = status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);

                // 8 回傳訂單狀態給前端
                var result = new
                {
                    OrderNum = order.OrderNum,
                    CompanyId = order.CompanyId,
                    PlanId = order.PlanId,
                    PaymentStatus = "Paid",
                    OrderStatus = "Active",
                    PaymentMethod = tradeInfoObj.PaymentType ?? order.PaymentMethod,
                    Card4No = tradeInfoObj.Card4No
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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

        // POST: api/v1/payments 藍新金流：建立訂單
        [HttpPost]
        [Route("")]
        [JwtAuthFilter]
        public IHttpActionResult SetChargeData([FromBody] ChargeRequestDto chargeData)
        {
            if (chargeData == null)
                return BadRequest("請傳送有效的 chargeData JSON");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (chargeData.PlanId <= 0)
                return BadRequest("plan_id 不可為 0");

            var plan = db.Plan.Find(chargeData.PlanId);
            if (plan == null)
                return BadRequest($"方案不存在 (planId={chargeData.PlanId})");

            var company = db.Companyinfoes.Find(chargeData.CompanyId);
            if (company == null)
                return BadRequest("企業不存在或無權限");

            // 建立訂單
            // 取得今天日期字串
            string dateStr = DateTime.Now.ToString("yyyyMMdd");

            // 計算今天已經有多少訂單，先抓今天的訂單到記憶體
            var todayOrders = db.CompanyPlanOrders
                .AsEnumerable() // <- 這會把資料拉到記憶體
                .Where(o => o.OrderNum.StartsWith($"ORD-{dateStr}-"))
                .Count();

            // 流水號 +1
            string sequence = (todayOrders + 1).ToString("D3"); // 3位數，不足補0

            // 組成訂單編號
            string merchantOrderNo = $"ORD-{dateStr}-{sequence}";

            var order = new CompanyPlanOrder
            {
                OrderNum = merchantOrderNo,
                CompanyId = chargeData.CompanyId,
                PlanId = chargeData.PlanId,
                Price = plan.Price,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(plan.DurationDays),
                PaymentStatus = "Pending",
                OrderStatus = "Created",
                PaymentMethod = string.IsNullOrEmpty(chargeData.PaymentMethod) ? "CreditCard" : chargeData.PaymentMethod,
                Card4No = "",
            };
            db.CompanyPlanOrders.Add(order);
            db.SaveChanges();

            // 生成藍新金流 TradeInfo / TradeSha (JSON 格式)
            string hashKey = ConfigurationManager.AppSettings["HashKey"];
            string hashIV = ConfigurationManager.AppSettings["HashIV"];
            string merchantID = ConfigurationManager.AppSettings["MerchantID"];
            string version = "2.0";

            // 將所有資料先轉成 JSON
            var tradeDict = new
            {
                MerchantID = merchantID,
                RespondType = "JSON",
                TimeStamp = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                Version = version,
                MerchantOrderNo = merchantOrderNo,
                Amt = plan.Price,
                ItemDesc = plan.Name,
                TradeLimit = 600,
                NotifyURL = chargeData.NotifyUrl,
                ReturnURL = chargeData.ReturnUrl,
                Email = chargeData.Email,
                LoginType = 0, // 0 表示不需要登入藍新金流會員
                CREDIT = order.PaymentMethod == "CreditCard" ? "1" : "0"
            };

            string tradeJson = JsonConvert.SerializeObject(tradeDict);
            string tradeInfo = CryptoUtil.EncryptAESHex(tradeJson, hashKey, hashIV);
            string tradeSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");

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
                },
                PayGetWay = "https://ccore.newebpay.com/MPG/mpg_getway"
            });
        }

        // POST: api/v1/payments/callback 藍新金流：根據結果更改資料庫
        // 後台自動回傳、server-to-server callback、可靠更新訂單
        [HttpPost]
        [Route("callback")]
        public IHttpActionResult PaymentCallbackPost()
        {
            try
            {
                string tradeInfo = null, tradeSha = null, status = null;

                // 1️ 判斷 Content-Type
                var contentType = HttpContext.Current.Request.ContentType?.ToLower();

                if (!string.IsNullOrEmpty(contentType) && contentType.Contains("application/json"))
                {
                    // JSON 模式 (本地測試)
                    using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                    {
                        var body = reader.ReadToEnd();
                        var json = JsonConvert.DeserializeObject<JObject>(body);
                        tradeInfo = (string)json["TradeInfo"];
                        tradeSha = (string)json["TradeSha"];
                        status = (string)json["Status"];
                    }
                }
                else
                {
                    // form-data 模式 (藍新正式回傳)
                    var form = HttpContext.Current.Request.Form;
                    tradeInfo = form["TradeInfo"];
                    tradeSha = form["TradeSha"];
                    status = form["Status"];
                }

                // 2️ 驗證必要參數
                if (string.IsNullOrEmpty(tradeInfo) || string.IsNullOrEmpty(tradeSha))
                    return BadRequest("缺少必要的回傳參數");

                if (string.IsNullOrEmpty(status) || !status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                    return Ok("1|OK"); // 付款失敗也回傳固定字串

                // 3️ 設定 HashKey / HashIV
                string hashKey = ConfigurationManager.AppSettings["HashKey"];
                string hashIV = ConfigurationManager.AppSettings["HashIV"];

                // 4️ 驗證 TradeSha
                string calcSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");
                if (!string.Equals(calcSha, tradeSha, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("TradeSha 驗證失敗");

                // 5️ 解密 TradeInfo
                string decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);
                var tradeInfoObj = JsonConvert.DeserializeObject<PaymentTradeInfoDto>(decrypted);

                if (tradeInfoObj == null || string.IsNullOrEmpty(tradeInfoObj.MerchantOrderNo))
                    return BadRequest("TradeInfo 資料異常");

                // 6️ 查訂單
                var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == tradeInfoObj.MerchantOrderNo);
                if (order == null)
                    return BadRequest("訂單不存在");

                // 7️ 查方案
                var plan = db.Plan.Find(order.PlanId);
                if (plan == null)
                    return BadRequest($"方案不存在 (planId={order.PlanId})");

                // 8️ 更新訂單狀態
                order.PaymentStatus = "Paid";
                order.OrderStatus = "Active";
                order.PaymentMethod = tradeInfoObj.PaymentType ?? order.PaymentMethod;
                order.Card4No = tradeInfoObj.Card4No;
                order.UpdatedAt = DateTime.Now;
                order.PaidAt = DateTime.Now;

                // 9️ 更新方案使用額度
                AddOrUpdatePlanUsage(order.CompanyId, order.PlanId, plan.MaxParticipants, plan.DurationDays);

                db.SaveChanges();

                // 10 回傳藍新固定字串
                return Ok("1|OK");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/v1/payments/result 藍新金流：根據結果導引前端頁面
        [HttpPost]
        [Route("result")]
        public IHttpActionResult Result()
        {
            string hashKey = ConfigurationManager.AppSettings["Newebpay:HashKey"];
            string hashIV = ConfigurationManager.AppSettings["Newebpay:HashIV"];

            var form = HttpContext.Current.Request.Form;
            string tradeInfo = form["TradeInfo"];
            string tradeSha = form["TradeSha"];

            // 驗證 TradeSha
            string calcSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");
            if (!string.Equals(calcSha, tradeSha, StringComparison.OrdinalIgnoreCase))
                return BadRequest("TradeSha 驗證失敗");

            // 解密 TradeInfo
            var decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);

            // 解析
            var result = JsonConvert.DeserializeObject<PaymentTradeInfoDto>(decrypted);

            // 防呆檢查 RespondCode
            if (result == null || string.IsNullOrEmpty(result.RespondCode) || string.IsNullOrEmpty(result.MerchantOrderNo))
            {
                // 導向失敗頁面，或可先導向通用錯誤頁
                return Redirect($"https://trybeta.rocekt.coding.com/payment/failed");
            }

            // 檢查訂單
            var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == result.MerchantOrderNo);
            if (order != null && result.RespondCode == "00")
            {
                var plan = db.Plan.Find(order.PlanId);
                if (plan != null)
                {
                    AddOrUpdatePlanUsage(order.CompanyId, order.PlanId, plan.MaxParticipants, plan.DurationDays);
                }
            }

            // Redirect 到前端頁面
            if (result.RespondCode == "00")
                return Redirect($"https://pages/company/purchase/success?order={result.MerchantOrderNo}");
            else
                return Redirect($"https://pages/company/purchase/index");
            //return Redirect($"https://trybeta.rocekt.coding.com/payment/failed?order={result.MerchantOrderNo}");
        }

        // POST: api/v1/payments/result/test 藍新金流：根據結果導引前端頁面 測試
        [HttpPost]
        [Route("result/test")]
        public IHttpActionResult ResultTest()
        {
            try
            {
                // ====== 測試用資料 ======
                string hashKey = ConfigurationManager.AppSettings["HashKey"];
                string hashIV = ConfigurationManager.AppSettings["HashIV"];

                // 模擬藍新回傳資料
                var tradeInfoObj = new PaymentTradeInfoDto
                {
                    MerchantOrderNo = "ORD-20250826-002", // 請確認資料庫已有這筆訂單
                    RespondCode = "00",                     // 成功
                    PaymentType = "CREDIT",
                    Card4No = "1234"
                };

                string json = JsonConvert.SerializeObject(tradeInfoObj);
                string tradeInfo = CryptoUtil.EncryptAESHex(json, hashKey, hashIV);
                string tradeSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}").ToUpperInvariant();

                // ====== 驗證 TradeSha (模擬正確) ======
                string calcSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");
                if (!string.Equals(calcSha, tradeSha, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("TradeSha 驗證失敗");

                // ====== 解密 TradeInfo ======
                var decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);
                var result = JsonConvert.DeserializeObject<PaymentTradeInfoDto>(decrypted);

                // ====== 檢查訂單 ======
                var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == result.MerchantOrderNo);
                if (order != null && result.RespondCode == "00")
                {
                    var plan = db.Plan.Find(order.PlanId);
                    if (plan != null)
                    {
                        AddOrUpdatePlanUsage(order.CompanyId, order.PlanId, plan.MaxParticipants, plan.DurationDays);
                    }
                }

                // ====== 回傳給前端測試 ======
                return Ok(new
                {
                    Message = "測試付款成功",
                    RedirectUrl = $"https://pages/company/purchase/success?order={result.MerchantOrderNo}",
                    TradeInfo = tradeInfo,
                    TradeSha = tradeSha
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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