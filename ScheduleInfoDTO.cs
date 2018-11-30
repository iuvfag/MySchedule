using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySchedule
{
    class ScheduleInfoDTO
    {
        internal int scheduleId;
        internal List<int> scheduleIdList = new List<int>();
        internal String userId;
        internal DateTime startTime;
        internal DateTime endingTime;
        internal String subject;
        internal List<String> subjectList = new List<string>();
        internal String detail;
        internal String hashKey;
        internal int scheduleCount;
    }
}
