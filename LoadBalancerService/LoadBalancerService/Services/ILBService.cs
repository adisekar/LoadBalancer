using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancerService.Models;

namespace LoadBalancerService.Services
{
    public interface ILBService
    {
        public IDictionary<string, string> ServerMap { get; set; }
        IList<ServerDetail> ServiceDiscovery(IConfiguration config);
        void UpdateServerMapping(IList<ServerDetail> serverDetails);
        IDictionary<string, ServerLastUpdated> GetClientServerMapping();
        IDictionary<string, HashSet<string>> GetServerClientMapping(List<string> servers);
        void CleanUp(IDictionary<string, ServerLastUpdated> clientServerMap, IDictionary<string, HashSet<string>> activeServerSessions);
    }
}
