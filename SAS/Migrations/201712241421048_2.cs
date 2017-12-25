namespace SAS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Owners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClienSecret = c.Int(nullable: false),
                        RedirectUri = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Codes", "ownerId", c => c.Int(nullable: false));
            CreateIndex("dbo.Codes", "ownerId");
            AddForeignKey("dbo.Codes", "ownerId", "dbo.Owners", "Id", cascadeDelete: true);
            DropColumn("dbo.Codes", "owner");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Codes", "owner", c => c.Int(nullable: false));
            DropForeignKey("dbo.Codes", "ownerId", "dbo.Owners");
            DropIndex("dbo.Codes", new[] { "ownerId" });
            DropColumn("dbo.Codes", "ownerId");
            DropTable("dbo.Owners");
        }
    }
}
