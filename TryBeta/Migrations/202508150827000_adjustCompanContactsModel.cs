namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustCompanContactsModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CompanyContacts", "CompanyId", "dbo.CompanyInfoes");
            DropPrimaryKey("dbo.CompanyContacts");
            AddPrimaryKey("dbo.CompanyContacts", "CompanyId");
            AddForeignKey("dbo.CompanyContacts", "CompanyId", "dbo.CompanyInfoes", "Id");
            DropColumn("dbo.CompanyContacts", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CompanyContacts", "Id", c => c.Int(nullable: false, identity: true));
            DropForeignKey("dbo.CompanyContacts", "CompanyId", "dbo.CompanyInfoes");
            DropPrimaryKey("dbo.CompanyContacts");
            AddPrimaryKey("dbo.CompanyContacts", "Id");
            AddForeignKey("dbo.CompanyContacts", "CompanyId", "dbo.CompanyInfoes", "Id", cascadeDelete: true);
        }
    }
}
