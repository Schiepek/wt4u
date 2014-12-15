using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wt4u.Models {

    public class ProjectBookingTime {

        public ProjectBookingTime() { }

        public ProjectBookingTime(int ProjectId, string Description, DateTime Start, DateTime? End, int WorkingSessionId )
        {
            this.ProjectId = ProjectId;
            this.Description = Description;
            this.Start = Start;
            this.End = End;
            this.WorkingSessionId = WorkingSessionId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? End { get; set; }
        public string Description { get; set; }
        [ForeignKey("WorkingSession")]
        public int WorkingSessionId { get; set; }
        public virtual WorkingSession WorkingSession { get; set; }
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}