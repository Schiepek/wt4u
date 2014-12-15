using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wt4u.Models {


    public class Break {
        public Break() { }

        public Break(DateTime StartTime, DateTime? endTime, int WorkingSessionId)
        {
            this.Start = StartTime;
            this.End = endTime;
            this.WorkingSessionId = WorkingSessionId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BreakId { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? End { get; set; }
        [ForeignKey("WorkingSession")]
        public int WorkingSessionId { get; set; }
        public virtual WorkingSession WorkingSession { get; set; }
        int test { get; set; }
    }
}