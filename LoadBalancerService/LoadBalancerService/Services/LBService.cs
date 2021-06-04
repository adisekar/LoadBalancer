using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancerService.Models;
using System.Collections.Concurrent;

namespace LoadBalancerService.Services
{
    public class LBService : ILBService
    {
        public ConcurrentDictionary<string, string> ServerMap { get; set; }

        public IList<ServerDetail> ServiceDiscovery(IConfiguration config)
        {
            var serverDetails = new List<ServerDetail>();
            var serverSectionEnumerable = config.GetSection("Servers").GetChildren();
            foreach (IConfigurationSection section in serverSectionEnumerable)
            {
                var name = section.GetValue<string>("Name");
                var url = section.GetValue<string>("Url");
                var serverDetail = new ServerDetail(name, url);
                serverDetails.Add(serverDetail);
            }

            return serverDetails;
        }

        public void UpdateServerMapping(IList<ServerDetail> serverDetails)
        {
            ServerMap = SingletonService.GetServerMapInstance(serverDetails);
        }

        public ConcurrentDictionary<string, ServerLastUpdated> GetClientServerMapping()
        {
            return SingletonService.GetClientServerMapInstance();
        }

        public ConcurrentDictionary<string, HashSet<string>> GetServerClientMapping(List<string> servers)
        {
            ConcurrentDictionary<string, HashSet<string>> activeServerSessions = SingletonService.GetServerSessionsMapInstance(servers);

            return activeServerSessions;
        }

        public void CleanUp(ConcurrentDictionary<string, ServerLastUpdated> clientServerMap, ConcurrentDictionary<string, HashSet<string>> activeServerSessions)
        {
            var iterationDictionary = clientServerMap.ToDictionary(entry => entry.Key,
                                                 entry => entry.Value);
            foreach (var kv in iterationDictionary)
            {
                if ((DateTime.Now - kv.Value.LastUpdated).TotalSeconds > 30)
                {
                    var clientId = kv.Key;
                    var serverName = kv.Value.Name;
                    activeServerSessions[serverName].Remove(clientId);
                    ServerLastUpdated _;
                    clientServerMap.TryRemove(clientId, out _);
                }
            }
        }
    }
}
