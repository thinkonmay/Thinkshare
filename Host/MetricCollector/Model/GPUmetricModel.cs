﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetricCollector.Model
{
    public class GPUmetricModel
    {
        public int GPUMem { get; set; }

        public int GPUEngine { get; set; }
    }

    public class GPUDataModel
    {
        public DateTime Time { get; set; }

        public GPUmetricModel Result { get; set; }
    }
}
