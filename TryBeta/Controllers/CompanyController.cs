using Jose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.UI.WebControls;
using TryBeta.Models;
using static TryBeta.Models.CompanInfoDto;

namespace TryBeta.Controllers
{
    [RoutePrefix("api/v1/company")]
    public class CompanyController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        // GET: api/Company/ 取得登入企業的基本資料
        [HttpGet]
        [Route("")]
        [JwtAuthFilter]
        [ResponseType(typeof(CompanyInfoResponseDto))]
        public IHttpActionResult GetMyCompanyInfo()
        {
            // 1.從 JwtAuthFilter 取得 UserId
            if (!Request.Properties.TryGetValue("UserId", out var userIdObj))
                return Unauthorized();

            int userId = (int)userIdObj;

            // 2.從資料庫抓登入企業資料，Include 關聯表
            var companyEntity = db.Companyinfoes
                                  .Include(c => c.CompanyContacts)
                                  .Include(c => c.CompanyImages)
                                  .Include(c => c.User)
                                  .FirstOrDefault(c => c.UserId == userId);

            if (companyEntity == null)
                return NotFound();

            // 取得網站基底 URL 指向 GetImage API
            string baseUrl = $"{Request.RequestUri.Scheme}://{Request.RequestUri.Host}:{Request.RequestUri.Port}/api/v1/company/image/";

           // 3.組 DTO
           var dto = new CompanyInfoResponseDto
           {
               Name = companyEntity.Name,
               IndustryId = companyEntity.IndustryId,
               TaxIdNum = companyEntity.TaxIdNum,
               Address = companyEntity.Address,
               Website = companyEntity.Website,
               Intro = companyEntity.Intro,
               ScaleId = companyEntity.ScaleId,
               Account = companyEntity.User.Account,
               Email = companyEntity.User.Email,

               // 聯絡人
               CompanyContact = companyEntity.CompanyContacts == null ? null : new CompanyContactDto
               {
                   Name = companyEntity.CompanyContacts.Name,
                   JobTitle = companyEntity.CompanyContacts.JobTitle,
                   Email = companyEntity.CompanyContacts.Email,
                   Phone = companyEntity.CompanyContacts.Phone
               },

               // 所有圖片，透過 GetImage API 生成可用路徑
               CompanyImg = companyEntity.CompanyImages.Select(img => new CompanyImgDto
               {
                   Type = img.Type,
                   ImgPath = baseUrl + img.ImgPath.TrimStart('~', '/')
               }).ToList(),
           };

            // 4.回傳 DTO
            return Ok(dto);
        }

        // PUT: api/CompanyRegister/5
        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCompanyInfo(int id, CompanyInfoes companyInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != companyInfo.Id)
            {
                return BadRequest();
            }

            db.Entry(companyInfo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyInfoExists(id))
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

