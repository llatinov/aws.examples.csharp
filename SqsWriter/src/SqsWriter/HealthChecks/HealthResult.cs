using System;
using System.Collections.Generic;

namespace SqsWriter.HealthChecks
{
    public class HealthResult
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public TimeSpan Duration { get; set; }
        public ICollection<HealthInfo> Info { get; set; }
    }
}