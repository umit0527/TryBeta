using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class GoogleMapsHelper
    {
        /// <summary>
        /// 生成 Google Maps Embed URL（Key 從後端設定讀取）
        /// </summary>
        public static string GenerateEmbedUrl(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            // 從設定檔讀取 Key
            string apiKey = ConfigurationManager.ConnectionStrings["GoogleMaps"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Google Maps API Key 未設定");

            string encodedAddress = HttpUtility.UrlEncode(address);
            return $"https://www.google.com/maps/embed/v1/place?key={apiKey}&q={encodedAddress}&language=zh-TW&region=TW";
        }
    }
}