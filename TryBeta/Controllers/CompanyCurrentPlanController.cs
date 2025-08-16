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
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int companyId = (int)userIdObj;

                // 2. 找出該企業最新的方案訂單
                var order = db.CompanyPlanOrders
                    .Where(o => o.CompanyId == companyId)
                    .OrderByDescending(o => o.PurchaseDate)
                    .FirstOrDefault();

                if (order == null)
                    return Ok(new { status = "no_plan", message = "尚未購買方案" });

                // 3. 查詢方案資料
                var plan = db.Plan.FirstOrDefault(p => p.Id == order.PlanId);
                if (plan == null)
                    return NotFound();

                // 4. 取得該企業的最新方案使用紀錄
                var planUsage = db.PlanUsage
                    .Include("Plan")
                    .Include("PlanUsageStatus")
                    .Where(p => p.CompanyId == companyId)
                    .OrderByDescending(p => p.StartDate)
                    .FirstOrDefault();

                if (planUsage == null)
                    return Ok(new { status = "expired", message = "方案已過期" });

                // 5. 驗證過期與額滿
                bool changed = false;

                // 過期檢查
                if (planUsage.EndDate.HasValue && planUsage.EndDate.Value.Date < DateTime.Now.Date)
                {
                    planUsage.StatusId = 2; // expired
                    changed = true;
                }
                // 額滿檢查（只在未過期時檢查）
                else if (planUsage.RemainingPeople <= 0)
                {
                    planUsage.StatusId = 4; // full
                    changed = true;
                }

                if (changed)
                    db.SaveChanges();

                // 6. 計算已使用人數與剩餘天數
                int usedParticipants = plan.MaxParticipants - planUsage.RemainingPeople;
                int remainingDays = planUsage.EndDate.HasValue
                                    ? (planUsage.EndDate.Value - DateTime.Now).Days
                                    : 0;
                if (remainingDays < 0) remainingDays = 0;

                // 7. 回傳資料
                var result = new
                {
                    plan_id = plan.Id,
                    status_id = planUsage.StatusId,
                    status_name = planUsage.PlanUsageStatus.Title,
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
