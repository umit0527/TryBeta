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
        }

        public DbSet<CompanyInfoes> Companyinfoes { get; set; }  //企業基本資料表
        public DbSet<Users> Users { get; set; }  //帳號密碼表(三個端)
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
        public DbSet<UserInfoes> UserInfoes { get; set; }  //體驗者基本資料表
        public DbSet<Industry> Industries { get; set; }  //產業表
        public DbSet<Position> Positions { get; set; }  //職務表
        public DbSet<City> City { get; set; }  //城市表
        public DbSet<District> Districts { get; set; }  //鄉鎮表


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

            // UserInfoes -> Users 外鍵，禁止 Cascade Delete
            modelBuilder.Entity<UserInfoes>()
                .HasRequired(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }


    }
}