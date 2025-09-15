namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TryBeta.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TryBeta.Models.TryBetaDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TryBeta.Models.TryBetaDbContext context)
        {
            //企業規模/人數
            context.CompanyScales.AddOrUpdate(
            cs => cs.EmployeeNum,
     new CompanyScales { EmployeeNum = "1-10人" },
     new CompanyScales { EmployeeNum = "11-30人" },
     new CompanyScales { EmployeeNum = "31-50人" },
     new CompanyScales { EmployeeNum = "50-100人" },
     new CompanyScales { EmployeeNum = "101-200人" },
     new CompanyScales { EmployeeNum = "201-500人" }
 );

            //產業類別
            context.Industries.AddOrUpdate(
        i => i.Title,
        new Industry { Title = "行銷/傳播" },
        new Industry { Title = "設計/創意" },
        new Industry { Title = "資訊科技" },
        new Industry { Title = "金融/保險" },
        new Industry { Title = "醫療/健康" },
        new Industry { Title = "教育/學習" },
        new Industry { Title = "藝術/文化" },
        new Industry { Title = "媒體/出版" },
        new Industry { Title = "餐飲/烘焙" },
        new Industry { Title = "旅遊/休閒" },
        new Industry { Title = "永續/綠能" },
        new Industry { Title = "農業/畜牧" },
        new Industry { Title = "水產/海洋" },
        new Industry { Title = "製造業" },
        new Industry { Title = "建築/營建" },
        new Industry { Title = "時尚/生活" },
        new Industry { Title = "物流/運輸" },
        new Industry { Title = "商業/管理" },
        new Industry { Title = "保全/安全" },
        new Industry { Title = "政府/公共服務" }
    );

            //職務類別
            context.Positions.AddOrUpdate(
        i => i.Title,
        new Position { Title = "行銷專員" },
        new Position { Title = "設計師" },
        new Position { Title = "軟體工程師" },
        new Position { Title = "數據分析師" },
        new Position { Title = "業務" },
        new Position { Title = "客服專員" },
        new Position { Title = "產品經理" },
        new Position { Title = "財務會計" },
        new Position { Title = "人力資源專員" },
        new Position { Title = "法務專員" },
        new Position { Title = "教育訓練師" },
        new Position { Title = "研究員" },
        new Position { Title = "物流專員" },
        new Position { Title = "生產操作員" },
        new Position { Title = "建築師" },
        new Position { Title = "醫師" },
        new Position { Title = "廚師" },
        new Position { Title = "編輯" },
        new Position { Title = "公務員" });

            //身分類別
            context.Identity.AddOrUpdate(
        i => i.Title,
        new Identity { Title = "學生" },
        new Identity { Title = "上班族（全職）" },
        new Identity { Title = "兼職工作者" },
        new Identity { Title = "自由工作者" },
        new Identity { Title = "家管" },
        new Identity { Title = "創業/自營業者" },
        new Identity { Title = "退休人士" },
        new Identity { Title = "實習生" },
        new Identity { Title = "軍人" },
        new Identity { Title = "公務員" },
        new Identity { Title = "教師 / 教職人員" },
        new Identity { Title = "醫療人員" },
        new Identity { Title = "藝術工作者" },
        new Identity { Title = "科研人員/學者" },
        new Identity { Title = "非營利/志工/社會工作者" },
        new Identity { Title = "勞工/技術工人" },
        new Identity { Title = "待業中" },
        new Identity { Title = "其他" }
        );

            //城市表
            context.City.AddOrUpdate(
        i => i.Name,
        new City { Name = "基隆市" },
        new City { Name = "台北市" },
        new City { Name = "新北市" },
        new City { Name = "桃園市" },
        new City { Name = "新竹縣" },
        new City { Name = "新竹市" },
        new City { Name = "苗栗縣" },
        new City { Name = "台中市" },
        new City { Name = "彰化縣" },
        new City { Name = "南投縣" },
        new City { Name = "雲林縣" },
        new City { Name = "嘉義縣" },
        new City { Name = "嘉義市" },
        new City { Name = "台南市" },
        new City { Name = "高雄市" },
        new City { Name = "屏東縣" },
        new City { Name = "台東縣" },
        new City { Name = "花蓮縣" },
        new City { Name = "宜蘭縣" },
        new City { Name = "連江縣" },
        new City { Name = "金門縣" },
        new City { Name = "澎湖縣" }
        );
            context.SaveChanges(); // 確保資料確實存入、才能拿到 Id

            //鄉鎮表
            //基隆市
            var keelung = context.City.FirstOrDefault(c => c.Name == "基隆市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "仁愛區", CityId = keelung.Id },
                new District { Name = "信義區", CityId = keelung.Id },
                new District { Name = "中正區", CityId = keelung.Id },
                new District { Name = "中山區", CityId = keelung.Id },
                new District { Name = "安樂區", CityId = keelung.Id },
                new District { Name = "暖暖區", CityId = keelung.Id },
                new District { Name = "七堵區", CityId = keelung.Id }
            );

            //台北市
            var taipei = context.City.FirstOrDefault(c => c.Name == "台北市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "中正區", CityId = taipei.Id },
    new District { Name = "大同區", CityId = taipei.Id },
    new District { Name = "中山區", CityId = taipei.Id },
    new District { Name = "松山區", CityId = taipei.Id },
    new District { Name = "大安區", CityId = taipei.Id },
    new District { Name = "萬華區", CityId = taipei.Id },
    new District { Name = "信義區", CityId = taipei.Id },
    new District { Name = "士林區", CityId = taipei.Id },
    new District { Name = "北投區", CityId = taipei.Id },
    new District { Name = "內湖區", CityId = taipei.Id },
    new District { Name = "南港區", CityId = taipei.Id },
    new District { Name = "文山區", CityId = taipei.Id }
                );

            //新北市
            var newTaipei = context.City.FirstOrDefault(c => c.Name == "新北市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "板橋區", CityId = newTaipei.Id },
                new District { Name = "三重區", CityId = newTaipei.Id },
                new District { Name = "中和區", CityId = newTaipei.Id },
                new District { Name = "永和區", CityId = newTaipei.Id },
                new District { Name = "新莊區", CityId = newTaipei.Id },
                new District { Name = "新店區", CityId = newTaipei.Id },
                new District { Name = "樹林區", CityId = newTaipei.Id },
                new District { Name = "鶯歌區", CityId = newTaipei.Id },
                new District { Name = "三峽區", CityId = newTaipei.Id },
                new District { Name = "淡水區", CityId = newTaipei.Id },
                new District { Name = "汐止區", CityId = newTaipei.Id },
                new District { Name = "瑞芳區", CityId = newTaipei.Id },
                new District { Name = "土城區", CityId = newTaipei.Id },
                new District { Name = "蘆洲區", CityId = newTaipei.Id },
                new District { Name = "五股區", CityId = newTaipei.Id },
                new District { Name = "八里區", CityId = newTaipei.Id },
                new District { Name = "林口區", CityId = newTaipei.Id },
                new District { Name = "深坑區", CityId = newTaipei.Id },
                new District { Name = "石碇區", CityId = newTaipei.Id },
                new District { Name = "坪林區", CityId = newTaipei.Id },
                new District { Name = "三芝區", CityId = newTaipei.Id },
                new District { Name = "石門區", CityId = newTaipei.Id },
                new District { Name = "雙溪區", CityId = newTaipei.Id },
                new District { Name = "貢寮區", CityId = newTaipei.Id },
                new District { Name = "金山區", CityId = newTaipei.Id },
                new District { Name = "萬里區", CityId = newTaipei.Id },
                new District { Name = "烏來區", CityId = newTaipei.Id },
                new District { Name = "平溪區", CityId = newTaipei.Id }
            );

            //桃園市
            var taoyuan = context.City.FirstOrDefault(c => c.Name == "桃園市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "桃園區", CityId = taoyuan.Id },
                new District { Name = "中壢區", CityId = taoyuan.Id },
                new District { Name = "平鎮區", CityId = taoyuan.Id },
                new District { Name = "八德區", CityId = taoyuan.Id },
                new District { Name = "楊梅區", CityId = taoyuan.Id },
                new District { Name = "蘆竹區", CityId = taoyuan.Id },
                new District { Name = "大園區", CityId = taoyuan.Id },
                new District { Name = "龜山區", CityId = taoyuan.Id },
                new District { Name = "龍潭區", CityId = taoyuan.Id },
                new District { Name = "新屋區", CityId = taoyuan.Id },
                new District { Name = "觀音區", CityId = taoyuan.Id },
                new District { Name = "復興區", CityId = taoyuan.Id }
            );

            //新竹縣
            var hsinchuCounty = context.City.FirstOrDefault(c => c.Name == "新竹縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "竹北市", CityId = hsinchuCounty.Id },
                new District { Name = "竹東鎮", CityId = hsinchuCounty.Id },
                new District { Name = "新埔鎮", CityId = hsinchuCounty.Id },
                new District { Name = "湖口鄉", CityId = hsinchuCounty.Id },
                new District { Name = "新豐鄉", CityId = hsinchuCounty.Id },
                new District { Name = "芎林鄉", CityId = hsinchuCounty.Id },
                new District { Name = "橫山鄉", CityId = hsinchuCounty.Id },
                new District { Name = "北埔鄉", CityId = hsinchuCounty.Id },
                new District { Name = "寶山鄉", CityId = hsinchuCounty.Id },
                new District { Name = "峨眉鄉", CityId = hsinchuCounty.Id },
                new District { Name = "尖石鄉", CityId = hsinchuCounty.Id },
                new District { Name = "五峰鄉", CityId = hsinchuCounty.Id }
            );

            //新竹市
            var hsinchuCity = context.City.FirstOrDefault(c => c.Name == "新竹市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "東區", CityId = hsinchuCity.Id },
                new District { Name = "北區", CityId = hsinchuCity.Id },
                new District { Name = "香山區", CityId = hsinchuCity.Id }
            );

            //苗栗縣
            var miaoli = context.City.FirstOrDefault(c => c.Name == "苗栗縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "苗栗市", CityId = miaoli.Id },
                new District { Name = "頭份市", CityId = miaoli.Id },
                new District { Name = "竹南鎮", CityId = miaoli.Id },
                new District { Name = "卓蘭鎮", CityId = miaoli.Id },
                new District { Name = "獅潭鄉", CityId = miaoli.Id },
                new District { Name = "後龍鎮", CityId = miaoli.Id },
                new District { Name = "通霄鎮", CityId = miaoli.Id },
                new District { Name = "苑裡鎮", CityId = miaoli.Id },
                new District { Name = "大湖鄉", CityId = miaoli.Id },
                new District { Name = "公館鄉", CityId = miaoli.Id },
                new District { Name = "銅鑼鄉", CityId = miaoli.Id },
                new District { Name = "南庄鄉", CityId = miaoli.Id },
                new District { Name = "頭屋鄉", CityId = miaoli.Id },
                new District { Name = "三灣鄉", CityId = miaoli.Id },
                new District { Name = "西湖鄉", CityId = miaoli.Id },
                new District { Name = "三義鄉", CityId = miaoli.Id }
            );

            //台中市
            var taichung = context.City.FirstOrDefault(c => c.Name == "台中市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "中區", CityId = taichung.Id },
                new District { Name = "東區", CityId = taichung.Id },
                new District { Name = "南區", CityId = taichung.Id },
                new District { Name = "西區", CityId = taichung.Id },
                new District { Name = "北區", CityId = taichung.Id },
                new District { Name = "西屯區", CityId = taichung.Id },
                new District { Name = "南屯區", CityId = taichung.Id },
                new District { Name = "北屯區", CityId = taichung.Id },
                new District { Name = "豐原區", CityId = taichung.Id },
                new District { Name = "后里區", CityId = taichung.Id },
                new District { Name = "石岡區", CityId = taichung.Id },
                new District { Name = "東勢區", CityId = taichung.Id },
                new District { Name = "和平區", CityId = taichung.Id },
                new District { Name = "新社區", CityId = taichung.Id },
                new District { Name = "潭子區", CityId = taichung.Id },
                new District { Name = "大雅區", CityId = taichung.Id },
                new District { Name = "神岡區", CityId = taichung.Id },
                new District { Name = "大肚區", CityId = taichung.Id },
                new District { Name = "沙鹿區", CityId = taichung.Id },
                new District { Name = "龍井區", CityId = taichung.Id },
                new District { Name = "梧棲區", CityId = taichung.Id },
                new District { Name = "清水區", CityId = taichung.Id },
                new District { Name = "大甲區", CityId = taichung.Id },
                new District { Name = "外埔區", CityId = taichung.Id },
                new District { Name = "大安區", CityId = taichung.Id },
                new District { Name = "霧峰區", CityId = taichung.Id },
                new District { Name = "烏日區", CityId = taichung.Id }
            );

            //彰化縣
            var changhua = context.City.FirstOrDefault(c => c.Name == "彰化縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "彰化市", CityId = changhua.Id },
                new District { Name = "員林市", CityId = changhua.Id },
                new District { Name = "和美鎮", CityId = changhua.Id },
                new District { Name = "鹿港鎮", CityId = changhua.Id },
                new District { Name = "田中鎮", CityId = changhua.Id },
                new District { Name = "北斗鎮", CityId = changhua.Id },
                new District { Name = "二林鎮", CityId = changhua.Id },
                new District { Name = "溪湖鎮", CityId = changhua.Id },
                new District { Name = "竹塘鄉", CityId = changhua.Id },
                new District { Name = "芬園鄉", CityId = changhua.Id },
                new District { Name = "花壇鄉", CityId = changhua.Id },
                new District { Name = "福興鄉", CityId = changhua.Id },
                new District { Name = "線西鄉", CityId = changhua.Id },
                new District { Name = "大村鄉", CityId = changhua.Id },
                new District { Name = "埔鹽鄉", CityId = changhua.Id },
                new District { Name = "埔心鄉", CityId = changhua.Id },
                new District { Name = "秀水鄉", CityId = changhua.Id },
                new District { Name = "田尾鄉", CityId = changhua.Id },
                new District { Name = "二水鄉", CityId = changhua.Id },
                new District { Name = "伸港鄉", CityId = changhua.Id },
                new District { Name = "社頭鄉", CityId = changhua.Id }
            );

            //南投縣
            var nantou = context.City.FirstOrDefault(c => c.Name == "南投縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "南投市", CityId = nantou.Id },
                new District { Name = "草屯鎮", CityId = nantou.Id },
                new District { Name = "竹山鎮", CityId = nantou.Id },
                new District { Name = "埔里鎮", CityId = nantou.Id },
                new District { Name = "名間鄉", CityId = nantou.Id },
                new District { Name = "集集鎮", CityId = nantou.Id },
                new District { Name = "水里鄉", CityId = nantou.Id },
                new District { Name = "魚池鄉", CityId = nantou.Id },
                new District { Name = "信義鄉", CityId = nantou.Id },
                new District { Name = "仁愛鄉", CityId = nantou.Id },
                new District { Name = "國姓鄉", CityId = nantou.Id },
                new District { Name = "中寮鄉", CityId = nantou.Id },
                new District { Name = "鹿谷鄉", CityId = nantou.Id }
            );

            //雲林縣
            var yunlin = context.City.FirstOrDefault(c => c.Name == "雲林縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "斗六市", CityId = yunlin.Id },
                new District { Name = "斗南鎮", CityId = yunlin.Id },
                new District { Name = "虎尾鎮", CityId = yunlin.Id },
                new District { Name = "西螺鎮", CityId = yunlin.Id },
                new District { Name = "土庫鎮", CityId = yunlin.Id },
                new District { Name = "北港鎮", CityId = yunlin.Id },
                new District { Name = "古坑鄉", CityId = yunlin.Id },
                new District { Name = "大埤鄉", CityId = yunlin.Id },
                new District { Name = "莿桐鄉", CityId = yunlin.Id },
                new District { Name = "林內鄉", CityId = yunlin.Id },
                new District { Name = "二崙鄉", CityId = yunlin.Id },
                new District { Name = "崙背鄉", CityId = yunlin.Id },
                new District { Name = "麥寮鄉", CityId = yunlin.Id },
                new District { Name = "東勢鄉", CityId = yunlin.Id },
                new District { Name = "褒忠鄉", CityId = yunlin.Id },
                new District { Name = "台西鄉", CityId = yunlin.Id },
                new District { Name = "元長鄉", CityId = yunlin.Id },
                new District { Name = "四湖鄉", CityId = yunlin.Id },
                new District { Name = "水林鄉", CityId = yunlin.Id },
                new District { Name = "口湖鄉", CityId = yunlin.Id }
            );

            //嘉義縣
            var chiayiCounty = context.City.FirstOrDefault(c => c.Name == "嘉義縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "太保市", CityId = chiayiCounty.Id },
                new District { Name = "朴子市", CityId = chiayiCounty.Id },
                new District { Name = "布袋鎮", CityId = chiayiCounty.Id },
                new District { Name = "民雄鄉", CityId = chiayiCounty.Id },
                new District { Name = "溪口鄉", CityId = chiayiCounty.Id },
                new District { Name = "新港鄉", CityId = chiayiCounty.Id },
                new District { Name = "六腳鄉", CityId = chiayiCounty.Id },
                new District { Name = "東石鄉", CityId = chiayiCounty.Id },
                new District { Name = "義竹鄉", CityId = chiayiCounty.Id },
                new District { Name = "鹿草鄉", CityId = chiayiCounty.Id },
                new District { Name = "水上鄉", CityId = chiayiCounty.Id },
                new District { Name = "中埔鄉", CityId = chiayiCounty.Id },
                new District { Name = "大林鎮", CityId = chiayiCounty.Id },
                new District { Name = "梅山鄉", CityId = chiayiCounty.Id },
                new District { Name = "竹崎鄉", CityId = chiayiCounty.Id },
                new District { Name = "阿里山鄉", CityId = chiayiCounty.Id },
                new District { Name = "番路鄉", CityId = chiayiCounty.Id },
                new District { Name = "大埔鄉", CityId = chiayiCounty.Id }
            );

            //嘉義市
            var chiayiCity = context.City.FirstOrDefault(c => c.Name == "嘉義市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "東區", CityId = chiayiCity.Id },
                new District { Name = "西區", CityId = chiayiCity.Id }
            );

            //台南市
            var tainan = context.City.FirstOrDefault(c => c.Name == "台南市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "中西區", CityId = tainan.Id },
                new District { Name = "東區", CityId = tainan.Id },
                new District { Name = "南區", CityId = tainan.Id },
                new District { Name = "北區", CityId = tainan.Id },
                new District { Name = "安平區", CityId = tainan.Id },
                new District { Name = "安南區", CityId = tainan.Id },
                new District { Name = "永康區", CityId = tainan.Id },
                new District { Name = "歸仁區", CityId = tainan.Id },
                new District { Name = "新化區", CityId = tainan.Id },
                new District { Name = "左鎮區", CityId = tainan.Id },
                new District { Name = "玉井區", CityId = tainan.Id },
                new District { Name = "楠西區", CityId = tainan.Id },
                new District { Name = "南化區", CityId = tainan.Id },
                new District { Name = "仁德區", CityId = tainan.Id },
                new District { Name = "關廟區", CityId = tainan.Id },
                new District { Name = "龍崎區", CityId = tainan.Id },
                new District { Name = "官田區", CityId = tainan.Id },
                new District { Name = "麻豆區", CityId = tainan.Id },
                new District { Name = "佳里區", CityId = tainan.Id },
                new District { Name = "西港區", CityId = tainan.Id },
                new District { Name = "七股區", CityId = tainan.Id },
                new District { Name = "將軍區", CityId = tainan.Id },
                new District { Name = "學甲區", CityId = tainan.Id },
                new District { Name = "北門區", CityId = tainan.Id },
                new District { Name = "新營區", CityId = tainan.Id },
                new District { Name = "後壁區", CityId = tainan.Id },
                new District { Name = "白河區", CityId = tainan.Id },
                new District { Name = "東山區", CityId = tainan.Id },
                new District { Name = "六甲區", CityId = tainan.Id },
                new District { Name = "下營區", CityId = tainan.Id },
                new District { Name = "柳營區", CityId = tainan.Id },
                new District { Name = "鹽水區", CityId = tainan.Id },
                new District { Name = "善化區", CityId = tainan.Id },
                new District { Name = "大內區", CityId = tainan.Id },
                new District { Name = "山上區", CityId = tainan.Id },
                new District { Name = "新市區", CityId = tainan.Id },
                new District { Name = "安定區", CityId = tainan.Id }
            );

            //高雄市
            var kaohsiung = context.City.FirstOrDefault(c => c.Name == "高雄市");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "鹽埕區", CityId = kaohsiung.Id },
                new District { Name = "鼓山區", CityId = kaohsiung.Id },
                new District { Name = "左營區", CityId = kaohsiung.Id },
                new District { Name = "楠梓區", CityId = kaohsiung.Id },
                new District { Name = "三民區", CityId = kaohsiung.Id },
                new District { Name = "新興區", CityId = kaohsiung.Id },
                new District { Name = "前金區", CityId = kaohsiung.Id },
                new District { Name = "苓雅區", CityId = kaohsiung.Id },
                new District { Name = "前鎮區", CityId = kaohsiung.Id },
                new District { Name = "旗津區", CityId = kaohsiung.Id },
                new District { Name = "小港區", CityId = kaohsiung.Id },
                new District { Name = "鳳山區", CityId = kaohsiung.Id },
                new District { Name = "大寮區", CityId = kaohsiung.Id },
                new District { Name = "林園區", CityId = kaohsiung.Id },
                new District { Name = "鳥松區", CityId = kaohsiung.Id },
                new District { Name = "大樹區", CityId = kaohsiung.Id },
                new District { Name = "旗山區", CityId = kaohsiung.Id },
                new District { Name = "美濃區", CityId = kaohsiung.Id },
                new District { Name = "六龜區", CityId = kaohsiung.Id },
                new District { Name = "內門區", CityId = kaohsiung.Id },
                new District { Name = "杉林區", CityId = kaohsiung.Id },
                new District { Name = "甲仙區", CityId = kaohsiung.Id },
                new District { Name = "桃源區", CityId = kaohsiung.Id },
                new District { Name = "那瑪夏區", CityId = kaohsiung.Id },
                new District { Name = "茂林區", CityId = kaohsiung.Id },
                new District { Name = "茄萣區", CityId = kaohsiung.Id },
                new District { Name = "燕巢區", CityId = kaohsiung.Id },
                new District { Name = "阿蓮區", CityId = kaohsiung.Id },
                new District { Name = "田寮區", CityId = kaohsiung.Id },
                new District { Name = "梓官區", CityId = kaohsiung.Id },
                new District { Name = "彌陀區", CityId = kaohsiung.Id },
                new District { Name = "永安區", CityId = kaohsiung.Id },
                new District { Name = "湖內區", CityId = kaohsiung.Id },
                new District { Name = "岡山區", CityId = kaohsiung.Id },
                new District { Name = "路竹區", CityId = kaohsiung.Id },
                new District { Name = "橋頭區", CityId = kaohsiung.Id }
            );

            //屏東縣
            var pingtung = context.City.FirstOrDefault(c => c.Name == "屏東縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "屏東市", CityId = pingtung.Id },
                new District { Name = "潮州鎮", CityId = pingtung.Id },
                new District { Name = "東港鎮", CityId = pingtung.Id },
                new District { Name = "萬丹鄉", CityId = pingtung.Id },
                new District { Name = "長治鄉", CityId = pingtung.Id },
                new District { Name = "麟洛鄉", CityId = pingtung.Id },
                new District { Name = "九如鄉", CityId = pingtung.Id },
                new District { Name = "里港鄉", CityId = pingtung.Id },
                new District { Name = "鹽埔鄉", CityId = pingtung.Id },
                new District { Name = "高樹鄉", CityId = pingtung.Id },
                new District { Name = "竹田鄉", CityId = pingtung.Id },
                new District { Name = "內埔鄉", CityId = pingtung.Id },
                new District { Name = "萬巒鄉", CityId = pingtung.Id },
                new District { Name = "新埤鄉", CityId = pingtung.Id },
                new District { Name = "枋寮鄉", CityId = pingtung.Id },
                new District { Name = "新園鄉", CityId = pingtung.Id },
                new District { Name = "崁頂鄉", CityId = pingtung.Id },
                new District { Name = "林邊鄉", CityId = pingtung.Id },
                new District { Name = "南州鄉", CityId = pingtung.Id },
                new District { Name = "枋山鄉", CityId = pingtung.Id },
                new District { Name = "春日鄉", CityId = pingtung.Id },
                new District { Name = "獅子鄉", CityId = pingtung.Id },
                new District { Name = "車城鄉", CityId = pingtung.Id },
                new District { Name = "牡丹鄉", CityId = pingtung.Id },
                new District { Name = "恆春鎮", CityId = pingtung.Id },
                new District { Name = "滿州鄉", CityId = pingtung.Id },
                new District { Name = "瑪家鄉", CityId = pingtung.Id },
                new District { Name = "霧台鄉", CityId = pingtung.Id },
                new District { Name = "三地門鄉", CityId = pingtung.Id },
                new District { Name = "佳冬鄉", CityId = pingtung.Id },
                new District { Name = "來義鄉", CityId = pingtung.Id },
                new District { Name = "泰武鄉", CityId = pingtung.Id }
            );

            //台東縣
            var taitung = context.City.FirstOrDefault(c => c.Name == "台東縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "台東市", CityId = taitung.Id },
                new District { Name = "成功鎮", CityId = taitung.Id },
                new District { Name = "卑南鄉", CityId = taitung.Id },
                new District { Name = "鹿野鄉", CityId = taitung.Id },
                new District { Name = "延平鄉", CityId = taitung.Id },
                new District { Name = "蘭嶼鄉", CityId = taitung.Id },
                new District { Name = "綠島鄉", CityId = taitung.Id },
                new District { Name = "太麻里鄉", CityId = taitung.Id },
                new District { Name = "金峰鄉", CityId = taitung.Id },
                new District { Name = "大武鄉", CityId = taitung.Id },
                new District { Name = "達仁鄉", CityId = taitung.Id },
                new District { Name = "海端鄉", CityId = taitung.Id }
            );

            //花蓮縣
            var hualien = context.City.FirstOrDefault(c => c.Name == "花蓮縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "花蓮市", CityId = hualien.Id },
                new District { Name = "鳳林鎮", CityId = hualien.Id },
                new District { Name = "玉里鎮", CityId = hualien.Id },
                new District { Name = "新城鄉", CityId = hualien.Id },
                new District { Name = "吉安鄉", CityId = hualien.Id },
                new District { Name = "壽豐鄉", CityId = hualien.Id },
                new District { Name = "光復鄉", CityId = hualien.Id },
                new District { Name = "豐濱鄉", CityId = hualien.Id },
                new District { Name = "瑞穗鄉", CityId = hualien.Id },
                new District { Name = "萬榮鄉", CityId = hualien.Id },
                new District { Name = "卓溪鄉", CityId = hualien.Id },
                new District { Name = "富里鄉", CityId = hualien.Id },
                new District { Name = "秀林鄉", CityId = hualien.Id }
            );

            //宜蘭縣
            var yilan = context.City.FirstOrDefault(c => c.Name == "宜蘭縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "宜蘭市", CityId = yilan.Id },
                new District { Name = "羅東鎮", CityId = yilan.Id },
                new District { Name = "蘇澳鎮", CityId = yilan.Id },
                new District { Name = "頭城鎮", CityId = yilan.Id },
                new District { Name = "壯圍鄉", CityId = yilan.Id },
                new District { Name = "員山鄉", CityId = yilan.Id },
                new District { Name = "冬山鄉", CityId = yilan.Id },
                new District { Name = "五結鄉", CityId = yilan.Id },
                new District { Name = "三星鄉", CityId = yilan.Id },
                new District { Name = "大同鄉", CityId = yilan.Id },
                new District { Name = "南澳鄉", CityId = yilan.Id },
                new District { Name = "礁溪鄉", CityId = yilan.Id }
            );

            //連江縣
            var lienchiang = context.City.FirstOrDefault(c => c.Name == "連江縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "南竿鄉", CityId = lienchiang.Id },
                new District { Name = "北竿鄉", CityId = lienchiang.Id },
                new District { Name = "東引鄉", CityId = lienchiang.Id },
                new District { Name = "莒光鄉", CityId = lienchiang.Id }
            );

            //金門縣
            var kinmen = context.City.FirstOrDefault(c => c.Name == "金門縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "金城鎮", CityId = kinmen.Id },
                new District { Name = "金沙鎮", CityId = kinmen.Id },
                new District { Name = "金湖鎮", CityId = kinmen.Id },
                new District { Name = "金寧鄉", CityId = kinmen.Id },
                new District { Name = "烈嶼鄉", CityId = kinmen.Id },
                new District { Name = "烏坵鄉", CityId = kinmen.Id }
            );

            //澎湖縣
            var penghu = context.City.FirstOrDefault(c => c.Name == "澎湖縣");

            context.Districts.AddOrUpdate(
                d => new { d.Name, d.CityId },
                new District { Name = "馬公市", CityId = penghu.Id },
                new District { Name = "湖西鄉", CityId = penghu.Id },
                new District { Name = "白沙鄉", CityId = penghu.Id },
                new District { Name = "西嶼鄉", CityId = penghu.Id },
                new District { Name = "望安鄉", CityId = penghu.Id },
                new District { Name = "七美鄉", CityId = penghu.Id }
            );


        }
    }
}
