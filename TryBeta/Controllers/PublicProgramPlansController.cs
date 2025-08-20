using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;
using static TryBeta.Models.ProgramPlanDto;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/programs")]
    public class PublicProgramPlansController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/PublicProgramPlans
        public IQueryable<ProgramPlan> GetProgramPlan()
        {
            return db.ProgramPlan;
        }

        // GET: api/v1/users/programs 體驗計畫篩選器，包含全部體驗計畫
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetPublicProgramsFilter(
            string search = null,
            int? industry_id = null,
            int? job_title_id = null,
            string city_id = null,
            string sort = "publish_start_desc",
            int page = 1,
            int limit = 21)
        {
            try
            {
                var now = DateTime.Now;

                // 基本查詢：只取已發布且時間有效的計畫
                var query = db.ProgramPlan
                    .Include(p => p.Industry)
                    .Include(p => p.JobTitle)
                    .Include(p => p.Status)
                    .Include(p => p.Steps)
                    .AsQueryable();

                // 🔹 過濾掉不符狀態或刊登過期的體驗
                query = query.Where(p =>
                    (p.StatusId == 2 || p.StatusId == 4 || p.StatusId == 7 || p.StatusId == 15) &&
                    p.PublishEndDate >= now
                );

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        p.Name.Contains(search) ||
                        p.Intro.Contains(search) ||
                        p.Steps.Any(s => s.Name.Contains(search) || s.Description.Contains(search))
                    );
                }

                // 產業篩選
                if (industry_id.HasValue)
                {
                    query = query.Where(p => p.IndustryId == industry_id.Value);
                }

                // 職務篩選
                if (job_title_id.HasValue)
                {
                    query = query.Where(p => p.JobTitleId == job_title_id.Value);
                }

                // 地區篩選
                if (!string.IsNullOrEmpty(city_id))
                {
                    var cityName = db.City
                                     .Where(c => c.Id.ToString() == city_id)
                                     .Select(c => c.Name)
                                     .FirstOrDefault();

                    if (!string.IsNullOrEmpty(cityName))
                    {
                        query = query.Where(p => p.Address.Contains(cityName));
                    }
                }

                // 排序
                switch (sort)
                {
                    case "publish_start_asc":
                        query = query.OrderBy(p => p.PublishStartDate);
                        break;
                    case "publish_start_desc":
                        query = query.OrderByDescending(p => p.PublishStartDate);
                        break;
                    case "publish_end_asc":
                        query = query.OrderBy(p => p.PublishEndDate);
                        break;
                    case "publish_end_desc":
                        query = query.OrderByDescending(p => p.PublishEndDate);
                        break;
                    case "program_start_asc":
                        query = query.OrderBy(p => p.ProgramStartDate);
                        break;
                    case "program_start_desc":
                        query = query.OrderByDescending(p => p.ProgramStartDate);
                        break;
                    case "hot": // 熱門排序
                        query = query
                            .OrderByDescending(p =>
                                p.ViewsCount * 1 +
                                p.FavoritesCount * 3 +
                                db.ProgramSubmits.Count(a => a.ProgramPlanId == p.Id) * 5
                            );
                        break;
                    default:
                        query = query.OrderByDescending(p => p.PublishStartDate);
                        break;
                }

                // 分頁
                var total = query.Count();
                var items = query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList()
                    .Select(p =>
                    {
                        // 已申請人數
                        var appliedCount = db.ProgramSubmits.Count(a => a.ProgramPlanId == p.Id);

                        // 判斷 DaysLeft 或 IsOngoing
                        int? daysLeft = null;
                        bool? isOngoing = null;

                        if (now < p.ProgramStartDate)
                        {
                            daysLeft = (p.PublishEndDate - now).Days;
                        }
                        else if (now >= p.ProgramStartDate && now <= p.ProgramEndDate)
                        {
                            isOngoing = true;
                        }
                        else
                        {
                            isOngoing = false;
                        }

                        // 三個熱門指標
                        var viewsCount = p.ViewsCount;       
                        var favoritesCount = p.FavoritesCount; 
                        var score = viewsCount * 1 + favoritesCount * 3 + appliedCount * 5;

                        return new
                        {
                            p.Id,
                            p.Name,
                            p.Intro,
                            p.Address,
                            Industry = new { p.Industry.Id, p.Industry.Title },
                            JobTitle = new { p.JobTitle.Id, p.JobTitle.Title },
                            p.PublishStartDate,
                            p.PublishEndDate,
                            p.ProgramStartDate,
                            p.ProgramEndDate,
                            Steps = p.Steps.Select(s => new
                            {
                                s.Id,
                                s.Name,
                                s.Description,
                                s.CreatedAt,
                                s.UpdatedAt
                            }),
                            AppliedCount = appliedCount,
                            DaysLeft = daysLeft,
                            IsOngoing = isOngoing,
                            ViewsCount = viewsCount,
                            FavoritesCount = favoritesCount,
                            Score = score
                        };
                    })
                    .ToList();

                string message = total == 0 ? "查無符合條件的體驗計畫" : null;

                return Ok(new
                {
                    total,
                    page,
                    limit,
                    items,
                    message
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // GET: api/PublicProgramPlans/5 取得單一體驗計畫
        [HttpGet]
        [Route("{programId:int}")]
        public IHttpActionResult GetPublicPrograms(int programId)
        {
            try
            {
                var now = DateTime.Now;

                // 取得單一計畫 (限制狀態 + 刊登有效)
                var plan = db.ProgramPlan
                    .Include(p => p.Status)
                    .FirstOrDefault(p =>
                        p.Id == programId &&
                        (p.StatusId == 2 || p.StatusId == 4 || p.StatusId == 7 || p.StatusId == 15) &&
                        p.PublishEndDate >= now
                    );

                if (plan == null)
                {
                    return Content(HttpStatusCode.NotFound, new
                    {
                        Message = "查無符合條件的體驗計畫，可能已過期、未通過審核或已拒絕"
                    });
                }

                // 取得企業名稱
                var companyName = db.Companyinfoes
                    .Where(c => c.Id == plan.CompanyId)
                    .Select(c => c.Name)
                    .FirstOrDefault();

                // 取得企業 Logo / Cover
                var companyImages = db.CompanyImages
                    .Where(ci => ci.CompanyId == plan.CompanyId)
                    .Select(ci => new
                    {
                        ci.Type,
                        ci.ImgPath
                    })
                    .ToList();
                var logo = companyImages.FirstOrDefault(i => i.Type == "logo")?.ImgPath;
                var cover = companyImages.FirstOrDefault(i => i.Type == "cover")?.ImgPath;

                // 取得已通過人數
                var appliedCount = db.ProgramSubmits
                    .Where(a => a.ProgramPlanId == plan.Id && a.StatusId == 2)
                    .Sum(a => (int?)a.ParticipantsCount) ?? 0;

                // 取得產業
                var industry = db.Industries
                    .Where(i => i.Id == plan.IndustryId)
                    .Select(i => new SimpleEntityDto
                    {
                        Id = i.Id,
                        Title = i.Title
                    })
                    .FirstOrDefault();

                // 取得職務
                var jobTitle = db.Positions
                    .Where(j => j.Id == plan.JobTitleId)
                    .Select(j => new SimpleEntityDto
                    {
                        Id = j.Id,
                        Title = j.Title
                    })
                    .FirstOrDefault();

                // 取得狀態
                var status = db.ProgramPlanStatuses
                    .Where(s => s.Id == plan.StatusId)
                    .Select(s => new SimpleEntityDto
                    {
                        Id = s.Id,
                        Title = s.Title
                    })
                    .FirstOrDefault();

                // 取得 Steps
                var steps = db.ProgramStep
                    .Where(s => s.ProgramPlanId == plan.Id)
                    .OrderBy(s => s.Id)
                    .Select(s => new ProgramStepDto
                    {
                        Name = s.Name,
                        Description = s.Description
                    })
                    .ToList();

                // 取得 Images
                var images = db.ProgramPlanImages
                    .Where(i => i.ProgramPlanId == plan.Id)
                    .OrderBy(i => i.Id)
                    .Select(i => i.ImgPath)
                    .ToList();

                // 建立 DTO
                var dto = new ProgramPlanDto
                {
                    CompanyName = companyName,
                    CompanyLogo = logo,
                    CompanyCover = cover,
                    Name = plan.Name,
                    Intro = plan.Intro,
                    Industry = industry,
                    JobTitle = jobTitle,
                    Status = status,
                    Address = plan.Address,
                    AddressMap = plan.AddressMap,
                    ContactName = plan.ContactName,
                    ContactPhone = plan.ContactPhone,
                    ContactEmail = plan.ContactEmail,
                    ProgramStartDate = plan.ProgramStartDate,
                    ProgramEndDate = plan.ProgramEndDate,
                    ProgramDurationDays = (plan.ProgramEndDate - plan.ProgramStartDate).Days,
                    AppliedCount = appliedCount,
                    Steps = steps,
                    Images = images
                };

                // 判斷 DaysLeft / IsOngoing
                if (now < plan.ProgramStartDate)
                {
                    dto.DaysLeft = Math.Max(0, (plan.ProgramStartDate - now).Days);
                    dto.IsOngoing = null;
                }
                else if (now >= plan.ProgramStartDate && now <= plan.ProgramEndDate)
                {
                    dto.IsOngoing = true;
                    dto.DaysLeft = null;
                }
                else
                {
                    dto.IsOngoing = false;
                    dto.DaysLeft = null;
                }

                // 瀏覽統計
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                var startOfDay = now.Date;

                dto.TotalViews = db.ProgramViews.Count(v => v.ProgramPlanId == plan.Id);
                dto.WeeklyViews = db.ProgramViews.Count(v => v.ProgramPlanId == plan.Id && v.ViewedAt >= startOfWeek);
                dto.DailyViews = db.ProgramViews.Count(v => v.ProgramPlanId == plan.Id && v.ViewedAt >= startOfDay);

                // ------------------------
                // 累積瀏覽紀錄 (登入使用者 + 訪客每天只算一次)
                // ------------------------
                int? userId = null;
                if (Request.Properties.ContainsKey("UserId"))
                {
                    userId = (int?)Request.Properties["UserId"];
                }

                var viewerIp = HttpContext.Current?.Request?.UserHostAddress;

                bool hasViewedToday = false;

                if (userId.HasValue)
                {
                    // 登入使用者：檢查今天是否已經看過
                    hasViewedToday = db.ProgramViews.Any(v =>
                        v.ProgramPlanId == plan.Id &&
                        v.ViewerUserId == userId.Value &&
                        v.ViewedAt >= startOfDay
                    );
                }
                else if (!string.IsNullOrEmpty(viewerIp))
                {
                    // 訪客：檢查今天是否已經用同 IP 看過
                    hasViewedToday = db.ProgramViews.Any(v =>
                        v.ProgramPlanId == plan.Id &&
                        v.ViewerUserId == null &&
                        v.ViewerIp == viewerIp &&
                        v.ViewedAt >= startOfDay
                    );
                }

                if (!hasViewedToday)
                {
                    db.ProgramViews.Add(new ProgramView
                    {
                        ProgramPlanId = plan.Id,
                        ViewedAt = now,
                        ViewerUserId = userId, // 未登入會是 null
                        ViewerIp = viewerIp
                    });
                    db.SaveChanges();
                }

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        //GET: api/v1/users/programs 申請的體驗計畫追蹤頁
        [HttpGet]
        [Route("~/api/v1/users/programs")]
        [JwtAuthFilter]
        public IHttpActionResult GetMyApplications(
            int page = 1, 
            int limit = 20, 
            string status = null, 
            string sort = "submit_desc")
        {
            if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                return Unauthorized();

            int userId = (int)userIdObj;

            int statusValue = 0;
            switch (status)
            {
                case "審核中":
                    statusValue = 1;
                    break;
                case "已通過":
                    statusValue = 2;
                    break;
                case "未通過":
                    statusValue = 3;
                    break;
                case "已取消":
                    statusValue = 4;
                    break;
                default:
                    statusValue = 0;
                    break;
            }

            // 查詢使用者的所有申請
            var query = db.ProgramSubmits
              .Include(s => s.ProgramPlan)
              .Include(s => s.ProgramPlan.Company)
              .Include(s => s.Status)
              .Where(s => s.Participant.UserId == userId);

            // 狀態篩選
            if (statusValue != 0)
                query = query.Where(s => s.StatusId == statusValue);

            // 排序
            switch (sort?.ToLower())
            {
                case "submit_asc":  // 申請日期舊到新
                    query = query.OrderBy(s => s.SubmitAt);
                    break;
                case "submit_desc": // 申請日期新到舊
                    query = query.OrderByDescending(s => s.SubmitAt);
                    break;
                case "program_start_asc":  // 體驗開始日期舊到新
                    query = query.OrderBy(s => s.ProgramPlan.ProgramStartDate);
                    break;
                case "program_start_desc":  // 體驗開始日期新到舊
                    query = query.OrderByDescending(s => s.ProgramPlan.ProgramStartDate);
                    break;
                case "program_end_asc":  // 體驗結束日期舊到新
                    query = query.OrderBy(s => s.ProgramPlan.ProgramEndDate);
                    break;
                case "program_end_desc":  // 體驗結束日期新到舊
                    query = query.OrderByDescending(s => s.ProgramPlan.ProgramEndDate);
                    break;
                case "publish_start_asc":  // 刊登開始日期舊到新
                    query = query.OrderBy(s => s.ProgramPlan.PublishStartDate);
                    break;
                case "publish_start_desc":  // 刊登開始日期新到舊
                    query = query.OrderByDescending(s => s.ProgramPlan.PublishStartDate);
                    break;
                case "publish_end_asc":  // 刊登結束日期舊到新
                    query = query.OrderBy(s => s.ProgramPlan.PublishEndDate);
                    break;
                case "publish_end_desc":  // 刊登結束日期新到舊
                    query = query.OrderByDescending(s => s.ProgramPlan.PublishEndDate);
                    break;
                default:   // 預設申請日期新到舊
                    query = query.OrderByDescending(s => s.SubmitAt);
                    break;
            }

            var total = query.Distinct().Count();

            var items = query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList()  // 先 ToList() 避免 EF 不支援 switch expression
                .Select(s =>
                {
                    string statusText;
                    switch (s.StatusId)
                    {
                        case 1:
                            statusText = "審核中";
                            break;
                        case 2:
                            statusText = "已通過";
                            break;
                        case 3:
                            statusText = "未通過";
                            break;
                        case 4:
                            statusText = "已取消";
                            break;
                        default:
                            statusText = "未知";
                            break;
                    }

                    return new
                    {
                        ApplicationId = s.Id,
                        SubmitAt = s.SubmitAt,
                        ProgramName = s.ProgramPlan.Name,
                        Address = s.ProgramPlan.Address,
                        ProgramStartDate = s.ProgramPlan.ProgramStartDate,
                        ProgramEndDate = s.ProgramPlan.ProgramEndDate,
                        MinParticipants = s.ProgramPlan.MinPeople,
                        MaxParticipants = s.ProgramPlan.MaxPeople,
                        Intro = s.ProgramPlan.Intro,
                        CompanyName = s.ProgramPlan.Company.Name,
                        Status = statusText
                    };
                })
                .ToList();

            return Ok(new
            {
                total,
                page,
                limit,
                items
            });
        }

        // GET: api/v1/users/{userId}/programs/{programId}/evaluation 取得體驗者的單一體驗評價頁資訊(填寫評價內容)
        [HttpGet]
        [Route("~/api/v1/users/{userId}/programs/{programId}/evaluation")]
        [JwtAuthFilter]
        public IHttpActionResult GetProgramEvaluation(int programId)
        {
            var plan = db.ProgramPlan
                .FirstOrDefault(p => p.Id == programId);

            if (plan == null)
                return NotFound();

            var companyName = db.Companyinfoes
                .Where(c => c.Id == plan.CompanyId)
                .Select(c => c.Name)
                .FirstOrDefault();

            var dto = new ParticipantEvaluationDto
            {
                ProgramName = plan.Name,
                ProgramStartDate = plan.ProgramStartDate,
                ProgramEndDate = plan.ProgramEndDate,
                CompanyName = companyName
            };

            return Ok(dto);
        }

        // PUT: api/PublicProgramPlans/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProgramPlan(int id, ProgramPlan programPlan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != programPlan.Id)
            {
                return BadRequest();
            }

            db.Entry(programPlan).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgramPlanExists(id))
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

        // POST: api/PublicProgramPlans  體驗者申請體驗計畫
        [HttpPost]
        [Route("{programId}/applications")]
        [JwtAuthFilter]
        [ResponseType(typeof(ProgramPlan))]
        public IHttpActionResult PostProgramPlan(int programId, [FromBody] ProgramSubmitDto dto)
        {
            try
            {
                // 1. 驗證登入
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int userId = (int)userIdObj;

                // 2. 驗證條款
                if (!dto.AgreeTerms)
                {
                    return BadRequest("請同意條款與隱私權政策");
                }

                // 3. 驗證 ProgramPlan 存在
                var programPlan = db.ProgramPlan.FirstOrDefault(p => p.Id == programId);
                if (programPlan == null)
                {
                    return NotFound();
                }

                // 4. 驗證 ParticipantInfoes
                var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                {
                    return BadRequest("找不到對應的體驗者資料，請先建立個人資料");
                }


                // 5. 防止重複申請同一個計畫
                var existingSubmit = db.ProgramSubmits
                    .FirstOrDefault(s => s.ProgramPlanId == programId && s.ParticipantId == participant.Id);
                if (existingSubmit != null)
                    return BadRequest("已申請該體驗計畫");

                // 6. 驗證 Resume
                var resumeType = dto.ResumeType?.Trim().ToLower();
                if (resumeType == "simple resume")
                {
                    var simpleResume = db.SimpleResume.FirstOrDefault(r => r.Id == dto.ResumeId && r.UserId == userId);
                    if (simpleResume == null)
                        return BadRequest("找不到該簡單履歷");
                }
                else if (resumeType == "existing resume")
                {
                    var existingResume = db.ExistingResume.FirstOrDefault(r => r.Id == dto.ResumeId && r.UserId == userId);
                    if (existingResume == null)
                        return BadRequest("找不到該上傳履歷");
                }
                else
                {
                    return BadRequest("履歷類型錯誤");
                }

                // 7. 生成申請編號 PA-2025-0818-001
                string prefix = "PA";
                string year = DateTime.Now.Year.ToString();
                string shortDate = DateTime.Now.ToString("MMdd"); // MMdd
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                int countToday = db.ProgramSubmits
                    .Count(s => s.SubmitAt >= today && s.SubmitAt < tomorrow) + 1;
                string participantSerialNumber = $"{prefix}-{year}-{shortDate}-{countToday:D3}";

                // 8. 建立 ProgramSubmit
                var submit = new ProgramSubmit
                {
                    ProgramPlanId = programId,
                    ParticipantId = participant.Id,
                    ParticipantsCount = dto.ParticipantsCount,
                    MotivationContent = dto.MotivationContent,
                    ResumeType = dto.ResumeType,
                    SubmitAt = DateTime.Now,
                    StatusId = 1, // 待審核
                    AgreeTerms = dto.AgreeTerms,
                    ParticipantSerialNum = participantSerialNumber
                };

                if (resumeType == "simple resume")
                    submit.SimpleResumeId = dto.ResumeId;
                else if (resumeType == "existing resume")
                    submit.ExistingResumeId = dto.ResumeId;

                db.ProgramSubmits.Add(submit);
                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    application_number = submit.ParticipantSerialNum,
                    message = "申請已送出，等待企業審核"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        
        // POST: api/v1/users/{userId}/programs/{programId}/evaluation    體驗者提交評價
        [HttpPost]
        [Route("~/api/v1/users/{userId}/programs/{programId}/evaluations")]
        [JwtAuthFilter]
        public IHttpActionResult SubmitReview(int userId, int programId, [FromBody] ParticipantEvaluationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 先找到對應的 ParticipantInfo
            var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                return BadRequest("找不到對應的體驗者");

            // 找到對應的申請紀錄
            var application = db.ProgramSubmits
                .FirstOrDefault(s => s.ParticipantId == participant.Id && s.ProgramPlanId == programId);

            if (application == null)
                return BadRequest("找不到對應的體驗申請");

            // 檢查申請是否已通過
            if (application.StatusId != 2) // 2 = 已通過
                return BadRequest("該體驗者尚未通過企業審核，無法提交評價");

            // 取得體驗計畫
            var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId);
            if (program == null)
                return BadRequest("找不到該體驗計畫");

            // 檢查是否超過體驗結束日期
            if (DateTime.Now <= program.ProgramEndDate)
                return BadRequest("體驗尚未結束，無法提交評價");

            // 檢查是否已有評價
            var existingReview = db.ParticipantEvaluations
                .FirstOrDefault(r => r.ProgramPlanId == programId && r.ParticipantId == participant.Id);

            if (existingReview != null)
                return BadRequest("該體驗已提交評價");

            // 建立資料庫實體
            var review = new ParticipantEvaluation
            {
                ParticipantId = participant.Id,
                ProgramPlanId = programId,
                Score = dto.Score,
                Comment = dto.Comment
            };

            db.ParticipantEvaluations.Add(review);
            db.SaveChanges();

            // 回傳結果
            var response = db.ParticipantEvaluations
                .Where(r => r.Id == review.Id)
                .Select(r => new ParticipantEvaluationDto
                {
                    ProgramName = r.Program.Name,
                    ProgramStartDate = r.Program.ProgramStartDate,
                    ProgramEndDate = r.Program.ProgramEndDate,
                    CompanyName = r.Program.Company.Name,
                    Score = r.Score,
                    Comment = r.Comment
                })
                .FirstOrDefault();

            return Ok(response);
        }

        // DELETE: api/PublicProgramPlans/5
        [ResponseType(typeof(ProgramPlan))]
        public IHttpActionResult DeleteProgramPlan(int id)
        {
            ProgramPlan programPlan = db.ProgramPlan.Find(id);
            if (programPlan == null)
            {
                return NotFound();
            }

            db.ProgramPlan.Remove(programPlan);
            db.SaveChanges();

            return Ok(programPlan);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProgramPlanExists(int id)
        {
            return db.ProgramPlan.Count(e => e.Id == id) > 0;
        }
    }
}