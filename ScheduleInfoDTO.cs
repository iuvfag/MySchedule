using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySchedule
{
    class ScheduleInfoDTO
    {
        public int scheduleId { get; set; }
        public String userId { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endingTime { get; set; }
        public String subject { get; set; }
        public String detail { get; set; }
    }
}
