using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReportBuilderAjax.Web
{
    public class UserReleaseNoteVersion
    {
        [Key]
        [ReadOnly(true)]
        public int ReleaseNoteID { get; set; }

        public DateTime ReleaseNoteDate { get; set; }

        public string ReleaseNoteNumber { get; set; }

        public string ReleaseNoteTitle { get; set; }

        public bool IsReleaseNoteRead { get; set; }

        //public bool IsReleaseNoteNotificationViewed { get; set; }        

        public string DocumentLink { get; set; }
        
    }
}