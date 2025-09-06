using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TryBeta.Models;

namespace TryBeta.Controllers
{

    public class ImageController : ApiController
    {
        private TryBetaDbContext db = new TryBetaDbContext();

        //GET: 讀取圖片
        [HttpGet]
        [Route("api/v1/programs/image/{*filePath}")]
        public HttpResponseMessage GetImage(string filePath)
        {

            // 先做 URL Decode
            filePath = Uri.UnescapeDataString(filePath ?? "");

            // MapPath
            string fullPath = HttpContext.Current.Server.MapPath("~/" + filePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("找不到檔案，實際路徑：" + fullPath)
                };
            }

            var contentType = MimeMapping.GetMimeMapping(fullPath);
            var bytes = System.IO.File.ReadAllBytes(fullPath);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            return result;
        }

        // POST: api/v1/uploads 上傳照片
        [HttpPost]
        [Route("~/api/v1/programs/{programId}/images")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> UploadImage(int programId)
        {
            try
            {
                // 1. 確認 ProgramPlan 存在
                var program = db.ProgramPlan.FirstOrDefault(p => p.Id == programId);
                if (program == null)
                    return NotFound();

                // 2. 確認有檔案
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest("Content type 必須是 multipart/form-data");

                // 3. 設定上傳路徑
                var uploadRoot = HttpContext.Current.Server.MapPath("~/Images");
                if (!Directory.Exists(uploadRoot))
                    Directory.CreateDirectory(uploadRoot);

                // 4. 讀取所有檔案
                var provider = new MultipartFormDataStreamProvider(uploadRoot);
                await Request.Content.ReadAsMultipartAsync(provider);

                if (!provider.FileData.Any())
                    return BadRequest("沒有收到檔案");

                var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                var results = new List<object>();
                var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                long maxSize = 5 * 1024 * 1024;

                foreach (var fileData in provider.FileData)
                {
                    var originalFileName = Path.GetFileName(fileData.Headers.ContentDisposition.FileName.Trim('"'));
                    var ext = Path.GetExtension(originalFileName).ToLower();

                    // 檔案格式驗證
                    if (!allowedExt.Contains(ext))
                    {
                        File.Delete(fileData.LocalFileName);
                        continue; // 跳過不合法的檔案
                    }

                    // 檔案大小驗證
                    var fileInfo = new FileInfo(fileData.LocalFileName);
                    if (fileInfo.Length > maxSize)
                    {
                        File.Delete(fileData.LocalFileName);
                        continue; // 跳過超過大小的檔案
                    }

                    // 產生唯一檔名並移動
                    var newFileName = Guid.NewGuid().ToString("N") + ext;
                    var newFilePath = Path.Combine(uploadRoot, newFileName);
                    File.Move(fileData.LocalFileName, newFilePath);

                    var fileUrl = $"{baseUrl}/Images/{newFileName}";

                    // 建立 DB 紀錄
                    var programImage = new ProgramPlanImage
                    {
                        ProgramPlanId = programId,
                        Url = fileUrl,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.ProgramPlanImages.Add(programImage);
                    db.SaveChanges();

                    results.Add(new
                    {
                        id = programImage.Id,
                        programplan_id = programId,
                        img_path = fileUrl,
                        created_at = programImage.CreatedAt
                    });
                }

                if (!results.Any())
                    return BadRequest("沒有符合格式或大小的檔案可以上傳");

                // 取得最新一筆 ProgramPlan 的 Id
                var latestProgramId = db.ProgramPlan.OrderByDescending(p => p.Id).Select(p => p.Id).FirstOrDefault();

                return Ok(new
                {
                    latest_programplan_id = latestProgramId,
                    uploaded_files = results
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