        // POST: api/Company 註冊 
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CompanyInfoes))]
        public async Task<IHttpActionResult> PostCompanyInfo()
        {
            // 讀取 JSON 資料 (dto)：從 HTTP content 取出文字。
            // 一樣是 I/ O 操作（stream → string 轉換），可能需要等待。
            var provider = new MultipartMemoryStreamProvider();
            // 讀取上傳檔案：把檔案讀成 byte[]。
            // 這是 磁碟/ 記憶體 I / O，大檔案時會很耗時。
            // 如果不用 await，整個 Thread 就會卡住等檔案讀完。
            await Request.Content.ReadAsMultipartAsync(provider);

            // provider.Contents：用來處理「multipart/form-data」的 HTTP POST 請求（表單 + 檔案一起送的格式）。
            // 找出一個特定叫 "dto" 的欄位。
            var dtoContent = provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "dto");
            if (dtoContent == null)
                return BadRequest("缺少公司資料");

            // 讀取欄位dto的 Json 資料：讀取後轉為string
            var jsonString = await dtoContent.ReadAsStringAsync();
            // 序列化
            var dto = JsonConvert.DeserializeObject<CompanyRegisterDto>(jsonString);
            
            var allErrors = new List<string>();

            // ModelState 驗證
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)  //把所有驗證錯誤抓出來
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)  //取出錯誤訊息
                                 ? e.Exception?.Message  //如果沒有才用 Exception.Message
                                 : e.ErrorMessage)  //先用 ErrorMessage
                    .Where(m => !string.IsNullOrWhiteSpace(m))  //過濾掉空字串
                    .ToList();  //轉成清單

                allErrors.AddRange(modelErrors);  //把錯誤存到自訂的 allErrors 集合裡，用來回傳給前端
            }

            // 唯一性檢查 (帳號、Email、公司名稱、統編)
            // 查詢 Users 表，是否有任何一筆資料的 Account 欄位 等於使用者填的帳號 (dto.Account)
            bool accountExists = db.Users.Any(u => u.Account == dto.Account);
            if (accountExists) //有的話是 true
            {
                allErrors.Add("該帳號已被使用");  // 把錯誤存到自訂的 allErrors 集合裡，用來回傳給前端
            }

            bool emailExists = db.Users.Any(u => u.Email == dto.Email);
            if (emailExists)
            {
                allErrors.Add("該 Email 已被使用");
            }

            // 在 CompanyInfo 表中檢查，企業名稱
            bool nameExists = db.Companyinfoes.Any(c => c.Name == dto.Name);
            if (nameExists)
            {
                allErrors.Add("該企業名稱已被使用");
            }

            //  統編
            if (!string.IsNullOrWhiteSpace(dto.TaxIdNum))
            {
                if (db.Companyinfoes.Any(c => c.TaxIdNum == dto.TaxIdNum))
                    allErrors.Add("該統編已被使用");
            }

            // 如果有任何錯誤就統一回傳
            if (allErrors.Any())
            {
                var content = new
                {
                    status = 400,
                    message = "註冊資料有誤",
                    errors = allErrors
                };
                return Content(HttpStatusCode.BadRequest, content);
            }

            var hashedPassword = PasswordHasher.HashPassword(dto.Password); // 將密碼(明碼)加鹽雜湊

            // 若帳號和email都沒有重複的值
            // 建立帳號，使用Transaction，當user、圖片與基本資料都建立成功，則建立帳號成功
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 建立 User
                    var user = new Users
                    {
                        Account = dto.Account,
                        Email = dto.Email,
                        PasswordHash = hashedPassword,
                        Role = "Company",
                        StatusId = 1,  //啟用
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Users.Add(user);
                    db.SaveChanges();

                    // 建立公司基本資料 + 聯絡人 (同時建立)
                    var company = new CompanyInfoes
                    {
                        Name = dto.Name,
                        IndustryId = dto.IndustryId,
                        TaxIdNum = dto.TaxIdNum,
                        Address = dto.Address,
                        Website = dto.Website,
                        Intro = dto.Intro,
                        ScaleId = dto.ScaleId,
                        UserId = user.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CompanyContacts = dto.CompanyContact == null ? null : new CompanyContacts
                        {
                            Name = dto.CompanyContact.Name,
                            JobTitle = dto.CompanyContact.JobTitle,
                            Email = dto.CompanyContact.Email,
                            Phone = dto.CompanyContact.Phone,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    };

                    db.Companyinfoes.Add(company);
                    db.SaveChanges();

                    // 上傳圖片
                    string baseRoot = HttpContext.Current.Server.MapPath("~/Images/Company");

                    foreach (var file in provider.Contents.Where(c => c.Headers.ContentDisposition.FileName != null))
                    {
                        var originalFileName = file.Headers.ContentDisposition.FileName.Trim('"');
                        var ext = Path.GetExtension(originalFileName).ToLower();
                        var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        if (!allowedExt.Contains(ext)) continue;

                        var bytes = await file.ReadAsByteArrayAsync();
                        if (bytes.Length > 5 * 1024 * 1024) continue;

                        // 依 type 分資料夾
                        string type = file.Headers.ContentDisposition.Name.Trim('"'); // logo / cover / environmentImgs[]

                        string subFolder;
                        switch (type.ToLower())
                        {
                            case "logo":
                                subFolder = "Logo";
                                break;
                            case "cover":
                                subFolder = "Cover";
                                break;
                            default:
                                // 不管是 environmentImgs 或 environmentImgs[]
                                if (type.ToLower().StartsWith("environmentimgs"))
                                    subFolder = "Environment";
                                else
                                    subFolder = "Other";
                                break;
                        }

                        string uploadRoot = Path.Combine(baseRoot, subFolder);
                        if (!Directory.Exists(uploadRoot))
                            Directory.CreateDirectory(uploadRoot);

                        var newFileName = Path.GetFileNameWithoutExtension(originalFileName)
                                          + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext;
                        var newFilePath = Path.Combine(uploadRoot, newFileName);
                        File.WriteAllBytes(newFilePath, bytes);

                        string relativePath = $"~/Images/Company/{subFolder}/{newFileName}";

                        // 存到 CompanyImages
                        db.CompanyImages.Add(new CompanyImages
                        {
                            CompanyId = company.Id,
                            Type = type,
                            ImgPath = relativePath,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                    db.SaveChanges();

                    transaction.Commit();

                    // 回傳成功訊息
                    return Content(HttpStatusCode.Created, new
                    {
                        status = 201,
                        message = "註冊成功",
                        company_id = company.Id
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }

        // DELETE: api/CompanyRegister/5
        [HttpDelete]
        [Route("{id:int}")]
        [ResponseType(typeof(CompanyInfoes))]
        public IHttpActionResult DeleteCompanyInfo(int id)
        {
            CompanyInfoes companyInfo = db.Companyinfoes.Find(id);
            if (companyInfo == null)
            {
                return NotFound();
            }

            db.Companyinfoes.Remove(companyInfo);
            db.SaveChanges();

            return Ok(companyInfo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private bool CompanyInfoExists(int id)
        {
            return db.Companyinfoes.Count(e => e.Id == id) > 0;
        }

        private IHttpActionResult Conflict(string message)
        {
            var content = new { status = 409, message = message };
            return Content(HttpStatusCode.Conflict, content);
        }


    }
}