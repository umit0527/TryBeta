using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
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
                string hashKey = ConfigurationManager.AppSettings["HashKey"];
                string hashIV = ConfigurationManager.AppSettings["HashIV"];

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

        // POST: api/v1/orders 建立方案訂單
        [HttpPost]
        [Route("~/api/v1/orders")]
        [JwtAuthFilter]
        public IHttpActionResult CreateCompanyPlanOrder([FromBody] CompanyPlanOrder dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("缺少必要資料");

                // 1. 驗證 Company 是否存在
                var company = db.Companyinfoes.FirstOrDefault(c => c.Id == dto.CompanyId);
                if (company == null)
                    return NotFound();

                // 2. 驗證 Plan 是否存在
                var plan = db.Plan.FirstOrDefault(p => p.Id == dto.PlanId);
                if (plan == null)
                    return NotFound();

                // 3. 日期驗證
                if (dto.EndDate.HasValue && dto.EndDate <= dto.StartDate)
                    return BadRequest("結束日期必須大於開始日期");

                // 4. 自動產生訂單編號（ORD-YYYYMMDD-流水號）
                var today = DateTime.Now.ToString("yyyyMMdd");
                var prefix = "ORD-" + today;

                // 計算今天已存在訂單數量
                var countToday = db.CompanyPlanOrders.Count(o => o.OrderNum.StartsWith(prefix)) + 1;

                // 三位數流水號
                var orderNum = $"{prefix}-{countToday:D3}";

                // 5. 建立訂單（避免前端亂傳敏感欄位）
                var order = new CompanyPlanOrder
                {
                    OrderNum = orderNum,
                    CompanyId = dto.CompanyId,
                    PlanId = dto.PlanId,
                    Price = dto.Price,
                    PurchaseDate = DateTime.Now,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    OrderStatus = "Created",
                    PaymentStatus = string.IsNullOrEmpty(dto.PaymentStatus) ? "Pending" : dto.PaymentStatus,
                    PaymentMethod = dto.PaymentMethod,
                    Card4No = dto.Card4No,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PaidAt = dto.PaymentStatus == "Paid" ? (DateTime?)DateTime.Now : null
                };

                db.CompanyPlanOrders.Add(order);
                db.SaveChanges();

                // 6. 精簡回傳資料，避免外鍵敏感資訊洩漏
                return Ok(new
                {
                    message = "訂單建立成功",
                    data = new
                    {
                        order.Id,
                        order.OrderNum,
                        order.Price,
                        order.PaymentStatus,
                        order.PaymentMethod,
                        order.PurchaseDate,
                        order.StartDate,
                        order.EndDate
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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

            try
            {
                // 生成訂單號，每次 +1
                string dateStr = DateTime.Now.ToString("yyyyMMdd");

                // 今天所有訂單數量 (包含未付款、已付款、失敗)
                var todayOrders = db.CompanyPlanOrders
                    .AsEnumerable()
                    .Count(o => o.OrderNum.StartsWith($"ORD{dateStr}"));

                string sequence = (todayOrders + 1).ToString("D3"); // 三位數補0
                string merchantOrderNo = $"ORD{dateStr}{sequence}";

                int amt = (int)Math.Round(plan.Price, 0);

                var order = new CompanyPlanOrder
                {
                    OrderNum = merchantOrderNo,
                    CompanyId = chargeData.CompanyId,
                    PlanId = chargeData.PlanId,
                    Price = amt,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(plan.DurationDays),
                    PaymentStatus = "Pending",
                    OrderStatus = "Created",
                    PaymentMethod = string.IsNullOrEmpty(chargeData.PaymentMethod) ? "CreditCard" : chargeData.PaymentMethod,
                    Card4No = "",
                };
                db.CompanyPlanOrders.Add(order);
                db.SaveChanges();

                // 生成藍新金流 TradeInfo / TradeSha
                string hashKey = ConfigurationManager.AppSettings["HashKey"];
                string hashIV = ConfigurationManager.AppSettings["HashIV"];
                string merchantID = ConfigurationManager.AppSettings["MerchantID"];
                string version = "2.0";

                string credit = order.PaymentMethod == "CreditCard" ? "1" : "0";
                string webatm = order.PaymentMethod == "WebATM" ? "1" : "0";
                string cvs = order.PaymentMethod == "CVS" ? "1" : "0";
                string googlepay = order.PaymentMethod == "GooglePay" ? "1" : "0";
                string applepay = order.PaymentMethod == "ApplePay" ? "1" : "0";
                string samsungpay = order.PaymentMethod == "SamsungPay" ? "1" : "0";
                string twpayatm = order.PaymentMethod == "TWPayATM" ? "1" : "0";
                string barcode = order.PaymentMethod == "Barcode" ? "1" : "0";

                var tradeParams = new Dictionary<string, string>
        {
            { "MerchantID", merchantID },
            { "RespondType", "JSON" },
            { "TimeStamp", ((int)(DateTime.Now - new DateTime(1970,1,1)).TotalSeconds).ToString() },
            { "Version", version },
            { "MerchantOrderNo", order.OrderNum },
            { "Amt", order.Price.ToString() },
            { "ItemDesc", plan.Name },
            { "TradeLimit", "600" },
            { "ReturnURL", "https://try-b.vercel.app/api/v1/company/payments/return" },    // 前端顯示用
            { "NotifyURL", "https://trybeta.rocket-coding.com/api/v1/payments/callback" }, // 後端驗證用
            { "Email", chargeData.Email },
            { "LoginType", "0" },
            { "CREDIT", credit },
            { "WEBATM", webatm },
            { "VACC", cvs },
            { "GooglePay", googlepay },
            { "ApplePay", applepay },
            { "SamsungPay", samsungpay },
            { "TWPayATM", twpayatm },
            { "Barcode", barcode }
        };

                string tradeInfoStr = string.Join("&", tradeParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                string tradeInfo = CryptoUtil.EncryptAESHex(tradeInfoStr, hashKey, hashIV);
                string tradeSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");

                return Ok(new
                {
                    Status = true,
                    OrderNum = order.OrderNum,
                    Amt = order.Price.ToString(),
                    PaymentData = new
                    {
                        MerchantID = merchantID,
                        TradeInfo = tradeInfo,
                        TradeSha = tradeSha,
                        Version = version
                    },
                    PayGetWay = "https://ccore.newebpay.com/MPG/mpg_gateway"
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    Status = false,
                    Message = "建立訂單失敗",
                    Error = ex.Message
                });
            }
        }

        // POST: api/v1/payments/callback 藍新金流：根據結果更改資料庫
        [HttpPost]
        [Route("callback")]
        public IHttpActionResult PaymentCallbackPost()
        {
            string logInfo = "";
            try
            {
                // 1️ 讀取藍新回傳表單
                var form = HttpContext.Current.Request.Form;
                string tradeInfo = form["TradeInfo"];
                string tradeSha = form["TradeSha"];

                logInfo += $"Received TradeInfo (前 20 字): {(tradeInfo?.Length > 20 ? tradeInfo.Substring(0, 20) + "..." : tradeInfo)}\n";

                // 2️ 驗證必要參數
                if (string.IsNullOrEmpty(tradeInfo) || string.IsNullOrEmpty(tradeSha))
                {
                    logInfo += "ERROR: Missing required parameters\n";
                    WriteLog(logInfo);
                    return Ok("1|OK");
                }

                // 3️ 取得 HashKey / HashIV
                string hashKey = ConfigurationManager.AppSettings["HashKey"];
                string hashIV = ConfigurationManager.AppSettings["HashIV"];
                if (string.IsNullOrEmpty(hashKey) || string.IsNullOrEmpty(hashIV))
                {
                    logInfo += "ERROR: HashKey or HashIV not found in config\n";
                    WriteLog(logInfo);
                    return Ok("1|OK");
                }

                // 4️ 驗證 SHA
                string calcSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");
                if (!string.Equals(calcSha, tradeSha, StringComparison.OrdinalIgnoreCase))
                {
                    logInfo += $"ERROR: SHA verification failed. Calculated: {calcSha}, Received: {tradeSha}\n";
                    WriteLog(logInfo);
                    return Ok("1|OK");
                }

                // 5️ 解密 TradeInfo
                string decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);
                logInfo += $"Decrypted TradeInfo: {decrypted}\n";

                // 6️ 解析交易結果 JSON，使用 JObject 取欄位
                var jsonObj = JObject.Parse(decrypted);
                var resultObj = jsonObj["Result"];
                if (resultObj == null || resultObj["MerchantOrderNo"] == null)
                {
                    logInfo += "ERROR: Failed to parse TradeInfo or missing MerchantOrderNo\n";
                    WriteLog(logInfo);
                    return Ok("1|OK");
                }

                string merchantOrderNo = resultObj["MerchantOrderNo"].ToString();
                string respondCode = resultObj["RespondCode"]?.ToString();
                string paymentType = resultObj["PaymentType"]?.ToString();
                string card4No = resultObj["Card4No"]?.ToString();

                // 7️ 使用 Transaction 確保資料一致性
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == merchantOrderNo);
                        if (order == null)
                        {
                            logInfo += $"ERROR: Order not found: {merchantOrderNo}\n";
                            WriteLog(logInfo);
                            transaction.Rollback();
                            return Ok("1|OK");
                        }

                        if (order.PaymentStatus == "Paid")
                        {
                            logInfo += $"INFO: Order {order.OrderNum} already processed\n";
                            WriteLog(logInfo);
                            transaction.Rollback();
                            return Ok("1|OK");
                        }

                        if (respondCode == "00") // 成功
                        {
                            order.PaymentStatus = "Paid";
                            order.PaymentMethod = paymentType ?? order.PaymentMethod;
                            order.Card4No = card4No;
                            order.PaidAt = DateTime.Now;

                            db.SaveChanges(); // 先更新訂單狀態，不改 StartDate / EndDate
                        }
                        else
                        {
                            order.PaymentStatus = "Failed";
                            order.OrderStatus = "Failed";
                            order.UpdatedAt = DateTime.Now;
                            db.SaveChanges();
                            transaction.Commit();
                            WriteLog($"INFO: Order {order.OrderNum} marked as failed\n");
                            return Ok("1|OK");
                        }

                        var plan = db.Plan.Find(order.PlanId);
                        if (plan == null)
                        {
                            logInfo += $"ERROR: Plan not found: {order.PlanId}\n";
                            WriteLog(logInfo);
                            transaction.Rollback();
                            return Ok("1|OK");
                        }

                        var usage = AddOrUpdatePlanUsage(order.CompanyId, order.PlanId, plan.MaxParticipants, plan.DurationDays);

                        order.OrderStatus = "Active";
                        order.UpdatedAt = DateTime.Now;

                        db.SaveChanges();
                        transaction.Commit();

                        logInfo += $"SUCCESS: Order {order.OrderNum} updated, PlanUsage synced\n";
                    }
                    catch (Exception exTrans)
                    {
                        transaction.Rollback();
                        logInfo += $"DATABASE ERROR: {exTrans.Message}\n{exTrans.StackTrace}\n";
                        WriteLog(logInfo);
                        return Ok("1|OK");
                    }
                }

                WriteLog(logInfo);
                return Ok("1|OK");
            }
            catch (Exception ex)
            {
                logInfo += $"EXCEPTION: {ex.Message}\n{ex.StackTrace}\n";
                WriteLog(logInfo);
                return Ok("1|OK");
            }
        }

        // 輔助方法：寫入日誌
        private void WriteLog(string content)
        {
            try
            {
                var logPath = System.Web.HttpContext.Current.Server.MapPath("~/newebpay_callback_log.txt");
                // 確保目錄存在
                var directory = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                System.IO.File.AppendAllText(logPath, content);
            }
            catch (Exception logEx)
            {
                // 日誌寫入失敗也不應該影響主要流程
                // 可以考慮寫入 Windows Event Log 或其他備用方案
            }
        }

        //// POST: api/v1/payments/result 藍新金流：根據結果導引前端頁面  Form-Data
        //[HttpPost]
        //[Route("result")]
        //public IHttpActionResult Result()
        //{
        //    try
        //    {
        //        var form = HttpContext.Current.Request.Form;
        //        string tradeInfo = form["TradeInfo"];
        //        string tradeSha = form["TradeSha"];

        //        if (string.IsNullOrEmpty(tradeInfo) || string.IsNullOrEmpty(tradeSha))
        //        {
        //            return BadRequest("TradeInfo or TradeSha missing");
        //        }

        //        string hashKey = ConfigurationManager.AppSettings["HashKey"];
        //        string hashIV = ConfigurationManager.AppSettings["HashIV"];

        //        // 驗證 SHA
        //        string checkSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}").ToUpper();
        //        if (checkSha != tradeSha.ToUpper())
        //            return BadRequest("SHA verification failed");

        //        // 解密 TradeInfo
        //        string decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);

        //        var dict = decrypted.Split('&')
        //            .Select(p => p.Split('='))
        //            .ToDictionary(k => k[0], v => v.Length > 1 ? v[1] : "");

        //        string orderNo = dict["MerchantOrderNo"];
        //        string amt = dict["Amt"];
        //        string paymentMethod = dict.ContainsKey("PaymentMethod") ? dict["PaymentMethod"] : "";
        //        string card4No = dict.ContainsKey("Card4No") ? dict["Card4No"] : "";
        //        string respondCode = dict.ContainsKey("RespondCode") ? dict["RespondCode"] : "";

        //        // 更新資料庫
        //        var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == orderNo);
        //        if (order != null)
        //        {
        //            order.PaymentStatus = respondCode == "00" ? "Paid" : "Failed";
        //            order.OrderStatus = order.PaymentStatus == "Paid" ? "Active" : "Failed";
        //            order.Card4No = card4No;
        //            order.UpdatedAt = DateTime.Now;
        //            db.SaveChanges();
        //        }

        //        // 回傳前端 JSON
        //        return Ok(new
        //        {
        //            status = respondCode == "00" ? "Success" : "Failed",
        //            orderNo,
        //            amount = amt,
        //            paymentMethod,
        //            card4No
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new { status = "Error", message = ex.Message });
        //    }
        //}

        //// POST: api/v1/payments/result/test 藍新金流：根據結果導引前端頁面 測試
        //[HttpPost]
        //[Route("result/test")]
        //public IHttpActionResult ResultTest()
        //{
        //    try
        //    {
        //        // ====== 測試用資料 ======
        //        string hashKey = ConfigurationManager.AppSettings["HashKey"];
        //        string hashIV = ConfigurationManager.AppSettings["HashIV"];

        //        // 模擬藍新回傳資料
        //        var tradeInfoObj = new PaymentTradeInfoDto
        //        {
        //            MerchantOrderNo = "ORD-20250826-002", // 請確認資料庫已有這筆訂單
        //            RespondCode = "00",                     // 成功
        //            PaymentType = "CREDIT",
        //            Card4No = "1234"
        //        };

        //        string json = JsonConvert.SerializeObject(tradeInfoObj);
        //        string tradeInfo = CryptoUtil.EncryptAESHex(json, hashKey, hashIV);
        //        string tradeSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}").ToUpperInvariant();

        //        // ====== 驗證 TradeSha (模擬正確) ======
        //        string calcSha = CryptoUtil.EncryptSHA256($"HashKey={hashKey}&{tradeInfo}&HashIV={hashIV}");
        //        if (!string.Equals(calcSha, tradeSha, StringComparison.OrdinalIgnoreCase))
        //            return BadRequest("TradeSha 驗證失敗");

        //        // ====== 解密 TradeInfo ======
        //        var decrypted = CryptoUtil.DecryptAESHex(tradeInfo, hashKey, hashIV);
        //        var result = JsonConvert.DeserializeObject<PaymentTradeInfoDto>(decrypted);

        //        // ====== 檢查訂單 ======
        //        var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == result.MerchantOrderNo);
        //        if (order != null && result.RespondCode == "00")
        //        {
        //            var plan = db.Plan.Find(order.PlanId);
        //            if (plan != null)
        //            {
        //                AddOrUpdatePlanUsage(order.CompanyId, order.PlanId, plan.MaxParticipants, plan.DurationDays);
        //            }
        //        }

        //        // ====== 回傳給前端測試 ======
        //        return Ok(new
        //        {
        //            Message = "測試付款成功",
        //            RedirectUrl = $"https://pages/company/purchase/success?order={result.MerchantOrderNo}",
        //            TradeInfo = tradeInfo,
        //            TradeSha = tradeSha
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

        // DELETE: api/Payment/5

        // POST: api/v1/payments/result 藍新金流：根據結果導引前端頁面  Json
        [HttpPost]
        [Route("result")]
        public IHttpActionResult Result([FromBody] PaymentCallbackDto req)
        {
            try
            {
                if (req == null || string.IsNullOrEmpty(req.TradeInfo))
                    return BadRequest("TradeInfo missing");

                // 取得 AES 金鑰
                string hashKey = ConfigurationManager.AppSettings["HashKey"];
                string hashIV = ConfigurationManager.AppSettings["HashIV"];

                // 解密 TradeInfo
                string decrypted = CryptoUtil.DecryptAESHex(req.TradeInfo, hashKey, hashIV);

                // 將解密後的 JSON 解析
                var jsonObj = JObject.Parse(decrypted);
                var resultObj = jsonObj["Result"];
                if (resultObj == null || resultObj["MerchantOrderNo"] == null)
                    return BadRequest("MerchantOrderNo missing in TradeInfo");

                // 取得訂單資料
                string orderNo = resultObj["MerchantOrderNo"].ToString();
                string tradeNo = resultObj["TradeNo"]?.ToString();
                string amt = resultObj["Amt"]?.ToString();
                string paymentType = resultObj["PaymentType"]?.ToString();

                // 查資料庫訂單
                var order = db.CompanyPlanOrders.FirstOrDefault(o => o.OrderNum == orderNo);
                if (order == null)
                    return Ok(new { status = "Error", message = "Order not found" });

                // 取得付款狀態
                string status = order.PaymentStatus == "Paid" ? "Success" : "Failed";

                return Ok(new
                {
                    status,
                    orderNo,
                    tradeNo,
                    amount = amt,
                    paymentMethod = paymentType ?? ""
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = "Error", message = ex.Message });
            }
        }


        // 累加方案使用額度或新增一筆新的使用紀錄
        private PlanUsage AddOrUpdatePlanUsage(int companyId, int planId, int purchasedPeople, int durationDays)
        {
            // 找出同公司最後一筆未過期的使用紀錄（不分 planId）
            var currentUsage = db.PlanUsage
                .Where(p => p.CompanyId == companyId && p.EndDate >= DateTime.Now)
                .OrderByDescending(p => p.EndDate)
                .FirstOrDefault();

            if (currentUsage != null)
            {
                // 累加剩餘體驗人數
                currentUsage.RemainingPeople += purchasedPeople;

                // 延長結束日期
                currentUsage.EndDate = currentUsage.EndDate.Value.AddDays(durationDays);
                currentUsage.UpdatedAt = DateTime.Now;

                db.SaveChanges();
                return currentUsage;
            }
            else
            {
                // 沒有未過期方案 → 新增
                var newUsage = new PlanUsage
                {
                    CompanyId = companyId,
                    RemainingPeople = purchasedPeople,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(durationDays),
                    StatusId = 1, // Active
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.PlanUsage.Add(newUsage);
                db.SaveChanges();
                return newUsage;
            }
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