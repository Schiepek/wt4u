namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isActive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "isActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "isActive");
        }
    }
}
