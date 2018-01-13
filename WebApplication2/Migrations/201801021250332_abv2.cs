namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class abv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.instatmes", "Np", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.instatmes", "Np");
        }
    }
}
