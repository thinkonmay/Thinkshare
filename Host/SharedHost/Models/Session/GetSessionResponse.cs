using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHost.Models.ResponseModel
{
    class GetSessionResponse
    {
        public DayOfWeek DayofWeek { get; set; }

        public double SessionTime { get; set; }
    }
}
