using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMATCTUI.Models
{
    public class AuditLogItem
    {
        public string ID { get; set; }
        public string Category { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public double? RetentionTime { get; set; }
        //public DateTime? RetentionTime { get; set; }
        public string Device { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string InstrumentName { get; set; }
        //public string SequenceName { get; set; }
        //public string Instrument { get; set; }
        //public string Summary { get; set; }
        //public string Severity { get; set; }

    }
}
