namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustfeildProramPlanFavoriteTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Favorites", "CanceledAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Favorites", "CanceledAt", c => c.DateTime(nullable: false));
        }
    }
}
