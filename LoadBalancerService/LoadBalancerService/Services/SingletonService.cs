using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancerService.Models;

namespace LoadBalancerService.Services
{
    public class SingletonService
    {
        private static ConcurrentDictionary<string, string> serverMap;
        private static ConcurrentDictionary<string, ServerLastUpdated> clientServerMap;
        private static ConcurrentDictionary<string, HashSet<string>> activeServerSessions;
        private static object padlock = new object();
        private SingletonService() { }

        public static ConcurrentDictionary<string, string> GetServerMapInstance(IList<ServerDetail> serverDetails)
        {
            if (serverMap == null)
            {
                // double checked locking
                lock (padlock)
                {
                    if (serverMap == null)
                    {
                        serverMap = new ConcurrentDictionary<string, string>();
                        foreach (var serverDetail in serverDetails)
                        {
                            serverMap.TryAdd(serverDetail.Name, serverDetail.Url);
                        }
                    }
                }
            }
            return serverMap;
        }

        public static ConcurrentDictionary<string, ServerLastUpdated> GetClientServerMapInstance()
        {
            // double checked locking
            if (clientServerMap == null)
            {
                lock (padlock)
                {
                    if (clientServerMap == null)
                    {
                        clientServerMap = new ConcurrentDictionary<string, ServerLastUpdated>();
                    }
                }
            }
            return clientServerMap;
        }

        public static ConcurrentDictionary<string, HashSet<string>> GetServerSessionsMapInstance(List<string> servers)
        {
            if (activeServerSessions == null)
            {
                // double checked locking
                lock (padlock)
                {
                    if (activeServerSessions == null)
                    {
                        activeServerSessions = new ConcurrentDictionary<string, HashSet<string>>();
                        foreach (var server in servers)
                        {
                            activeServerSessions.TryAdd(server, new HashSet<string>());
                        }
                    }
                }
            }
            return activeServerSessions;
        }
    }
}
