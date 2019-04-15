using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submissions
    {
        public uint AId { get; set; }
        public string Student { get; set; }
        public DateTime Time { get; set; }
        public uint? Score { get; set; }
        public string Contents { get; set; }

        public virtual Assignments A { get; set; }
        public virtual Students StudentNavigation { get; set; }
    }
}
