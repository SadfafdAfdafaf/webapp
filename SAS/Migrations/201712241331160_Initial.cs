namespace SAS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Codes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessCode = c.String(),
                        Timeofrelease = c.DateTime(),
                        owner = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessToken = c.String(),
                        RefreshToken = c.String(),
                        TimeofreleaseAccessToken = c.DateTime(),
                        TimeofreleaseRefreshToken = c.DateTime(),
                        UserId = c.Int(nullable: false),
                        AccessCodeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Codes", t => t.AccessCodeId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.AccessCodeId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        UserPass = c.String(),
                        UserRole = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tokens", "UserId", "dbo.Users");
            DropForeignKey("dbo.Tokens", "AccessCodeId", "dbo.Codes");
            DropIndex("dbo.Tokens", new[] { "AccessCodeId" });
            DropIndex("dbo.Tokens", new[] { "UserId" });
            DropTable("dbo.Users");
            DropTable("dbo.Tokens");
            DropTable("dbo.Codes");
        }
    }
}
