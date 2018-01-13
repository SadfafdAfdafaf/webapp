namespace Stat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.stats", "state", c => c.Guid(nullable: false));
            CreateIndex("dbo.stats", "state", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.stats", new[] { "state" });
            DropColumn("dbo.stats", "state");
        }
    }
}
