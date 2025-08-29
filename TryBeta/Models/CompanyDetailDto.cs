using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TryBeta.Models
{
    public class CompanyDetailDto
    {
        // 公司基本資訊
        public int Id { get; set; }
        public string Name { get; set; }
        public string Intro { get; set; }
        public string Industry { get; set; }  // 可從 Industry 表抓名字
        public string Website { get; set; }
        public string Address { get; set; }
        public string EmployeeNum { get; set; }

        // 聯絡人
        public string ContactName { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        // 公司照片
        public string CoverImage { get; set; }
        public List<string> EnvironmentImages { get; set; } = new List<string>();

        // 體驗計畫
        public List<ProgramPlanDto> ProgramPlans { get; set; } = new List<ProgramPlanDto>();

        // 評價
        public List<ParticipantEvaluationsDto> Evaluations { get; set; } = new List<ParticipantEvaluationsDto>();
    }

    public class ProgramPlansDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Intro { get; set; }
        public string Address { get; set; }
        public int MinPeople { get; set; }
        public int MaxPeople { get; set; }
        public int FavoritesCount { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }

    public class ParticipantEvaluationsDto
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public int ProgramPlanId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; }

    }
}