using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoadBalancerService.Models
{
    public class ServerDetail
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public ServerDetail(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
