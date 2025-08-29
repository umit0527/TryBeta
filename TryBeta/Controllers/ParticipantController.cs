using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;
using static TryBeta.Models.CompanInfoDto;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/users")]
    public class ParticipantController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Users
        public IQueryable<Users> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Users/5 取得基本資料
        [HttpGet]
        [Route("{participantid:int}")]
        [JwtAuthFilter]
        [ResponseType(typeof(ParticipantRegisterDto))]
        public IHttpActionResult GetParticipant(int id)
        {
            // 先從 JwtAuthFilter 裡取得 UserId
            if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
            {
                return Unauthorized();
            }
            int userId = (int)userIdObj;

            // 權限判斷：確認該 userId 是否屬於 id 這個體驗者
            bool hasAccess = db.ParticipantInfoes.Any(c => c.Id == id && c.UserId == userId);
            if (!hasAccess)
            {
                var resp = Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "權限不足" });
                return ResponseMessage(resp);
            }

            // 1. 從資料庫抓
            var participantEntity = db.ParticipantInfoes.Include(p => p.Identity)
                                                        .Include(p => p.User)
                                                        .FirstOrDefault(c => c.Id == id);
            if (participantEntity == null)
            {
                return NotFound();
            }

            // 2. Entity轉DTO（這裡簡單手動轉）
            var dto = new ParticipantDto
            {
                Name = participantEntity.Name,
                Phone = participantEntity.Phone,
                Birthday = participantEntity.Birthday,
                Headshot = participantEntity.Headshot,
                CityId = participantEntity.CityId,
                DistrictId = participantEntity.DistrictId,
                Street = participantEntity.Street,
                IdentityId = participantEntity.IdentityId,
                IdentityElse = participantEntity.IdentityElse,
                IdentityName = participantEntity.Identity?.Title, // Identity 對應欄位
                Gender = participantEntity.Gender
            };

            // 3. 回傳DTO
            return Ok(dto);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUsers(int id, Users users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != users.Id)
            {
                return BadRequest();
            }

            db.Entry(users).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
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

        // POST: api/Users 註冊
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(Users))]
        public IHttpActionResult PostUsers(ParticipantRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage)
                                  .ToList();
                var content = new { status = 400, message = "資料驗證失敗", errors = errors };
                return Content(HttpStatusCode.BadRequest, content);
            }

            // 檢查帳號或 Email 是否已存在
            if (db.Users.Any(u => u.Account == dto.Account))
            {
                return BadRequest("該帳號已被使用");
            }

            if (db.Users.Any(u => u.Email == dto.Email))
            {
                return BadRequest("該 Email 已被使用");
            }

            // 加密密碼
            var hashedPassword = PasswordHasher.HashPassword(dto.Password); // 將密碼(明碼)加鹽雜湊

            // 若帳號和email是獨立 User 表的資料，需要先建立 User
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 建立 Users 物件
                    var user = new Users
                    {
                        Role = "Participant", // 體驗/參與者，Participant較常見
                        Account = dto.Account,
                        Email = dto.Email,
                        PasswordHash = hashedPassword,
                        StatusId = 1, // 預設啟用
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    transaction.Commit();

                    return Content(HttpStatusCode.Created, new
                    {
                        status = 201,
                        message = "註冊成功",
                        id = user.Id,
                        Role = "Participant", // 體驗/參與者，Participant較常見
                        Account = dto.Account,
                        Email = dto.Email,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }

        // POST: api/users/google/register Google註冊
        [HttpPost]
        [Route("google/register")]
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> PostGoogleRegister(GoogleDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Token))
            {
                return BadRequest("缺少 Google 登入 Token");
            }

            Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.Token);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "無效的 Google Token"
                });
            }

            // 確保 db 不為 null
            if (db == null)
            {
                return InternalServerError(new Exception("資料庫上下文未初始化"));
            }

            if (string.IsNullOrEmpty(payload.Email))
            {
                return BadRequest("Google Token 中沒有 Email");
            }

            var user = db.Users.FirstOrDefault(u => u.Email == payload.Email && u.Role == "Participant");

            //重複註冊的訊息
            if (user != null)
            {
                return Content(HttpStatusCode.Conflict, new
                {
                    status = 409,
                    message = "此帳號已存在，請直接登入"
                });
            }
            if (user == null)
            {

                user = new Users
                {
                    Account = payload.Email.Length > 50 ? payload.Email.Substring(0, 50) : payload.Email,
                    Email = payload.Email,
                    Role = "Participant",
                    GoogleId = payload.Subject.Length > 100 ? payload.Subject.Substring(0, 100) : payload.Subject,
                    PasswordHash = "GOOGLE",
                    StatusId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    var errors = ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                        .ToList();
                    return BadRequest("EF 驗證失敗: " + string.Join("; ", errors));
                }
            }
            else if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = payload.Subject.Length > 100 ? payload.Subject.Substring(0, 100) : payload.Subject;
                user.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();
            }
            // 檢查 ParticipantInfoes 是否已存在
            var participantInfo = db.ParticipantInfoes.FirstOrDefault(p => p.UserId == user.Id);
            if (participantInfo == null)
            {
                participantInfo = new ParticipantInfoes
                {
                    UserId = user.Id,
                    Name = payload.Name ?? "未填", // 取 Google 名字
                    Phone = "0000000000",       // 避免空字串
                    Birthday = DateTime.UtcNow, // 使用今天作為暫時值
                    Headshot = payload.Picture ?? "default.jpg",   // 預設頭像
                    CityId = 1,                 // 有效 CityId
                    DistrictId = 1,             // 有效 DistrictId
                    Street = "未填",
                    IdentityId = 3,             // 有效 IdentityId
                    Gender = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.ParticipantInfoes.Add(participantInfo);
                db.SaveChanges();
            }

            // 確保 jwtUtil 不為 null
            var jwtUtil = new JwtAuthUtil();
            if (jwtUtil == null)
            {
                return InternalServerError(new Exception("JWT 工具未初始化"));
            }

            string token = jwtUtil.GenerateToken(user.Id, user.Account, "");

            return Ok(new
            {
                status = 200,
                message = "註冊成功",
                token = token,
                user = new
                {
                    user.Id,
                    user.Account,
                    user.Email,
                    user.Role,
                }
            });
        }

        // POST: api/users/google/login Google登入
        [HttpPost]
        [Route("google/login")]
        [ResponseType(typeof(Users))]
        public async Task<IHttpActionResult> PostGoogleLogin(GoogleDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Token))
                return BadRequest("缺少 Google 登入 Token");

            Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.Token);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "無效的 Google Token"
                });
            }

            if (string.IsNullOrEmpty(payload.Email))
                return BadRequest("Google Token 中沒有 Email");

            if (db == null)
                return InternalServerError(new Exception("資料庫上下文未初始化"));

            // 1️ 檢查 User 是否存在
            var user = db.Users.FirstOrDefault(u => u.Email == payload.Email && u.Role == "Participant");

            if (user == null)
            {
                // 帳號不存在 → 提示前端註冊
                return Content(HttpStatusCode.NotFound, new
                {
                    status = 404,
                    message = "此 Google 帳號尚未註冊，請先註冊"
                });
            }

            // 2️ 更新 GoogleId，如果尚未綁定
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = payload.Subject.Length > 100 ? payload.Subject.Substring(0, 100) : payload.Subject;
                user.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();
            }

            // 3️ 產生 JWT
            var jwtUtil = new JwtAuthUtil();
            string token = jwtUtil.GenerateToken(user.Id, user.Account, "");

            return Ok(new
            {
                status = 200,
                message = "登入成功",
                token = token,
                user = new
                {
                    user.Id,
                    user.Account,
                    user.Email,
                    user.Role
                }
            });
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(Users))]
        public IHttpActionResult DeleteUsers(int id)
        {
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            db.Users.Remove(users);
            db.SaveChanges();

            return Ok(users);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsersExists(int id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }
    }
}