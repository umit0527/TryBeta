namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class fix2 : DbMigration
    {
        public override void Up()
        {
            // 先刪掉舊索引
        //DropIndex("dbo.ProgramSubmits", new[] { "ProgramSubmitStatus_Id" });

            // 將舊欄位改名
            //RenameColumn(table: "dbo.ProgramSubmits", name: "ProgramSubmitStatus_Id", newName: "StatusId");

            // 修改欄位為 NOT NULL
            AlterColumn("dbo.ProgramSubmits", "StatusId", c => c.Int(nullable: false));

            // 建立新索引
            //CreateIndex("dbo.ProgramSubmits", "StatusId");

            // 重新建立外鍵
            AddForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus", "Id", cascadeDelete: false);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus");
            DropIndex("dbo.ProgramSubmits", new[] { "StatusId" });
            AlterColumn("dbo.ProgramSubmits", "StatusId", c => c.Int());
            RenameColumn(table: "dbo.ProgramSubmits", name: "StatusId", newName: "ProgramSubmitStatus_Id");
            CreateIndex("dbo.ProgramSubmits", "ProgramSubmitStatus_Id");
        }
    }
}
