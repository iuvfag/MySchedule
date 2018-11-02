using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySchedule
{
    class RegistHistoryDTO
    {
        public String user_id { get; set; }
        public String updateType { get; set; }
        public DateTime updateStartTime { get; set; }
        public DateTime updateEndingTime { get; set; }
        public String subject { get; set; }
        public String detail { get; set; }
    }
}
