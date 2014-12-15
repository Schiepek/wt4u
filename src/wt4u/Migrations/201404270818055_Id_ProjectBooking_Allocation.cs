namespace wt4u.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Id_ProjectBooking_Allocation : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ProjectAllocations", new[] { "ProjectID" });
            DropIndex("dbo.ProjectBookingTimes", new[] { "WorkingSessionID" });
            DropIndex("dbo.ProjectBookingTimes", new[] { "ProjectID" });
            CreateIndex("dbo.ProjectAllocations", "ProjectId");
            CreateIndex("dbo.ProjectBookingTimes", "WorkingSessionId");
            CreateIndex("dbo.ProjectBookingTimes", "ProjectId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ProjectBookingTimes", new[] { "ProjectId" });
            DropIndex("dbo.ProjectBookingTimes", new[] { "WorkingSessionId" });
            DropIndex("dbo.ProjectAllocations", new[] { "ProjectId" });
            CreateIndex("dbo.ProjectBookingTimes", "ProjectID");
            CreateIndex("dbo.ProjectBookingTimes", "WorkingSessionID");
            CreateIndex("dbo.ProjectAllocations", "ProjectID");
        }
    }
}
