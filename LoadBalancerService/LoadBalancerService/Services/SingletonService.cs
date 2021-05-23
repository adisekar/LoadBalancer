using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancerService.Models;

namespace LoadBalancerService.Services
{
    public class SingletonService
    {
        private static Dictionary<string, string> serverMap;
        private static Dictionary<string, ServerLastUpdated> clientServerMap;
        private static Dictionary<string, HashSet<string>> activeServerSessions;
        private SingletonService() { }

        public static Dictionary<string, string> GetServerMapInstance(IList<ServerDetail> serverDetails)
        {
            if (serverMap == null)
            {
                serverMap = new Dictionary<string, string>();

                foreach (var serverDetail in serverDetails)
                {
                    serverMap.Add(serverDetail.Name, serverDetail.Url);
                }
            }
            return serverMap;
        }

        public static Dictionary<string, ServerLastUpdated> GetClientServerMapInstance()
        {
            if (clientServerMap == null)
            {
                clientServerMap = new Dictionary<string, ServerLastUpdated>();
            }
            return clientServerMap;
        }

        public static Dictionary<string, HashSet<string>> GetServerSessionsMapInstance(List<string> servers)
        {
            if (activeServerSessions == null)
            {
                activeServerSessions = new Dictionary<string, HashSet<string>>();
                foreach (var server in servers)
                {
                    activeServerSessions.Add(server, new HashSet<string>());
                }
            }
            return activeServerSessions;
        }
    }
}
