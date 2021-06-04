using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancerService.Models;
using System.Collections.Concurrent;

namespace LoadBalancerService.Services
{
    public interface ILBService
    {
        public ConcurrentDictionary<string, string> ServerMap { get; set; }
        IList<ServerDetail> ServiceDiscovery(IConfiguration config);
        void UpdateServerMapping(IList<ServerDetail> serverDetails);
        ConcurrentDictionary<string, ServerLastUpdated> GetClientServerMapping();
        ConcurrentDictionary<string, HashSet<string>> GetServerClientMapping(List<string> servers);
        void CleanUp(ConcurrentDictionary<string, ServerLastUpdated> clientServerMap, ConcurrentDictionary<string, HashSet<string>> activeServerSessions);
    }
}
