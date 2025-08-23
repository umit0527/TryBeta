using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.UI;
using TryBeta.Models;
using static TryBeta.Models.ParticipantDetailDto;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/company/{companyid:int}")]
    public class ProgramPlansController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/ProgramPlans
        //public IQueryable<ProgramPlans> GetProgramPlans()
        //{
        //    return db.ProgramPlans;
        //}

        // GET: api/ProgramPlans/5 取得所有體驗計畫(未審核即通過)
        //[HttpGet]
        //[Route("programs")]
        //[JwtAuthFilter] // 必須登入
        //public IHttpActionResult GetProgramPlans(int page = 1, int pageSize = 21)
        //{
        //    try
        //    {
        //        // 1. 取得登入企業 ID
        //        if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
        //        {
        //            return Unauthorized();
        //        }
        //        int companyId = (int)userIdObj;


        //        // 2.保護 page/pageSize
        //        if (page <= 0) page = 1;
        //        if (pageSize <= 0 || pageSize > 100) pageSize = 21;

        //        // 3. 計算總筆數
        //        var totalCount = db.ProgramPlan.Count(p => p.CompanyId == companyId);

        //        // 4.分頁查詢
        //        var programs = db.ProgramPlan
        //            .Where(p => p.CompanyId == companyId)
        //            .OrderByDescending(p => p.CreatedAt)
        //            .Skip((page - 1) * pageSize)   // 跳過前面頁數的資料
        //            .Take(pageSize)                // 只取一頁大小
        //            .Select(p => new
        //            {
        //                p.Id,
        //                p.StatusId,
        //                p.Name,
        //                p.Intro,


        //                p.PublishStartDate,
        //                p.PublishDurationDays,
        //                p.ProgramStartDate,
        //                p.ProgramEndDate,
        //                p.CreatedAt,
        //                p.UpdatedAt,
        //            // 成團判斷，假設你有一個方法計算當前報名人數
        //        IsConfirmed = db.ProgramRegistrations
        //                        .Count(r => r.ProgramId == p.Id) >= p.MinPeople
        //            })
        //    .ToList();

        //        // 回傳分頁資訊
        //        return Ok(new
        //        {
        //            total = totalCount,     // 總筆數
        //            page,
        //            pageSize,
        //            data = programs
        //        });               
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

        // GET: api/v1/company/{companyid}/programs/{programId} 取得單一體驗計畫詳情
        [HttpGet]
        [Route("programs/{programplanId:int}")]
        [JwtAuthFilter]
        public IHttpActionResult GetProgramPlan(int companyid, int programId)
        {
            try
            {
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int companyId = (int)userIdObj;

                // 確認登入的企業ID與route companyid一致，可做額外保護
                if (companyId != companyid)
                {
                    return Unauthorized();
                }

                var programPlan = db.ProgramPlan
                    .Where(p => p.Id == programId && p.CompanyId == companyId)
                    .FirstOrDefault();

                if (programPlan == null)
                {
                    return NotFound();
                }

                // 取得階段資料
                var steps = db.ProgramStep
                    .Where(s => s.ProgramPlanId == programPlan.Id)
                    .OrderBy(s => s.Id)
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Description
                    })
                    .ToList();

                // 取得產業名稱與職務名稱
                var industry = db.Industries
                    .Where(i => i.Id == programPlan.IndustryId)
                    .Select(i => new { i.Id, i.Title })
                    .FirstOrDefault();
                var jobTitle = db.Positions
                    .Where(j => j.Id == programPlan.JobTitleId)
                    .Select(j => new { j.Id, j.Title })
                    .FirstOrDefault();

                // 取得狀態名稱
                var status = db.ProgramPlanStatuses
                    .Where(s => s.Id == programPlan.StatusId)
                    .Select(s => new { s.Id, s.Title })
                    .FirstOrDefault();

                // 取得圖片資料
                var images = db.ProgramPlanImages
                    .Where(img => img.ProgramPlanId == programPlan.Id)
                    .Select(img => new { img.Id, img.ImgPath })
                    .ToList();

                //  取得申請統計資訊 
                var totalApplicants = db.ProgramSubmits.Count(s => s.ProgramPlanId == programPlan.Id);
                var reviewedCount = db.ProgramSubmits
                    .Count(s => s.ProgramPlanId == programPlan.Id && db.ProgramSubmitReviews.Any(r => r.ProgramSubmitId == s.Id));
                var pendingCount = totalApplicants - reviewedCount;


                // 瀏覽統計
                var now = DateTime.Now;
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek); // 星期日為一週起點
                var startOfDay = now.Date;

                var totalViews = db.ProgramViews.Count(v => v.ProgramPlanId == programPlan.Id);
                var weeklyViews = db.ProgramViews.Count(v => v.ProgramPlanId == programPlan.Id && v.ViewedAt >= startOfWeek);
                var dailyViews = db.ProgramViews.Count(v => v.ProgramPlanId == programPlan.Id && v.ViewedAt >= startOfDay);

                var response = new
                {
                    //統計區塊
                    Statistics = new
                    {
                        TotalApplicants = totalApplicants,
                        ReviewedCount = reviewedCount,
                        PendingCount = pendingCount
                    },
                    programPlan.Id,
                    programPlan.Name,
                    programPlan.Intro,
                    Industry = industry,
                    JobTitle = jobTitle,
                    Status = status,
                    programPlan.Address,
                    programPlan.ContactName,
                    programPlan.ContactPhone,
                    programPlan.MinPeople,
                    programPlan.MaxPeople,
                    programPlan.PublishStartDate,
                    programPlan.PublishDurationDays,
                    programPlan.PublishEndDate,
                    programPlan.ProgramStartDate,
                    programPlan.ProgramEndDate,
                    programPlan.ProgramDurationDays,
                    Steps = steps,
                    Images = images,
                    //瀏覽數據
                    Views = new
                    {
                        TotalViews = totalViews,
                        WeeklyViews = weeklyViews,
                        DailyViews = dailyViews
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 企業的體驗計畫清單 (支援搜尋、篩選、排序、分頁)
        /// </summary>
        // GET: api/v1/company/{companyid}/programs 企業的體驗計畫篩選器，包含全部體驗計畫
        [HttpGet]
        [Route("programs")]
        [JwtAuthFilter]
        public IHttpActionResult GetCompanyPrograms(
        string search = null,
        int? industry_id = null,
        int? job_title_id = null,
        int? status_id = null,
        string sort = "publish_start_desc",
        int page = 1,
        int limit = 21)
        {
            try
            {
                // 驗證登入企業ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int companyId = (int)userIdObj;

                // 基本查詢
                var query = db.ProgramPlan
                    .Include(p => p.Industry)
                    .Include(p => p.JobTitle)
                    .Include(p => p.Status)
                    .Include(p => p.Steps)
                    .Include(p => p.ProgramPlanImages)
                    .Where(p => p.CompanyId == companyId)
                    .AsQueryable();

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) ||
                                        p.Intro.Contains(search) ||
                                        p.Steps.Any(s => s.Name.Contains(search) ||
                                        s.Description.Contains(search)));
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

                // 狀態篩選
                if (status_id.HasValue)
                {
                    switch (status_id.Value)
                    {
                        case 1: // 審核中
                            query = query.Where(p => p.StatusId == 1);
                            break;

                        case 2: // 系統通過
                            query = query.Where(p => p.StatusId == 2);
                            break;

                        case 3: // 系統拒絕
                            query = query.Where(p => p.StatusId == 3);
                            break;

                        case 4: // 人工通過
                            query = query.Where(p => p.StatusId == 4);
                            break;

                        case 5: // 人工拒絕
                            query = query.Where(p => p.StatusId == 5);
                            break;

                        case 6: // 待發布 (狀態=已發布，但開始時間還沒到)
                            var now = DateTime.Now;
                            query = query.Where(p =>
                                p.PublishStartDate > now
                            );
                            break;

                        case 7: // 已發布 (狀態=已發布，在時間區間內，並且是系統/人工通過) (依刊登時間判斷)
                            now = DateTime.Now;
                            query = query.Where(p =>
                                (p.StatusId == 2 || p.StatusId == 4) &&   // 系統或人工通過
                                p.PublishStartDate <= now &&
                                (p.PublishEndDate == null || p.PublishEndDate >= now)
                            );
                            break;

                        case 15: // 已通過 (包含系統通過+人工通過)
                            query = query.Where(p =>
                                p.StatusId == 2 || p.StatusId == 4
                            );
                            break;

                        case 16: // 未通過 (包含系統拒絕+人工拒絕)
                            query = query.Where(p =>
                                p.StatusId == 3 || p.StatusId == 5
                            );
                            break;

                        default:
                            // 不做篩選 (全部)
                            break;
                    }
                }

                // 排序
                switch (sort)
                {
                    case "publish_start_asc":        // 刊登開始日期舊到新
                        query = query.OrderBy(p => p.PublishStartDate);
                        break;
                    case "publish_end_asc":          // 刊登結束日期舊到新
                        query = query.OrderBy(p => p.PublishEndDate);
                        break;
                    case "publish_end_desc":         // 刊登結束日期新到舊
                        query = query.OrderByDescending(p => p.PublishEndDate);
                        break;
                    case "program_start_asc":        // 體驗開始日期舊到新
                        query = query.OrderBy(p => p.ProgramStartDate);
                        break;
                    case "program_start_desc":       // 體驗開始日期新到舊
                        query = query.OrderByDescending(p => p.ProgramStartDate);
                        break;
                    case "program_end_asc":       // 體驗結束日期舊到新
                        query = query.OrderByDescending(p => p.ProgramStartDate);
                        break;
                    case "program_end_desc":       // 體驗結束日期新到舊
                        query = query.OrderByDescending(p => p.ProgramStartDate);
                        break;
                    default:                         // 預設刊登開始日期新到舊
                        query = query.OrderByDescending(p => p.PublishStartDate);
                        break;
                }

                // 分頁
                var total = query.Count();
                var items = query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Intro,
                        Industry = new { p.Industry.Id, p.Industry.Title },
                        JobTitle = new { p.JobTitle.Id, p.JobTitle.Title },
                        p.PublishStartDate,
                        p.PublishEndDate,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        CoverImage = p.ProgramPlanImages
                        .OrderBy(img => img.Id)
                        .Select(img => img.ImgPath)
                        .FirstOrDefault(),
                        Steps = p.Steps.Select(s => new
                        {
                            s.Id,
                            s.Name,
                            s.Description,
                            s.CreatedAt,
                            s.UpdatedAt
                        })
                    })
                    .ToList();

                // 回傳訊息
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

        // GET: api/v1/company/{companyid}/programs/{programId}/applications  查看單一體驗的申請者列表
        [HttpGet]
        [Route("~/api/v1/company/programs/{programId:int}/applications")]
        [JwtAuthFilter]
        public IHttpActionResult GetProgramApplications(
            string pending_sort = "submit_desc",
            string reviewed_sort = "submit_desc",
            int? reviewed_filter = null)
        {
            try
            {
                // 從 JWT 取得 companyId
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int companyId = (int)userIdObj;

                // 從 query string 或 route 取得 programId
                var routeData = this.Request.GetRouteData();
                int programId = int.Parse(routeData.Values["programId"].ToString());

                // 取得 program，確認公司
                var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId && p.CompanyId == companyId);
                if (program == null) return NotFound();

                // 取得所有申請，包含 Participant 與 Status
                var applications = db.ProgramSubmits
                    .Include(a => a.Participant)
                    .Include(a => a.Status)
                    .Where(a => a.ProgramPlanId == programId)
                    .OrderByDescending(a => a.SubmitAt)
                    .ToList();

                // Pending
                var pendingQuery = applications.Where(a => a.StatusId == 1);
                pendingQuery = pending_sort == "submit_asc" ? pendingQuery.OrderBy(a => a.SubmitAt) : pendingQuery.OrderByDescending(a => a.SubmitAt);
                var pending = pendingQuery.Select(a => new
                {
                    applicant_name = a.Participant.Name,
                    identity = a.Participant.IdentityId,
                    program_name = program.Name,
                    submit_date = a.SubmitAt,
                    review_status = a.Status.Title
                }).ToList();

                // Reviewed
                var reviewedQuery = applications.Where(a => a.StatusId != 1);

                // 篩選已通過 / 已拒絕 / 全部
                if (reviewed_filter.HasValue)
                {
                    switch (reviewed_filter.Value)
                    {
                        case 1: // 已通過
                            reviewedQuery = reviewedQuery.Where(a => a.StatusId == 1);
                            break;

                        case 2: // 已拒絕
                            reviewedQuery = reviewedQuery.Where(a => a.StatusId == 2);
                            break;

                        default: // 其他情況 (例如亂丟數字)
                            reviewedQuery = reviewedQuery.Where(a => a.StatusId == reviewed_filter.Value);
                            break;
                    }
                }

                reviewedQuery = reviewed_sort == "submit_asc" ? reviewedQuery.OrderBy(a => a.SubmitAt) : reviewedQuery.OrderByDescending(a => a.SubmitAt);
                var reviewed = reviewedQuery.Select(a => new
                {
                    applicant_name = a.Participant.Name,
                    identity = a.Participant.IdentityId,
                    program_name = program.Name,
                    submit_date = a.SubmitAt,
                    review_status = a.Status.Title,
                    review_date = a.SubmitAt
                }).ToList();

                var response = new
                {
                    total_applicants = applications.Count,
                    reviewed_count = reviewed.Count,
                    pending_count = pending.Count,
                    pending_applications = pending,
                    reviewed_applications = reviewed
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/v1/company/{companyid}/programs/{programId}/applications  查看單一體驗的單一申請者詳情
        [HttpGet]
        [Route("~/api/v1/programs/{programId:int}/applications/{participantId:int}")]
        [JwtAuthFilter]
        public IHttpActionResult GetApplicantDetail(int programId, int participantId)
        {
            try
            {
                // 從 JWT 取得 companyId
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int companyId = (int)userIdObj;

                // 取得 program，確認體驗計畫與公司
                var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId && p.CompanyId == companyId);
                if (program == null) return NotFound();

                // 取得申請 + 參加者 + 教育資訊 + City / District
                var application = db.ProgramSubmits
                    .Include(a => a.Participant)
                    .Include(a => a.Participant.Education)
                    .Include(a => a.Participant.City)
                    .Include(a => a.Participant.District)
                    .Include(a => a.Participant.Identity)
                    .Include(a => a.Participant.User)
                    .Include(a => a.ProgramPlan)
                    .FirstOrDefault(a => a.ProgramPlanId == programId && a.ParticipantId == participantId);

                if (application == null)
                    return NotFound();

                var participant = application.Participant;

                // 取得該體驗者的所有評價
                var reviews = db.ParticipantEvaluations
                    .Where(r => r.ParticipantId == participantId)
                    .ToList();

                // 取得使用者啟用的簡單履歷
                var simpleResume = db.SimpleResume
                    .Include(r => r.Skills)
                    .Include(r => r.PortfolioFiles)
                    .FirstOrDefault(r => r.UserId == participant.UserId && r.IsActive);

                // 組成 DTO
                var dto = new ParticipantDetailDto
                {
                    ReviewStatusId = application.StatusId,
                    ReviewStatusName = application.Status?.Title ?? "",

                    // 第一個區塊
                    ParticipantSerialNum = application.ParticipantSerialNum ?? application.Id.ToString(),
                    Name = participant.Name,
                    Phone = participant.Phone,
                    Age = DateTime.Now.Year - participant.Birthday.Year -
                          (DateTime.Now.DayOfYear < participant.Birthday.DayOfYear ? 1 : 0),
                    Gender = participant.Gender,
                    IdentityId = participant.IdentityId,
                    IdentityName = participant.Identity?.Title ?? "",
                    Address = (participant.City?.Name ?? "") + (participant.District?.Name ?? "") + participant.Street,
                    Email = participant.User?.Email ?? "",
                    Headshot = participant.Headshot ?? "",
                    SchoolName = participant.Education?.SchoolName ?? "",
                    Major = participant.Education?.Major ?? "",
                    StatusId = participant.Education?.StatusId ?? 0,
                    ReviewCount = reviews.Count,
                    AverageScore = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Score), 2) : 0,

                    // 第二個區塊
                    ProgramPlan = new ParticipantDetailDto.ProgramInfoDto
                    {
                        Name = program.Name,
                        SerialNum = program.SerialNum,
                        ProgramStartDate = program.ProgramStartDate,
                        ProgramEndDate = program.ProgramEndDate,
                        DurationDays = program.ProgramDurationDays,
                        Address = program.Address
                    },
                    MotivationContent = application.MotivationContent,

                    // 第三個區塊：技能
                    Skills = simpleResume?.Skills.Select(s => s.SkillName).ToList() ?? new List<string>(),

                    // 第四區塊：附件(上傳)
                    PortfolioFiles = simpleResume?.PortfolioFiles
                                    .Select(f => new ParticipantDetailDto.PortfolioFileDto
                                    {
                                        Id = f.Id,
                                        Title = f.Title,
                                        PortfolioPath = f.PortfolioPath,
                                        FileSize = f.FileSize
                                    })
                                    .ToList() ?? new List<ParticipantDetailDto.PortfolioFileDto>(),
                };

                // 第五個區塊：過去參加的體驗計畫
                var pastPrograms = db.ProgramSubmits
                    .Include(s => s.ProgramPlan)
                    .Include(s => s.Status)
                    .Where(s => s.ParticipantId == participantId)
                    .Where(s => s.StatusId == 2 || s.StatusId == 4) // 已參加或自行取消
                    .Where(s => s.StatusId == 2 ? s.ProgramPlan.ProgramEndDate < DateTime.Now : true)
                    .Select(s => new ParticipantDetailDto.PastProgramDto
                    {
                        ProgramName = s.ProgramPlan.Name,
                        ProgramStartDate = s.ProgramPlan.ProgramStartDate,
                        ProgramEndDate = s.ProgramPlan.ProgramEndDate,
                        ParticipationStatus = s.StatusId == 2 ? "Attended" : "Cancelled",
                        CancelReason = s.StatusId == 4 ? s.CancelReason : null,
                        ReviewScore = s.StatusId == 2 ? db.ParticipantEvaluations
                                                         .Where(r => r.ParticipantId == participantId && r.ProgramPlanId == s.ProgramPlanId)
                                                         .Select(r => (double?)r.Score)
                                                         .FirstOrDefault()
                                                     : null
                    })
                    .ToList();

                dto.PastPrograms = pastPrograms;

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/v1/company/{companyId}/evaluations  企業取得評價列表資訊
        [HttpGet]
        [Route("~/api/v1/company/{companyId}/evaluations")]
        public IHttpActionResult GetCompanyEvaluations(
            int companyId,
            string search = null,
            int? score = null,                // 分數 1-5
            DateTime? start_date = null,       // 評價起始日
            DateTime? end_date = null,         // 評價結束日
            string sort = "date_desc",        // 排序方式
            int page = 1,
            int limit = 20)
        {
            var query = db.ParticipantEvaluations
                .Where(e => e.Program.CompanyId == companyId)  //該企業的體驗評價
                .Where(e => e.StatusId == 2 || e.StatusId == 4 || e.StatusId == 15);  //評價是審核通過的

            // 搜尋：體驗者名字 / 評價內容 / 計畫名稱
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    e.Participant.Name.Contains(search) ||
                    e.Comment.Contains(search) ||
                    e.Program.Name.Contains(search));
            }

            // 篩選分數
            if (score.HasValue)
            {
                query = query.Where(e => e.Score == score.Value);
            }

            // 篩選日期區間
            if (start_date.HasValue)
                query = query.Where(e => e.CreatedAt >= start_date.Value);
            if (end_date.HasValue)
            {
                var endDateInclusive = end_date.Value.Date.AddDays(1); // 把時間拉到隔天 0:00
                query = query.Where(e => e.CreatedAt < endDateInclusive); // 小於隔天 0:00 => 包含整天
            }

                // 排序
                switch (sort.ToLower())
            {
                case "date_asc":
                    query = query.OrderBy(e => e.CreatedAt);
                    break;
                case "date_desc":
                default:
                    query = query.OrderByDescending(e => e.CreatedAt);
                    break;
            }

            // 總筆數
            var totalCount = query.Count();

            // 先投影需要的欄位 + ToList() 關閉 DataReader
            var tempList = query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(e => new
                {
                    Id = e.Id,
                    ParticipantName = e.Participant.Name,
                    ParticipantIdentity = e.Participant.Identity,
                    Birthday = e.Participant.Birthday,
                    ProgramName = e.Program.Name,
                    ProgramPlanId=e.Program.Id,
                    Score = e.Score,
                    Comment = e.Comment,
                    EvaluationDate = e.CreatedAt
                })
                .ToList();

            // 在記憶體中處理計算年齡
            
            var evaluations = tempList.Select(e => new
            {
                e.Id,
                e.ParticipantName,
                e.ParticipantIdentity,
                ParticipantAge = DateTime.Today.Year - e.Birthday.Year -
                                 (DateTime.Today.DayOfYear < e.Birthday.DayOfYear ? 1 : 0),
                e.ProgramName,
                e.ProgramPlanId,
                e.Score,
                e.Comment,
                e.EvaluationDate
            }).ToList();

            var result = new
            {
                TotalCount = totalCount,
                Page = page,
                Limit = limit,
                Data = evaluations
            };

            return Ok(result);
        }

        // POST: api/ProgramPlans
        //public IHttpActionResult PostProgramPlans(ProgramPlan programPlans)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.ProgramPlan.Add(programPlans);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = programPlans.Id }, programPlans);
        //}

        // POST: api/v1/program 新增體驗計畫
        [HttpPost]
        [Route("programs")]
        [JwtAuthFilter]
        public IHttpActionResult CreateProgramPlan([FromBody] ProgramPlanDto dto)
        {
            try
            {
                // 1. 驗證最大.最少人數
                if (dto.MaxPeople < dto.MinPeople)
                    return BadRequest("最大人數不得小於最少人數");

                // 2. 取得登入企業 ID
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                    return Unauthorized();
                int companyId = (int)userIdObj;

                // 3. 檢查方案是否過期並更新狀態
                var planUsage = db.PlanUsage
                    .Include("Plan")
                    .Include("PlanUsageStatus")
                    .Where(p => p.CompanyId == companyId)
                    .OrderByDescending(p => p.StartDate)
                    .FirstOrDefault();

                if (planUsage == null)
                    return BadRequest("尚未購買方案或方案已過期");

                bool changed = false;
                if (planUsage.EndDate.HasValue && planUsage.EndDate.Value.Date < DateTime.Now.Date)
                {
                    planUsage.StatusId = 2; // expired
                    changed = true;
                }
                else if (planUsage.RemainingPeople <= 0)
                {
                    planUsage.StatusId = 4; // full
                    changed = true;
                }

                if (changed) db.SaveChanges();
                if (planUsage.StatusId != 1) return BadRequest("方案不可用（已過期或已額滿）");
                if (planUsage.RemainingPeople < dto.MaxPeople) return BadRequest("體驗剩餘人數不足");

                // 4. 生成序號 PRJ-yyyyMMdd-序號
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                int todayCount = db.ProgramPlan.Count(p => p.CreatedAt >= today && p.CreatedAt < tomorrow) + 1;
                string serialNumber = $"PRJ-{DateTime.Now:yyyyMMdd}-{todayCount:D3}";

                // 5. 建立 ProgramPlan
                var programPlan = new ProgramPlan
                {
                    CompanyId = companyId,
                    SerialNum = serialNumber,
                    Name = dto.Name,
                    Intro = dto.Intro,
                    IndustryId = dto.IndustryId,
                    JobTitleId = dto.JobTitleId,
                    Address = dto.Address,
                    AddressMap = dto.AddressMap,
                    ContactName = dto.ContactName,
                    ContactPhone = dto.ContactPhone,
                    ContactEmail = dto.ContactEmail,
                    MinPeople = dto.MinPeople,
                    MaxPeople = dto.MaxPeople,
                    PublishStartDate = dto.PublishStartDate,
                    PublishEndDate = dto.PublishStartDate.AddDays(dto.PublishDurationDays - 1),
                    PublishDurationDays = dto.PublishDurationDays,
                    ProgramStartDate = dto.ProgramStartDate,
                    ProgramEndDate = dto.ProgramEndDate,
                    ProgramDurationDays = (dto.ProgramEndDate - dto.ProgramStartDate).Days + 1,
                    StatusId = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.ProgramPlan.Add(programPlan);
                db.SaveChanges();

                // 6. 建立階段
                foreach (var stepDto in dto.Steps)
                {
                    var step = new ProgramStep
                    {
                        Name = stepDto.Name,
                        Description = stepDto.Description,
                        ProgramPlanId = programPlan.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.ProgramStep.Add(step);
                }

                // 7. 建立圖片
                if (dto.Images != null && dto.Images.Any())
                {
                    if (dto.Images.Count > 4) return BadRequest("最多只能上傳 4 張圖片");

                    foreach (var imgUrl in dto.Images)
                    {
                        var image = new ProgramPlanImage
                        {
                            ProgramPlanId = programPlan.Id,
                            ImgPath = imgUrl,
                            CreatedAt = DateTime.Now
                        };
                        db.ProgramPlanImages.Add(image);
                    }
                }

                // 8. 扣掉剩餘人數
                planUsage.RemainingPeople -= dto.MaxPeople;
                if (planUsage.RemainingPeople <= 0) planUsage.StatusId = 4; // full
                db.SaveChanges();

                // 9. 取得公司資訊
                var company = db.Companyinfoes
                    .Include("CompanyImages")
                    .FirstOrDefault(c => c.Id == companyId);

                var companyName = company?.Name;
                var companyLogo = company?.CompanyImages.FirstOrDefault(img => img.Type == "logo")?.ImgPath;
                var companyCover = company?.CompanyImages.FirstOrDefault(img => img.Type == "cover")?.ImgPath;

                // 10. 取得產業與職務名稱
                var industry = db.Industries.FirstOrDefault(i => i.Id == programPlan.IndustryId)?.Title;
                var jobTitle = db.Positions.FirstOrDefault(p => p.Id == programPlan.JobTitleId)?.Title;

                // 11. 取得狀態名稱
                var statusTitle = db.ProgramPlanStatuses.FirstOrDefault(s => s.Id == programPlan.StatusId)?.Title;

                // 12. 計算剩餘天數與是否進行中
                var daysLeft = (programPlan.ProgramEndDate - DateTime.Today).Days;
                var isOngoing = DateTime.Today >= programPlan.ProgramStartDate
                                && DateTime.Today <= programPlan.ProgramEndDate;

                // 13. 回傳 DTO
                var responseDto = new ProgramPlanDto
                {
                    SerialNum = programPlan.SerialNum,
                    CompanyName = companyName,
                    CompanyLogo = companyLogo,
                    CompanyCover = companyCover,
                    Name = programPlan.Name,
                    Intro = programPlan.Intro,
                    IndustryId = programPlan.IndustryId,
                    JobTitleId = programPlan.JobTitleId,
                    Address = programPlan.Address,
                    AddressMap = programPlan.AddressMap,
                    ContactName = programPlan.ContactName,
                    ContactPhone = programPlan.ContactPhone,
                    ContactEmail = programPlan.ContactEmail,
                    MinPeople = programPlan.MinPeople,
                    MaxPeople = programPlan.MaxPeople,
                    PublishStartDate = programPlan.PublishStartDate,
                    PublishEndDate = programPlan.PublishEndDate,
                    PublishDurationDays = programPlan.PublishDurationDays,
                    ProgramStartDate = programPlan.ProgramStartDate,
                    ProgramEndDate = programPlan.ProgramEndDate,
                    ProgramDurationDays = programPlan.ProgramDurationDays,
                    StatusId = programPlan.StatusId,
                    StatusTitle = statusTitle,
                    Industry = new ProgramPlanDto.SimpleEntityDto { Id = programPlan.IndustryId, Title = industry },
                    JobTitle = new ProgramPlanDto.SimpleEntityDto { Id = programPlan.JobTitleId, Title = jobTitle },
                    Steps = dto.Steps,
                    Images = dto.Images,
                    DaysLeft = daysLeft,
                    //IsOngoing = isOngoing
                };

                return Ok(responseDto);
            }
            catch (DbEntityValidationException ex)
            {
                var allErrors = ex.EntityValidationErrors
                    .SelectMany(eve => eve.ValidationErrors)
                    .Select(ve => new { Property = ve.PropertyName, Error = ve.ErrorMessage })
                    .ToList();

                return Content(HttpStatusCode.BadRequest, new { Message = "欄位驗證失敗", Errors = allErrors });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/ProgramPlans/5 審核體驗者通過或拒絕
        [HttpPut]
        [Route("~/api/v1/programs/{programId:int}/applications/{participantId:int}/review")]
        [JwtAuthFilter]
        public IHttpActionResult ReviewParticipant(int programId, int participantId, [FromBody] ProgramSubmitReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.StatusId != (int)ReviewStatus.Approved && dto.StatusId != (int)ReviewStatus.Rejected)
                return BadRequest("只能設定為核准申請或婉拒申請");

            if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                return Unauthorized();

            int companyId = (int)userIdObj;

            var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId && p.CompanyId == companyId);
            if (program == null)
            {
                return Content(System.Net.HttpStatusCode.Forbidden, new
                {
                    message = "非本公司不得審核該體驗計畫"
                });
            }

            var application = db.ProgramSubmits.FirstOrDefault(a => a.ProgramPlanId == programId && a.ParticipantId == participantId);
            if (application == null) return NotFound();

            // 更新申請狀態與審核時間
            application.StatusId = (int)dto.StatusId;
            application.ReviewedAt = DateTime.Now;

            // 新增審核紀錄到 ProgramSubmitReviews
            var review = new ProgramSubmitReview
            {
                ProgramSubmitId = application.Id,
                StatusId = dto.StatusId,
                Comment = dto.Comment,   // 統一存通過訊息或拒絕理由
                ReviewedAt = DateTime.Now,
                ReviewerId = companyId
            };
            db.ProgramSubmitReviews.Add(review);

            //當審核狀態為「通過」時，將 ProgramPlan 的 AppliedCount 欄位加一
            if (dto.StatusId == (int)ReviewStatus.Approved)
            {
               program.AppliedCount++;
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errors = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                return BadRequest("資料驗證失敗: " + string.Join("; ", errors));
            }

            return Ok(new
            {
                message = "審核完成",
                status = application.StatusId,
                status_title = ((ReviewStatus)application.StatusId).ToString(),
                comment = review.Comment
            });
        }

        // DELETE: api/ProgramPlans/5
        [ResponseType(typeof(ProgramPlan))]
        public IHttpActionResult DeleteProgramPlans(int id)
        {
            ProgramPlan programPlans = db.ProgramPlan.Find(id);
            if (programPlans == null)
            {
                return NotFound();
            }

            db.ProgramPlan.Remove(programPlans);
            db.SaveChanges();

            return Ok(programPlans);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProgramPlansExists(int id)
        {
            return db.ProgramPlan.Count(e => e.Id == id) > 0;
        }
    }
}