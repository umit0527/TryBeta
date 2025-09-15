namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addScoreFeildPopularParams : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlans", "ViewsCount", c => c.Int(nullable: false));
            AddColumn("dbo.ProgramPlans", "FavoritesCount", c => c.Int(nullable: false));
            AddColumn("dbo.ProgramPlans", "AppliedCount", c => c.Int(nullable: false));
            AlterColumn("dbo.CompanyInfoes", "Name", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CompanyInfoes", "Name", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.ProgramPlans", "AppliedCount");
            DropColumn("dbo.ProgramPlans", "FavoritesCount");
            DropColumn("dbo.ProgramPlans", "ViewsCount");
        }
    }
}
