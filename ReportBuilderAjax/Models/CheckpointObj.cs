using System.ComponentModel.DataAnnotations;

namespace ReportBuilderAjax.Web.Models
{
    public class CheckpointObj
    {
        [Key]
        public string CheckpointID { get; set; }

        public string CheckpointDescription { get; set; }
    }
}