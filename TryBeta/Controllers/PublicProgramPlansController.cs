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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Configuration;
using System.Web.Http.Results;
using Jose;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/programs")]
    public class PublicProgramPlansController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/PublicProgramPlans
        //public IQueryable<ProgramPlan> GetProgramPlan()
        //{
        //    return db.ProgramPlan;
        //}

        // GET: api/v1/programs 體驗計畫篩選器，包含全部體驗計畫 + 首頁Navbar搜尋欄
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetPublicProgramsFilter(
            string search = null,
            int? industry_id = null,
            int? job_title_id = null,
            string city_id = null,
            string district_id = null,
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
                    //.Include(p => p.Steps)
                    .Include(p => p.ProgramPlanImages)
                    .Include(p => p.Company)
                    .AsQueryable();

                // 過濾掉不符狀態或刊登過期的體驗
                query = query.Where(p =>
                    (p.StatusId == 2 || p.StatusId == 4 || p.StatusId == 7 || p.StatusId == 15) &&
                    p.PublishEndDate >= now
                );

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                     p.Name.Contains(search) ||
                     p.Industry.Title.Contains(search) ||
                     p.JobTitle.Title.Contains(search) ||
                     p.Address.Contains(search) ||
                     p.Company.Name.Contains(search)
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

                        // 如果同時選擇鄉鎮
                        if (!string.IsNullOrEmpty(district_id))
                        {
                            var districtName = db.Districts
                                                 .Where(d => d.Id.ToString() == district_id)
                                                 .Select(d => d.Name)
                                                 .FirstOrDefault();
                            if (!string.IsNullOrEmpty(districtName))
                            {
                                query = query.Where(p => p.Address.Contains(districtName));
                            }
                        }
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
                            CoverImage = p.ProgramPlanImages
                            .OrderBy(img => img.Id)
                            .Select(img => img.ImgPath)
                            .FirstOrDefault(),
                            //Steps = p.Steps.Select(s => new
                            //{
                            //    s.Name,
                            //    s.Description,
                            //    s.CreatedAt,
                            //    s.UpdatedAt
                            //}),
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

        // GET: api/Programs/5 取得單一體驗計畫詳情
        [HttpGet]
        [Route("{programId:int}")]
        public IHttpActionResult GetPublicPrograms(int programId)
        {
            try
            {
                var now = DateTime.Now;
                var startOfDay = now.Date;
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);

                // 取得單一計畫 (限制狀態 + 刊登有效)
                var plan = db.ProgramPlan
                    .Include(p => p.Status)
                    .Include(p => p.Industry)
                    .Include(p => p.JobTitle)
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
                    .Select(ci => new { ci.Type, ci.ImgPath })
                    .ToList();
                var logo = companyImages.FirstOrDefault(i => i.Type == "logo")?.ImgPath;
                var cover = companyImages.FirstOrDefault(i => i.Type == "cover")?.ImgPath;

                // 取得已通過人數
                var appliedCount = db.ProgramSubmits
                    .Where(a => a.ProgramPlanId == plan.Id && a.StatusId == 2)
                    .Sum(a => (int?)a.ParticipantsCount) ?? 0;

                // 取得 Steps
                var steps = db.ProgramStep
                    .Where(s => s.ProgramPlanId == plan.Id)
                    .OrderBy(s => s.Id)
                    .Select(s => new ProgramPlanDto.ProgramStepDto { Name = s.Name, Description = s.Description })
                    .ToList();

                // 取得 Images
                var images = db.ProgramPlanImages
                    .Where(i => i.ProgramPlanId == plan.Id)
                    .OrderBy(i => i.Id)
                    .Select(i => i.ImgPath)
                    .ToList();

                // 一次撈出 Views
                var views = db.ProgramViews
                    .Where(v => v.ProgramPlanId == plan.Id)
                    .ToList();
                var totalViews = views.Count;
                var weeklyViews = views.Count(v => v.ViewedAt >= startOfWeek);
                var dailyViews = views.Count(v => v.ViewedAt >= startOfDay);

                // 建立 DTO
                var dto = new ProgramPlanDto
                {
                    Id = plan.Id,
                    CompanyName = companyName,
                    CompanyLogo = logo,
                    CompanyCover = cover,
                    SerialNum = plan.SerialNum,
                    Name = plan.Name,
                    Intro = plan.Intro,
                    IndustryId = plan.IndustryId,
                    JobTitleId = plan.JobTitleId,
                    Address = plan.Address,
                    AddressMap = plan.AddressMap,
                    ContactName = plan.ContactName,
                    ContactPhone = plan.ContactPhone,
                    ContactEmail = plan.ContactEmail,
                    MinPeople = plan.MinPeople,
                    MaxPeople = plan.MaxPeople,
                    PublishStartDate = plan.PublishStartDate,
                    PublishDurationDays = plan.PublishDurationDays,
                    PublishEndDate = plan.PublishEndDate,
                    ProgramStartDate = plan.ProgramStartDate,
                    ProgramEndDate = plan.ProgramEndDate,
                    ProgramDurationDays = (plan.ProgramEndDate - plan.ProgramStartDate).Days + 1, // 修正
                    StatusId = plan.StatusId,
                    StatusTitle = plan.Status.Title,
                    ViewsCount = plan.ViewsCount, // 快取欄位
                    FavoritesCount = plan.FavoritesCount,
                    AppliedCount = appliedCount,

                    Industry = new ProgramPlanDto.SimpleEntityDto { Id = plan.Industry.Id, Title = plan.Industry.Title },
                    JobTitle = new ProgramPlanDto.SimpleEntityDto { Id = plan.JobTitle.Id, Title = plan.JobTitle.Title },
                    Status = new ProgramPlanDto.SimpleEntityDto { Id = plan.Status.Id, Title = plan.Status.Title },

                    Steps = steps,
                    Images = images,

                    TotalViews = totalViews,
                    WeeklyViews = weeklyViews,
                    DailyViews = dailyViews
                };

                // 判斷 DaysLeft
                if (now < plan.ProgramStartDate)
                {
                    dto.DaysLeft = Math.Max(0, (plan.ProgramStartDate - now).Days);
                }
                else if (now >= plan.ProgramStartDate && now <= plan.ProgramEndDate)
                {
                    dto.DaysLeft = null; // 可改成 (plan.ProgramEndDate - now).Days 顯示剩餘天數
                }
                else
                {
                    dto.DaysLeft = null;
                }

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
                    hasViewedToday = db.ProgramViews.Any(v =>
                        v.ProgramPlanId == plan.Id &&
                        v.ViewerUserId == userId.Value &&
                        v.ViewedAt >= startOfDay
                    );
                }
                else if (!string.IsNullOrEmpty(viewerIp))
                {
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
                        ViewerUserId = userId,
                        ViewerIp = viewerIp
                    });

                    // 同步更新 ViewsCount 快取
                    plan.ViewsCount += 1;

                    // 更新熱門分數
                    plan.Score = plan.ViewsCount * 1
                                 + plan.FavoritesCount * 3
                                 + plan.AppliedCount * 5;

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

        // GET: api/v1/users/{userId}/programs/{programId}/evaluation 體驗者取得單一體驗評價頁資訊(填寫評價內容)
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

        // GET: api/v1/users/{userId}/evaluations  體驗者取得自己所有體驗的評價資訊
        [HttpGet]
        [Route("~/api/v1/users/{userId}/evaluations")]
        [JwtAuthFilter]
        public IHttpActionResult GetParticipantEvaluations(
            int userId,
            string search = null,
            int? status_id = null,
            string sort = "date_desc",
            int page = 1,
            int limit = 20)
        {
            var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                return NotFound();

            int participantId = participant.Id;

            var query = db.ParticipantEvaluations
                .Where(e => e.ParticipantId == participantId);

            // 搜尋關鍵字 (體驗名稱、企業名稱、評價內容)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    e.Program.Name.Contains(search) ||
                    e.Program.Company.Name.Contains(search) ||
                    e.Comment.Contains(search));
            }

            // 篩選狀態
            switch (status_id)
            {
                case 15: // 已通過
                    query = query.Where(e =>  e.StatusId == 4);
                    break;
                case 16: // 已拒絕
                    query = query.Where(e => e.StatusId == 5 );
                    break;
                case 17: // 未評價 
                    query = query.Where(e => e.StatusId == 17);
                    break;
                default:
                    // 不過濾
                    break;
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

            // 分頁
            var evaluations = query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(e => new ParticipantEvaluationDto
                {
                    StatusId = (ReviewStatus)e.StatusId,
                    Score = e.Score,
                    Comment = e.Comment,
                    SerialNum = e.SerialNum,
                    ProgramName = e.Program.Name,
                    ProgramStartDate = e.Program.ProgramStartDate,
                    ProgramEndDate = e.Program.ProgramEndDate,
                    CompanyName = e.Program.Company.Name,
                    CompanyLogo = e.Program.Company.CompanyImages
                                        .OrderBy(ci => ci.Id)
                                        .Select(ci => ci.ImgPath)
                                        .FirstOrDefault(),
                    EvaluationDate = e.CreatedAt
                })
            .ToList();

            var result = new
            {
                TotalCount = totalCount,
                Page = page,
                Limit = limit,
                Data = evaluations
            };

            return Ok(evaluations);
        }

        // GET: api/HomePage 取得熱門體驗清單與體驗者評價
        [Route("~/api/v1/HomePage")]
        public IHttpActionResult GetPopularProgramPlans()
        {
            // 1. 取得熱門體驗
            var popularPlans = db.ProgramPlan
                .Where(p => p.PublishEndDate >= DateTime.Today && (p.StatusId == 2 || p.StatusId == 4))
                .OrderByDescending(p => p.Score)
                .Take(10)
                .ToList();

            var popularDtoList = popularPlans.Select(plan =>
            {
                var cover = db.ProgramPlanImages
                    .Where(img => img.ProgramPlanId == plan.Id)
                    .OrderBy(img => img.Id)
                    .FirstOrDefault();
 
                return new HomePageDto
                {
                    Id = plan.Id,
                    Score = plan.Score,
                    // 三個計數
                    ViewsCount = plan.ViewsCount,
                    FavoritesCount = plan.FavoritesCount,
                    AppliedCount = plan.AppliedCount,
                    CompanyName = plan.Company.Name,
                    Name = plan.Name,
                    Intro = plan.Intro,
                    Address = plan.Address,
                    ProgramStartDate = plan.ProgramStartDate,
                    ProgramEndDate = plan.ProgramEndDate,
                    DaysLeft = (plan.PublishEndDate - DateTime.Today).Days,
                    StatusId = plan.StatusId,
                    StatusTitle = plan.Status.Title,
                    CoverId = cover?.Id ?? 0,
                    ImgPath = cover?.ImgPath
                };
            }).ToList();

            // 2. 取得最新高分評價（join ParticipantInfoes + Identity）
            var latestEvaluations = (from ev in db.ParticipantEvaluations
                                     join p in db.ParticipantInfoes on ev.ParticipantId equals p.Id
                                     join i in db.Identity on p.IdentityId equals i.Id
                                     join prog in db.ProgramPlan on ev.ProgramPlanId equals prog.Id
                                     where ev.Score >= 4 && (prog.StatusId == 2 || prog.StatusId == 4)
                                     orderby ev.UpdatedAt descending
                                     select new
                                     {
                                         Participant = p,
                                         IdentityTitle = i.Title,
                                         Comment = ev.Comment,
                                         Score = ev.Score,
                                         EvaluationDate = ev.UpdatedAt
                                     })
                         .Take(10)
                         .ToList() // ← 先把資料抓到記憶體
                         .Select(x => new
                         {
                             ParticipantName = MaskName(x.Participant.Name),
                             IdentityTitle = x.IdentityTitle,
                             Age = DateTime.Today.Year - x.Participant.Birthday.Year -
                                   (DateTime.Today.DayOfYear < x.Participant.Birthday.DayOfYear ? 1 : 0),
                             Comment = x.Comment,
                             Score = x.Score,
                             EvaluationDate = x.EvaluationDate
                         })
                         .ToList();

            // 3. 回傳單一 JSON
            var result = new
            {
                PopularPrograms = popularDtoList,
                LatestHighScoreEvaluations = latestEvaluations
            };

            return Ok(result);
        }

        // GET: api/v1/users/{userId}/favorites 取得收藏體驗列表資訊
        [HttpGet]
        [Route("~/api/v1/users/{userId}/favorites")]
        [JwtAuthFilter]
        public IHttpActionResult GetUserFavorites(
            int userId,
            string search = null,
            int? industry_id = null,
            int? job_title_id = null,
            string city_id = null,
            string district_id = null,
            string sort = "publish_start_desc",
            int page = 1,
            int limit = 21)
        {
            try
            {
                var now = DateTime.Now;

                //轉換: userid 對應到 participantId
                var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
                if (participant == null) return NotFound();

                var participantId = participant.Id;

                // 取出該使用者的收藏紀錄
                var favoriteQuery = db.Favorites
                    .Where(f => f.ParticipantId == participantId)
                    .Select(f => f.ProgramPlan)
                    .Include(p => p.Industry)
                    .Include(p => p.JobTitle)
                    .Include(p => p.Status)
                    .Include(p => p.Steps)
                    .Include(p => p.ProgramPlanImages)
                    .AsQueryable();

                // 過濾掉不符狀態或刊登過期的體驗
                favoriteQuery = favoriteQuery.Where(p =>
                    (p.StatusId == 2 || p.StatusId == 4 || p.StatusId == 7 || p.StatusId == 15) &&
                    p.PublishEndDate >= now
                );

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(search))
                {
                    favoriteQuery = favoriteQuery.Where(p =>
                        p.Name.Contains(search)
                    );
                }

                // 產業篩選
                if (industry_id.HasValue)
                {
                    favoriteQuery = favoriteQuery.Where(p => p.IndustryId == industry_id.Value);
                }

                // 職務篩選
                if (job_title_id.HasValue)
                {
                    favoriteQuery = favoriteQuery.Where(p => p.JobTitleId == job_title_id.Value);
                }

                if (!string.IsNullOrEmpty(city_id))
                {
                    var cityName = db.City
                                     .Where(c => c.Id.ToString() == city_id)
                                     .Select(c => c.Name)
                                     .FirstOrDefault();

                    if (!string.IsNullOrEmpty(cityName))
                    {
                        favoriteQuery = favoriteQuery.Where(p => p.Address.Contains(cityName));

                        // 如果同時選擇鄉鎮
                        if (!string.IsNullOrEmpty(district_id))
                        {
                            var districtName = db.Districts
                                                 .Where(d => d.Id.ToString() == district_id)
                                                 .Select(d => d.Name)
                                                 .FirstOrDefault();
                            if (!string.IsNullOrEmpty(districtName))
                            {
                                favoriteQuery = favoriteQuery.Where(p => p.Address.Contains(districtName));
                            }
                        }
                    }
                }

                // 排序 
                switch (sort)
                {
                    case "publish_start_asc": //刊登開始日期舊到新
                        favoriteQuery = favoriteQuery.OrderBy(p => p.PublishStartDate);
                        break;
                    case "publish_start_desc": //刊登開始日期新到舊
                        favoriteQuery = favoriteQuery.OrderByDescending(p => p.PublishStartDate);
                        break;
                    case "publish_end_asc": //刊登結束日期舊到新
                        favoriteQuery = favoriteQuery.OrderBy(p => p.PublishEndDate);
                        break;
                    case "publish_end_desc": //刊登結束日期新到舊
                        favoriteQuery = favoriteQuery.OrderByDescending(p => p.PublishEndDate);
                        break;
                    case "program_start_asc": //體驗開始日期舊到新
                        favoriteQuery = favoriteQuery.OrderBy(p => p.ProgramStartDate);
                        break;
                    case "program_start_desc": //體驗開始日期新到舊
                        favoriteQuery = favoriteQuery.OrderByDescending(p => p.ProgramStartDate);
                        break;
                    case "hot":
                        favoriteQuery = favoriteQuery.OrderByDescending(p =>
                            p.ViewsCount * 1 +
                            p.FavoritesCount * 3 +
                            db.ProgramSubmits.Count(a => a.ProgramPlanId == p.Id) * 5
                        );
                        break;
                    default:
                        favoriteQuery = favoriteQuery.OrderByDescending(p => p.PublishStartDate);
                        break;
                }

                // 分頁
                var total = favoriteQuery.Count();
                var items = favoriteQuery
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToList()
                    .Select(p =>
                    {
                        var appliedCount = db.ProgramSubmits.Count(a => a.ProgramPlanId == p.Id);

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

                        var viewsCount = p.ViewsCount;
                        var favoritesCount = db.Favorites.Count(f => f.ProgramPlanId == p.Id);
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
                            AppliedCount = appliedCount,
                            DaysLeft = daysLeft,
                            CoverImage = p.ProgramPlanImages
                                .OrderBy(img => img.Id)
                                .Select(img => img.ImgPath)
                                .FirstOrDefault(),
                            IsOngoing = isOngoing,
                        };
                    })
                    .ToList();

                string message = total == 0 ? "尚未收藏任何體驗計畫" : null;

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

        //GET:api/v1/programs/{programId}/company 取得企業詳情頁資訊
        [HttpGet]
        [Route("{programId}/company")]
        public IHttpActionResult GetCompanyDetail(int programId)
        {
            var now = DateTime.Now;

            var program = db.ProgramPlan
                         .Include(p => p.Company)
                         .Include(p => p.Company.CompanyContacts)
                         .Include(p => p.Company.CompanyImages)
                         .Include(p => p.Company.ProgramPlans.Select(pp => pp.ProgramPlanImages))
                         .Include(p => p.Company.ProgramPlans.Select(pp => pp.Industry))
                         .Include(p => p.Company.ProgramPlans.Select(pp => pp.JobTitle))
                         .FirstOrDefault(p => p.Id == programId);

            if (program == null || program.Company == null)
                return NotFound();

            var company = program.Company;

            var dto = new
            {
                company.Id,
                company.Name,
                company.Intro,
                Industry = company.Industry?.Title,
                company.Website,
                company.Address,
                EmployeeNum = company.Scales?.EmployeeNum,
                ContactName = company.CompanyContacts.Name,
                ContactJobTitle = company.CompanyContacts.JobTitle,
                ContactEmail = company.CompanyContacts.Email,
                ContactPhone = company.CompanyContacts.Phone,
                CoverImage = company.CompanyImages.FirstOrDefault(i => i.Type == "cover")?.ImgPath,
                EnvironmentImages = company.CompanyImages.Where(i => i.Type == "environment").Select(i => i.ImgPath).ToList(),
                ProgramPlans = company.ProgramPlans
                .Where(p => (p.StatusId == 2 || p.StatusId == 4 || p.StatusId == 15)
                            && p.PublishEndDate > now)
                .Select(p =>
                {
                    var appliedCount = db.ProgramSubmits.Count(a => a.ProgramPlanId == p.Id);
                    int? daysLeft = null;
                    bool? isOngoing = null;

                    if (now < p.ProgramStartDate)
                        daysLeft = (p.PublishEndDate - now).Days;
                    else if (now >= p.ProgramStartDate && now <= p.ProgramEndDate)
                        isOngoing = true;
                    else
                        isOngoing = false;

                    var score = p.ViewsCount * 1 + p.FavoritesCount * 3 + appliedCount * 5;

                    return new
                    {
                        p.Id,
                        p.Name,
                        p.Intro,
                        p.Address,
                        Industry = p.Industry != null ? new { p.Industry.Id, p.Industry.Title } : null,
                        JobTitle = p.JobTitle != null ? new { p.JobTitle.Id, p.JobTitle.Title } : null,
                        p.PublishStartDate,
                        p.PublishEndDate,
                        p.ProgramStartDate,
                        p.ProgramEndDate,
                        CoverImage = p.ProgramPlanImages.OrderBy(img => img.Id).Select(img => img.ImgPath).FirstOrDefault(),
                        AppliedCount = appliedCount,
                        DaysLeft = daysLeft,
                        IsOngoing = isOngoing,
                        ViewsCount = p.ViewsCount,
                        FavoritesCount = p.FavoritesCount,
                        Score = score
                    };
                })
                .ToList()
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

        // PUT: api/v1/users/{userId}/programs/{programId}/evaluations  體驗者修改評價
        [HttpPut]
        [Route("~/api/v1/users/{userId}/programs/{programId}/evaluations")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> UpdateReview(int userId, int programId, [FromBody] ParticipantEvaluationDto dto)
        {
            // 驗證 ModelState
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 讀取 OpenAI API Key
            var jsonText = File.ReadAllText(HttpContext.Current.Server.MapPath("~/OpenAISettings.json"));
            dynamic jsonObj = JsonConvert.DeserializeObject(jsonText);
            string apiKey = jsonObj.OpenAI.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
                return InternalServerError(new Exception("OpenAI API Key 未設定"));

            // Moderation API 審核
            bool flagged = false;
            var flaggedCategories = new List<string>();
            string moderationResultJson = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var payload = new
                {
                    model = "omni-moderation-latest",
                    input = dto.Comment
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var moderationResponse = await client.PostAsync("https://api.openai.com/v1/moderations", content);
                moderationResultJson = await moderationResponse.Content.ReadAsStringAsync();

                if (!moderationResponse.IsSuccessStatusCode)
                {
                    return Content(HttpStatusCode.ServiceUnavailable, new
                    {
                        message = "Moderation API 呼叫失敗，請稍後再試。",
                        error = moderationResultJson
                    });
                }

                // 解析回傳結果
                var moderationObj = JsonConvert.DeserializeObject<ModerationResponse>(moderationResultJson);
                if (moderationObj?.Results != null && moderationObj.Results.Count > 0)
                {
                    var first = moderationObj.Results[0];
                    if (first.Categories != null)
                    {
                        foreach (var kv in first.Categories)
                        {
                            if (kv.Value)
                                flaggedCategories.Add(kv.Key);
                        }
                    }
                    flagged = flaggedCategories.Count > 0;
                }
            }

            // 找 ParticipantInfo
            var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
            if (participant == null) return BadRequest("找不到對應的體驗者");

            // 找 ProgramSubmit
            var application = db.ProgramSubmits
                .FirstOrDefault(s => s.ParticipantId == participant.Id && s.ProgramPlanId == programId);
            if (application == null) return BadRequest("找不到對應的體驗申請");
            if (application.StatusId != 2) return BadRequest("尚未通過企業審核");

            // 找 ProgramPlan
            var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId);
            if (program == null) return BadRequest("找不到該體驗計畫");
            if (DateTime.Now <= program.ProgramEndDate) return BadRequest("體驗尚未結束");

            // 找已有評價
            var existingReview = db.ParticipantEvaluations
                .FirstOrDefault(r => r.ProgramPlanId == programId && r.ParticipantId == participant.Id);
            if (existingReview == null) return BadRequest("尚未提交過評價，請使用 POST 新增");

            // 更新評價
            existingReview.Score = dto.Score;
            existingReview.Comment = dto.Comment;
            existingReview.StatusId = flagged ? 3 : 2; // AI審核結果
            db.SaveChanges(); // 更新 ParticipantEvaluation

            // 建立新的 AI 系統審核結果
            var aiReview = new EvaluationReview
            {
                EvaluationId = existingReview.Id,
                ReviewedAt = DateTime.Now,
                ReviewerId = 11,
                ReviewTypeId = ReviewTypeEnum.System,
                StatusId = flagged ? 3 : 2,
                Comment = flagged ? string.Join(",", flaggedCategories) : "系統自動審核通過"
            };
            db.EvaluationReviews.Add(aiReview);
            db.SaveChanges();

            // 組裝回傳
            var reviewResponse = new
            {
                ProgramName = program.Name,
                ProgramStartDate = program.ProgramStartDate,
                ProgramEndDate = program.ProgramEndDate,
                CompanyName = program.Company.Name,
                Score = existingReview.Score,
                Comment = existingReview.Comment,
                SerialNum = existingReview.SerialNum,
                AiStatus = aiReview.StatusId
            };

            if (flagged)
                return Content(HttpStatusCode.BadRequest, new { message = "評價內容被判定為不當，請修改後再提交。", review = reviewResponse });

            return Ok(reviewResponse);
        }

        // PUT: api/Programs/{program_id}/application/{application_Id} 體驗者取消申請體驗
        [HttpPut]
        [Route("{program_id}/applications/{application_id}/cancel")]
        [JwtAuthFilter]
        [ResponseType(typeof(ProgramSubmit))]
        public async Task<IHttpActionResult> CancelProgramSubmit(int program_id, int application_id)
        {
            try
            {
                // 1. 驗證登入
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int userId = (int)userIdObj;

                // 2. 取得要取消的 ProgramSubmit
                var submit = db.ProgramSubmits.FirstOrDefault(s => s.Id == application_id && s.Participant.UserId == userId);
                if (submit == null)
                {
                    return NotFound();
                }

                // 2.1 防止重複取消
                if (submit.StatusId == (int)ReviewStatus.Cancelled)
                {
                    return BadRequest("此申請已取消");
                }

                // 3. 從 Request body 讀取消原因
                string body = await Request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(body))
                {
                    return BadRequest("取消原因為必填");
                }

                string cancelReason;

                try
                {
                    // 嘗試解析 JSON，如果前端傳的是 {"cancel_reason": "家裡有事"} 格式
                    dynamic obj = JsonConvert.DeserializeObject<dynamic>(body);
                    cancelReason = obj.cancel_reason != null ? (string)obj.cancel_reason : body;
                }
                catch
                {
                    // 如果不是 JSON 格式，直接當成文字
                    cancelReason = body;
                }

                // 4. 更新狀態與取消原因
                submit.StatusId = (int)ReviewStatus.Cancelled;
                submit.CancelReason = cancelReason.Trim(); // 去掉多餘空白
                submit.CancelAt = DateTime.Now;

                db.SaveChanges();

                // 更新 ProgramPlan 的熱門分數
                // ------------------------
                var program = db.ProgramPlan.FirstOrDefault(p => p.Id == program_id);
                if (program != null)
                {
                    // 更新申請通過數
                    program.AppliedCount = db.ProgramSubmits
                        .Where(a => a.ProgramPlanId == program_id && a.StatusId == (int)ReviewStatus.Approved)
                        .Sum(a => (int?)a.ParticipantsCount) ?? 0;

                    // 計算熱門分數
                    program.Score = program.ViewsCount * 1
                                  + program.FavoritesCount * 3
                                  + program.AppliedCount * 5;

                    db.SaveChanges();
                }
                    // 5. 回傳乾淨 JSON
                    return Ok(new
                {
                    success = true,
                    message = "申請已取消",
                    cancel_reason = cancelReason,
                    cancel_at = submit.CancelAt
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
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

                //5.驗證 MotivationContent(新增的驗證步驟)
                if (string.IsNullOrWhiteSpace(dto.MotivationContent))
                {
                    return BadRequest("申請動機欄位為必填");
                }

                // 6. 防止重複申請同一個計畫
                var existingSubmit = db.ProgramSubmits
                    .FirstOrDefault(s => s.ProgramPlanId == programId && s.ParticipantId == participant.Id);
                if (existingSubmit != null)
                    return BadRequest("已申請該體驗計畫");

                // 7. 驗證 Resume
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

                // 8. 生成申請編號 PA-2025-0818-001
                string prefix = "PA";
                string year = DateTime.Now.Year.ToString();
                string shortDate = DateTime.Now.ToString("MMdd"); // MMdd
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                int countToday = db.ProgramSubmits
                    .Count(s => s.SubmitAt >= today && s.SubmitAt < tomorrow) + 1;
                string participantSerialNumber = $"{prefix}-{year}-{shortDate}-{countToday:D3}";

                // 9. 建立 ProgramSubmit
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
        public async Task<IHttpActionResult> SubmitReview(int userId, int programId, [FromBody] ParticipantEvaluationDto dto)
        {
            // 驗證 ModelState
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 讀取 OpenAI API Key
            var jsonText = File.ReadAllText(HttpContext.Current.Server.MapPath("~/OpenAISettings.json"));
            dynamic jsonObj = JsonConvert.DeserializeObject(jsonText);
            string apiKey = jsonObj.OpenAI.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
                return InternalServerError(new Exception("OpenAI API Key 未設定"));

            // Moderation API 審核
            bool flagged = false;
            var flaggedCategories = new List<string>();
            string moderationResultJson = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var payload = new
                {
                    model = "omni-moderation-latest",
                    input = dto.Comment
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var moderationResponse = await client.PostAsync("https://api.openai.com/v1/moderations", content);
                moderationResultJson = await moderationResponse.Content.ReadAsStringAsync();

                if (!moderationResponse.IsSuccessStatusCode)
                {
                    return Content(HttpStatusCode.ServiceUnavailable, new
                    {
                        message = "Moderation API 呼叫失敗，請稍後再試。",
                        error = moderationResultJson
                    });
                }

                    // 解析回傳結果
                    var moderationObj = JsonConvert.DeserializeObject<ModerationResponse>(moderationResultJson);
                if (moderationObj?.Results != null && moderationObj.Results.Count > 0)
                {
                    var first = moderationObj.Results[0];
                    if (first.Categories != null)
                    {
                        foreach (var kv in first.Categories)
                        {
                            if (kv.Value)
                                flaggedCategories.Add(kv.Key);
                        }
                    }
                    flagged = flaggedCategories.Count > 0;
                }
            }

            // 找 ParticipantInfo
            var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
            if (participant == null) return BadRequest("找不到對應的體驗者");

            // 找 ProgramSubmit
            var application = db.ProgramSubmits
                .FirstOrDefault(s => s.ParticipantId == participant.Id && s.ProgramPlanId == programId);
            if (application == null) return BadRequest("找不到對應的體驗申請");
            if (application.StatusId != 2) return BadRequest("尚未通過企業審核");

            // 找 ProgramPlan
            var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId);
            if (program == null) return BadRequest("找不到該體驗計畫");
            if (DateTime.Now <= program.ProgramEndDate) return BadRequest("體驗尚未結束");

            // 檢查是否已有評價
            var existingReview = db.ParticipantEvaluations
                .FirstOrDefault(r => r.ProgramPlanId == programId && r.ParticipantId == participant.Id);
            if (existingReview != null) return BadRequest("該體驗已提交評價");

            // 產生評價編號
            string todayStr = DateTime.Now.ToString("yyyyMMdd");
            int todayCount = db.ParticipantEvaluations.Count(r => r.SerialNum.StartsWith("REV-" + todayStr + "-"));
            string serialNum = $"REV-{todayStr}-{(todayCount + 1).ToString("D4")}";

            // 建立 ParticipantEvaluation
            var review = new ParticipantEvaluation
            {
                ParticipantId = participant.Id,
                ProgramPlanId = programId,
                Score = dto.Score,
                Comment = dto.Comment,
                SerialNum = serialNum,
                StatusId = flagged ? 3 : 2 // AI審核結果 3=System Rejected, 2=System Pass
            };
            db.ParticipantEvaluations.Add(review);
            db.SaveChanges(); // 先存 review 取得 Id

            // 建立 AI 系統審核結果
            var aiReview = new EvaluationReview
            {
                EvaluationId = review.Id,
                ReviewedAt = DateTime.Now,
                ReviewerId = 11,
                ReviewTypeId = ReviewTypeEnum.System,
                StatusId = flagged ? 3 : 2,
                Comment = flagged ? string.Join(",", flaggedCategories) : "通過"
            };
            db.EvaluationReviews.Add(aiReview);
            db.SaveChanges();

            // 組裝回傳
            var reviewResponse = new
            {
                ProgramName = program.Name,
                ProgramStartDate = program.ProgramStartDate,
                ProgramEndDate = program.ProgramEndDate,
                CompanyName = program.Company.Name,
                Score = review.Score,
                Comment = review.Comment,
                SerialNum = review.SerialNum,
                AiStatus = aiReview.StatusId
                //AiComment = aiReview.Comment,
                //ModerationRawJson = moderationResultJson // Postman 可直接看到 AI 回傳
            };

            if (flagged)
                return Content(HttpStatusCode.BadRequest, new { message = "評價內容被判定為不當，請修改後再提交。", review = reviewResponse });

            return Ok(reviewResponse);
        }

        //POST: /api/v1/favorites 體驗者收藏體驗計畫
        [HttpPost]
        [Route("~/api/v1/favorites")]
        [JwtAuthFilter]
        public IHttpActionResult AddFavorite([FromBody] FavoriteDto dto)
        {
            try
            {
                // 驗證 ModelState
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                //  驗證登入
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int userId = (int)userIdObj;


                var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                    return NotFound();

                // 驗證 ProgramPlan 是否存在且已通過
                var program = db.ProgramPlan.FirstOrDefault(p =>
                    p.Id == dto.ProgramPlanId &&
                    (p.StatusId == 2 || p.StatusId == 4 || p.StatusId == 15));

                if (program == null)
                    return BadRequest("該體驗計畫尚未通過發布或不存在");

                // 驗證刊登是否已結束
                if (program.PublishEndDate < DateTime.Today)
                    return BadRequest("該體驗計畫刊登已結束");

                // 檢查是否已經收藏
                var exists = db.Favorites.Any(f => f.ParticipantId == participant.Id && f.ProgramPlanId == dto.ProgramPlanId);
                if (exists)
                    return BadRequest("已收藏此體驗計畫");

                // 新增 Favorite
                var favorite = new Favorite
                {
                    ParticipantId = participant.Id,
                    ProgramPlanId = dto.ProgramPlanId,
                    CreatedAt = DateTime.Now
                };

                db.Favorites.Add(favorite);
                db.SaveChanges();

                // 更新 ProgramPlan 的 FavoritesCount
                program.FavoritesCount = db.Favorites.Count(f => f.ProgramPlanId == dto.ProgramPlanId);

                // 更新熱門分數
                program.Score = program.ViewsCount * 1
                                + program.FavoritesCount * 3
                                + program.AppliedCount * 5;
                db.SaveChanges();

                return Ok(new { Message = "成功收藏體驗計畫" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        //DELETE: api/v1/favorites 體驗者取消收藏體驗計畫
        [HttpDelete]
        [Route("~/api/v1/favorites/{programPlanId:int}")]
        [JwtAuthFilter]
        public IHttpActionResult RemoveFavorite(int programPlanId)
        {
            try
            {
                // 驗證登入
                if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    return Unauthorized();
                }
                int userId = (int)userIdObj;

                // 找到 participant
                var participant = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == userId);
                if (participant == null)
                    return NotFound();

                // 找到收藏紀錄
                var favorite = db.Favorites.FirstOrDefault(f => f.ParticipantId == participant.Id && f.ProgramPlanId == programPlanId);
                if (favorite == null)
                    return BadRequest("尚未收藏此體驗計畫");

                db.Favorites.Remove(favorite);
                db.SaveChanges();

                // 更新 ProgramPlan 的 FavoritesCount 與熱門分數
                var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programPlanId);
                if (program != null)
                {
                    // 更新收藏數
                    program.FavoritesCount = db.Favorites.Count(f => f.ProgramPlanId == programPlanId);

                    // 更新申請通過數
                    program.AppliedCount = db.ProgramSubmits
                        .Where(a => a.ProgramPlanId == programPlanId && a.StatusId == 2)
                        .Sum(a => (int?)a.ParticipantsCount) ?? 0;

                    // 更新瀏覽次數（可選，如果你想保證最新）
                    program.ViewsCount = db.ProgramViews.Count(v => v.ProgramPlanId == programPlanId);

                    // 計算熱門分數，權重可調整
                    program.Score = program.ViewsCount * 1
                                  + program.FavoritesCount * 3
                                  + program.AppliedCount * 5;

                    db.SaveChanges();
                }

                return Ok(new { Message = "已取消收藏" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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

        // 工具方法：隱藏名字中間字，HomePage撈體驗者名字用
        private string MaskName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return fullName;

            if (fullName.Length <= 2)
            {
                // 如果名字只有兩個字 → 保留第一字，第二字遮住
                return fullName[0] + "O";
            }

            // 三個字以上 → 保留第一個與最後一個，中間全部換成 O
            var middleMasked = new string('O', fullName.Length - 2);
            return $"{fullName[0]}{middleMasked}{fullName[fullName.Length - 1]}";
        }
    }
}