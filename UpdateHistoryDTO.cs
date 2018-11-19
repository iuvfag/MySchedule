using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySchedule
{
    class UpdateHistoryDTO
    {
        public String userId { get; set; }
        public int scheduleId { get; set; }
        public String updateType { get; set; }
        public DateTime updateStartTime { get; set; }
        public DateTime updateEndingTime { get; set; }
        public String subject { get; set; }
        public String detail { get; set; }
        public DateTime updateTime { get; set; }
        public String hashKey { get; set; }
        public String previousHashKey { get; set; }
        public int nonce { get; set; }
    }
}
