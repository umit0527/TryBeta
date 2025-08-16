using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TryBeta.Models;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/plans/current")]
    public class CompanyCurrentPlanController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();
        /// <summary>
        /// 取得目前企業方案與已使用體驗人數
        /// 需要在 Header 中傳入 Authorization: Bearer {token}
        /// </summary>
        /// <returns>回傳方案基本資料與已使用體驗人數</returns>
        // GET: api/company/current
        [HttpGet]
        [Route("")]
        [JwtAuthFilter]
        public IHttpActionResult GetCurrentPlan()
        {
            try
            {
                // 1. 驗證並取得目前登入的企業 ID
                var authHeader = Request.Headers.Authorization;
                if (authHeader == null || authHeader.Scheme != "Bearer")
                {
                    return Unauthorized();
                }

                var token = authHeader.Parameter;
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized(); // Token 無效或缺少
                }
                int companyId = (int)userIdObj;

                // 2. 找出該企業（未過期）的最新方案訂單
                var order = db.CompanyPlanOrders
                    .Where(o => o.CompanyId == companyId)
                    .OrderByDescending(o => o.PurchaseDate)
                    .FirstOrDefault();

                if (order == null)
                {
                    // 沒有購買方案
                    return Ok(new { status = "no_plan", message = "尚未購買方案" });
                }

                // 3. 查詢方案資料
                var plan = db.Plan.FirstOrDefault(p => p.Id == order.PlanId);
                if (plan == null)
                {
                    return NotFound(); // 找不到對應方案
                }

                // 4. 取得該企業的有效方案
                var planUsage = db.PlanUsage
                    .Include("Plan")
                    .FirstOrDefault(p => p.CompanyId == companyId && p.Status == "active");

                if (planUsage == null)
                {
                    // 方案過期
                    return Ok(new { status = "expired", message = "方案已過期" });
                }

                // 計算已使用的體驗人數
                int usedParticipants = planUsage.Plan.MaxParticipants - planUsage.RemainingPeople;

                // 判斷是否已達上限
                string currentStatus = planUsage.RemainingPeople <= 0 ? "full" : "active";

                // 計算剩餘天數
                int remainingDays = planUsage.EndDate.HasValue
                                    ? (planUsage.EndDate.Value - DateTime.Now).Days
                                    : 0;
                if (remainingDays < 0)
                {
                    remainingDays = 0;
                }

                // 5. 回傳資料
                var result = new
                {
                    plan_id = plan.Id,
                    status = currentStatus,
                    plan_name = plan.Name,
                    plan_price = plan.Price,
                    plan_duration_days = plan.DurationDays,
                    max_participants = plan.MaxParticipants,
                    used_participants = usedParticipants,
                    remaining_people = planUsage.RemainingPeople,
                    start_date = planUsage.StartDate,
                    end_date = planUsage.EndDate,
                    remaining_days = remainingDays,
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
