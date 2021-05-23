using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoadBalancerService.Models
{
    public class ServerLastUpdated
    {
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }

        public ServerLastUpdated(string name, DateTime lastUpdated)
        {
            Name = name;
            LastUpdated = lastUpdated;
        }
    }
}
