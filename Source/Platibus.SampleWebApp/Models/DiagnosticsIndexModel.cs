using System.ComponentModel.DataAnnotations;

namespace Platibus.SampleWebApp.Models
{
    public class DiagnosticsIndexModel
    { 
        [Display(Name = "Requests")]
        public int Requests { get; set; }

        [Display(Name = "Min. Time (ms)")]
        public int MinTime { get; set; }

        [Display(Name = "Max. Time (ms)")]
        public int MaxTime { get; set; }

        [Display(Name = "% Acknowledged")]
        public double AcknowledgementRate { get; set; }

        [Display(Name = "% Reply")]
        public double ReplyRate { get; set; }

        [Display(Name = "% Error")]
        public double ErrorRate { get; set; }
    }
}