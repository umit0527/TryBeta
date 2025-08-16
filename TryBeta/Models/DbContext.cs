using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Globalization;


namespace TryBeta.Models
{
    public class TryBetaDbContext : DbContext
    {
        public TryBetaDbContext() : base("TryBeta") // 對應 web.config 的 connection string 名稱
        {
            //this.Configuration.LazyLoadingEnabled = false; // EF6 關閉 Lazy Loading
        }

        public DbSet<CompanyInfoes> Companyinfoes { get; set; }  //企業基本資料表
        public DbSet<Users> Users { get; set; }  //帳號密碼表(三個端)
        public DbSet<UserStatus> UserStatuses { get; set; } //帳號密碼狀態表
        public DbSet<CompanyScales> CompanyScales { get; set; }  //企業規模表
        public DbSet<CompanyContacts> CompanyContact { get; set; }  //企業聯絡人表
        public DbSet<CompanyImages> CompanyImages { get; set; }  //企業照片表(logo/cover/環境)
        public DbSet<Identity> Identity { get; set; }  //體驗者身分表
        public DbSet<ExistingResume> ExistingResume { get; set; }  //(上傳)現有履歷表 
        public DbSet<ResumeEducation> ResumeEducation { get; set; }  //學歷表 (簡單履歷表)
        public DbSet<ResumeExperience> ResumeExperience { get; set; }  //工作經歷表 (簡單履歷表)
        public DbSet<PortfolioFiles> PortfolioFiles { get; set; }  //作品(上傳)表 (簡單履歷表)
        public DbSet<PortfolioLinks> PortfolioLinks { get; set; }  //作品連結表 (簡單履歷表)
        public DbSet<ResumeSkill> ResumeSkill { get; set; }  //技能表 (簡單履歷表)
        public DbSet<SimpleResume> SimpleResume { get; set; }  //簡單履歷表
        public DbSet<ParticipantInfoes> ParticipantInfoes { get; set; }  //體驗者基本資料表
        public DbSet<Industry> Industries { get; set; }  //產業表
        public DbSet<Position> Positions { get; set; }  //職務表
        public DbSet<City> City { get; set; }  //城市表
        public DbSet<District> Districts { get; set; }  //鄉鎮表
        public DbSet<Plan> Plan { get; set; } //方案表
        public DbSet<CompanyPlanOrder> CompanyPlanOrders { get; set; } //企業方案訂單表
        public DbSet<PlanUsage> PlanUsage { get; set; } //企業目前方案表
        public DbSet<PlanUsageStatus> PlanUsageStatuses { get; set; } //企業目前方案狀態表
        public DbSet<ProgramRegistration> ProgramRegistrations { get; set; } //體驗計畫註冊表
        public DbSet<ProgramPlan> ProgramPlan { get; set; } //體驗計畫表
        public DbSet<ProgramStep> ProgramStep { get; set; } //體驗計畫階段表
        public DbSet<ProgramPlanStatus> ProgramPlanStatuses { get; set; } //體驗計畫狀態表









        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 設定 email 欄位唯一
            modelBuilder.Entity<Users>()
                .Property(u => u.Email)
                .HasMaxLength(100)
                .HasColumnAnnotation(
                    "Index",
                    new IndexAnnotation(
                        new IndexAttribute("IX_Unique_Email") { IsUnique = true }));

            // ParticipantInfoes -> Users 外鍵，禁止 Cascade Delete
            modelBuilder.Entity<ParticipantInfoes>()
                .HasRequired(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .WillCascadeOnDelete(false);

            //企業購買的方案訂單表
            modelBuilder.Entity<Plan>()
        .Property(p => p.Price)
        .HasPrecision(18, 2);


            base.OnModelCreating(modelBuilder);
        }


    }
}