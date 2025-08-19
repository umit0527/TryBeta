namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixProgramSubmit : DbMigration
    {
        public override void Up()
        {
            // 刪除舊的外鍵與欄位（如果還存在）
            DropForeignKey("dbo.ProgramSubmits", "ProgramSubmitStatus_Id", "dbo.ProgramSubmitStatus");
            DropIndex("dbo.ProgramSubmits", new[] { "ProgramSubmitStatus_Id" });
            DropColumn("dbo.ProgramSubmits", "ProgramSubmitStatus_Id");

            // 建立新的外鍵索引
            CreateIndex("dbo.ProgramSubmits", "StatusId");
            AddForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus");
            DropIndex("dbo.ProgramSubmits", new[] { "StatusId" });

            AddColumn("dbo.ProgramSubmits", "ProgramSubmitStatus_Id", c => c.Int());
            CreateIndex("dbo.ProgramSubmits", "ProgramSubmitStatus_Id");
            AddForeignKey("dbo.ProgramSubmits", "ProgramSubmitStatus_Id", "dbo.ProgramSubmitStatus", "Id");
        }
    }
}
