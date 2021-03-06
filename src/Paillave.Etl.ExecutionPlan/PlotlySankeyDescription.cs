﻿using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl;

namespace Paillave.Etl.ExecutionPlan
{
    public class PlotlySankeyDescription
    {
        public List<string> NodeNames { get; set; }
        public List<string> NodeColors { get; set; }
        public List<int> LinkSources { get; set; }
        public List<int> LinkTargets { get; set; }
        public List<int> LinkValues { get; set; }
    }
}
