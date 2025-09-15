namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustGoogleIdLengthinUserTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "GoogleId", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "GoogleId", c => c.String(maxLength: 50));
        }
    }
}
