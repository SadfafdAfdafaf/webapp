namespace Stat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.stats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        server_name = c.Int(nullable: false),
                        request_type = c.Int(nullable: false),
                        detail = c.String(),
                        Time = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.stats");
        }
    }
}
