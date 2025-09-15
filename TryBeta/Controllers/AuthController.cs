using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TryBeta.Models;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1")]
    public class AuthController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Auth
        public IQueryable<Users> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Auth/5
        [ResponseType(typeof(Users))]
        public IHttpActionResult GetUsers(int id)
        {
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            return Ok(users);
        }

        // PUT: api/Auth/5
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

        // POST: api/Auth 企業登入
        [HttpPost]
        [Route("company/login")]
        [ResponseType(typeof(Users))]
        public IHttpActionResult PostCompanyLogin(CompanyLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("登入資料有誤");
            }
                
            // 找出對應的帳號/email，限定身分是企業
            var user = db.Users.FirstOrDefault(u =>(u.Account == dto.Identifier || u.Email == dto.Identifier) && u.Role == "Company");

            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 驗證密碼
            bool isPasswordValid = PasswordHasher.VerifyPassword(user.PasswordHash, dto.Password);

            if (!isPasswordValid)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 撈取公司資料
            var company = db.Companyinfoes.FirstOrDefault(c => c.UserId == user.Id);

            // 產生 JWT Token
            var jwtUtil = new JwtAuthUtil();
            string token = jwtUtil.GenerateToken(user.Id, user.Account, company?.Name ?? "");

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
                    user.Role,
                }
                
            });
        }

        // POST: api/v1/company/logout 企業登出
        [HttpPost]
        [Route("company/logout")]
        [JwtAuthFilter] // 必須登入才能登出
        public IHttpActionResult PostCompanyLogout()
        {
            try
            {
                // 1. 取出 token
                var token = Request.Headers.Authorization?.Parameter;
                if (string.IsNullOrEmpty(token))
                    return BadRequest("未找到 token");

                // 2. 從 JWT payload 解析 userId 與過期時間
                var jwtUtil = new JwtAuthUtil();
                Dictionary<string, object> payload;
                try
                {
                    payload = jwtUtil.GetPayload(token);
                }
                catch
                {
                    return BadRequest("Token 無效或已過期");
                }

                if (!payload.ContainsKey("Id") || !payload.ContainsKey("Exp"))
                    return BadRequest("Token 資訊不完整");

                int userId = Convert.ToInt32(payload["Id"]);
                DateTime expiredAt = Convert.ToDateTime(payload["Exp"]);

                // 3. 存進黑名單
                using (var db = new TryBetaDbContext())
                {
                    db.TokenBlacklistes.Add(new TokenBlacklist
                    {
                        Token = token,
                        UserId = userId,
                        ExpiredAt = expiredAt,
                        CreatedAt = DateTime.UtcNow
                    });
                    db.SaveChanges();
                }

                return Ok(new { status = 200, message = "登出成功" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/Auth 體驗者登入
        [HttpPost]
        [Route("users/login")] 
        [ResponseType(typeof(Users))]
        public IHttpActionResult PostParticipantLogin(ParticipantLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("登入資料有誤");
            }

            // 找出對應的帳號/email，限定身分是體驗者
            var user = db.Users.FirstOrDefault(u => (u.Account == dto.Identifier || u.Email == dto.Identifier) && u.Role == "Participant");

            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 驗證密碼
            bool isPasswordValid = PasswordHasher.VerifyPassword(user.PasswordHash, dto.Password);

            if (!isPasswordValid)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 撈取公司資料
            var participant = db.Companyinfoes.FirstOrDefault(c => c.UserId == user.Id);

            // 產生 JWT Token
            var jwtUtil = new JwtAuthUtil();
            string token = jwtUtil.GenerateToken(user.Id, user.Account, participant?.Name ?? "");

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
                    user.Role,
                }
            });
        }

        //// POST: api/Auth/users/login/google
        //[HttpPost]
        //[Route("users/login/google")]
        //[ResponseType(typeof(Users))]
        //public async Task<IHttpActionResult> PostParticipantLoginWithGoogle(GoogleDto dto)
        //{
        //    if (!ModelState.IsValid || string.IsNullOrEmpty(dto.id_token))
        //    {
        //        return BadRequest("缺少 Google 登入 Token");
        //    }

        //    Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
        //    try
        //    {
        //        // 驗證 Google ID Token
        //        payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.id_token);
        //    }
        //    catch (Exception)
        //    {
        //        return Content(HttpStatusCode.Unauthorized, new
        //        {
        //            status = 401,
        //            message = "無效的 Google Token"
        //        });
        //    }

        //    // 使用 email 或 Google 子 ID 來查詢是否已存在帳號
        //    var user = db.Users.FirstOrDefault(u => u.Email == payload.Email && u.Role == "Participant");

        //    if (user == null)
        //    {
        //        // 自動註冊新帳號
        //        user = new Users
        //        {
        //            Account = payload.Email.Length > 50 ? payload.Email.Substring(0, 50) : payload.Email,
        //            Email = payload.Email,
        //            Role = "Participant",
        //            GoogleId = payload.Subject.Length > 100 ? payload.Subject.Substring(0, 100) : payload.Subject, // 已更新長度
        //            PasswordHash = "GOOGLE",
        //            StatusId = 1, // 啟用
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow
        //            // 可額外存 Name / Picture 等基本資料
        //        };

        //        db.Users.Add(user);
        //        db.SaveChanges();
        //    }
        //    else
        //    {
        //        // 已存在帳號，但 GoogleId 尚未綁定
        //        if (string.IsNullOrEmpty(user.GoogleId))
        //        {
        //            user.GoogleId = payload.Subject;
        //            user.UpdatedAt = DateTime.UtcNow;
        //            db.SaveChanges();
        //        }
        //    }

        //    // 產生 JWT Token
        //    var jwtUtil = new JwtAuthUtil();
        //    string token = jwtUtil.GenerateToken(user.Id, user.Account, "");

        //    return Ok(new
        //    {
        //        status = 200,
        //        message = "登入成功",
        //        token = token,
        //        user = new
        //        {
        //            user.Id,
        //            user.Account,
        //            user.Email,
        //            user.Role,
        //        }
        //    });
        //}

        // POST: api/v1/users/logout 體驗者登出
        [HttpPost]
        [Route("users/logout")]
        [JwtAuthFilter] // 必須登入才能登出
        public IHttpActionResult PostParticipantLogout()
        {
            try
            {
                // 1. 取出 token
                var token = Request.Headers.Authorization?.Parameter;
                if (string.IsNullOrEmpty(token))
                    return BadRequest("未找到 token");

                // 2. 從 JWT payload 解析 userId 與過期時間
                var jwtUtil = new JwtAuthUtil();
                Dictionary<string, object> payload;
                try
                {
                    payload = jwtUtil.GetPayload(token);
                }
                catch
                {
                    return BadRequest("Token 無效或已過期");
                }

                if (!payload.ContainsKey("Id") || !payload.ContainsKey("Exp"))
                    return BadRequest("Token 資訊不完整");

                int userId = Convert.ToInt32(payload["Id"]);
                DateTime expiredAt = Convert.ToDateTime(payload["Exp"]);

                // 3. 存進黑名單
                using (var db = new TryBetaDbContext())
                {
                    db.TokenBlacklistes.Add(new TokenBlacklist
                    {
                        Token = token,
                        UserId = userId,
                        ExpiredAt = expiredAt,
                        CreatedAt = DateTime.UtcNow
                    });
                    db.SaveChanges();
                }

                return Ok(new { status = 200, message = "登出成功" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/v1/admin/login 管理員登入
        [HttpPost]
        [Route("admin/login")]
        [ResponseType(typeof(Users))]
        public IHttpActionResult PostAdminLogin(AdminLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("登入資料有誤");
            }

            // 找出對應的帳號/email，限定身分是管理員
            var user = db.Users.FirstOrDefault(u =>
                (u.Account == dto.Identifier || u.Email == dto.Identifier) && u.Role == "Admin");

            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 驗證密碼
            bool isPasswordValid = false;

            if (user.Role == "Admin")
            {
                // 管理員帳號允許用明碼 (僅限測試用)
                isPasswordValid = (user.PasswordHash == dto.Password);
            }
            else
            {
                // 其他角色仍使用 Hash 驗證
                isPasswordValid = PasswordHasher.VerifyPassword(user.PasswordHash, dto.Password);
            }

            if (!isPasswordValid)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    status = 401,
                    message = "帳號或密碼錯誤"
                });
            }

            // 產生 JWT Token (這邊不需要公司或體驗者名稱)
            var jwtUtil = new JwtAuthUtil();
            string token = jwtUtil.GenerateToken(user.Id, user.Account, "Admin");

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

        // POST: api/v1/admin/logout 管理員登出
        [HttpPost]
        [Route("admin/logout")]
        [JwtAuthFilter] // 必須登入才能登出
        public IHttpActionResult PostAdminLogout()
        {
            try
            {
                // 1. 取出 token
                var token = Request.Headers.Authorization?.Parameter;
                if (string.IsNullOrEmpty(token))
                    return BadRequest("未找到 token");

                // 2. 從 JWT payload 解析 userId 與過期時間
                var jwtUtil = new JwtAuthUtil();
                Dictionary<string, object> payload;
                try
                {
                    payload = jwtUtil.GetPayload(token);
                }
                catch
                {
                    return BadRequest("Token 無效或已過期");
                }

                if (!payload.ContainsKey("Id") || !payload.ContainsKey("Exp"))
                    return BadRequest("Token 資訊不完整");

                int userId = Convert.ToInt32(payload["Id"]);
                DateTime expiredAt = Convert.ToDateTime(payload["Exp"]);

                // 3. 存進黑名單
                using (var db = new TryBetaDbContext())
                {
                    db.TokenBlacklistes.Add(new TokenBlacklist
                    {
                        Token = token,
                        UserId = userId,
                        ExpiredAt = expiredAt,
                        CreatedAt = DateTime.UtcNow
                    });
                    db.SaveChanges();
                }

                return Ok(new { status = 200, message = "登出成功" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/Auth/5
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