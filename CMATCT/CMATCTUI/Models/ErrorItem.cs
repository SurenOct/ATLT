using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMATCTUI.Models
{
    public class ErrorItem
    {
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public DateTime RetentionTime { get; set; }
        public string SequenceName { get; set; }
        public string Instrument { get; set; }
        public string Summary { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
    }
}
