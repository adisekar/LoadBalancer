using LoadBalancerService.Models;
using LoadBalancerService.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace LoadBalancerService.Filters
{
    public class GetLeastActiveServer : ActionFilterAttribute
    {
        private readonly ILBService _loadBalancerService;
        static Random random = new Random();
        private readonly ConcurrentDictionary<string, ServerLastUpdated> clientServerMap;
        private readonly ConcurrentDictionary<string, HashSet<string>> activeServerSessions;

        private const string SERVER_NAME = "serverName";
        private const string CLIENT_IP = "clientIp";

        public GetLeastActiveServer(ILBService lBService)
        {
            _loadBalancerService = lBService;
            clientServerMap = _loadBalancerService.GetClientServerMapping();
            activeServerSessions = _loadBalancerService.GetServerClientMapping(_loadBalancerService.ServerMap.Keys.ToList());
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Identify client based on ip address
            string clientIp = Utils.GetClientIPAddress(context.HttpContext);
            if (!string.IsNullOrEmpty(clientIp) && clientIp != "::1")
            {
                context.HttpContext.Items.Add(CLIENT_IP, clientIp);

                // If load balancer is having active connections
                if (clientServerMap.Count > 0)
                {
                    // If existing client, find last request time 30s or higher
                    if (clientServerMap.ContainsKey(clientIp))
                    {
                        var serverDetails = clientServerMap[clientIp];
                        // If 30 seconds have been passed, then assign to lowest session server
                        //if (serverDetails.LastUpdated.AddSeconds(30) > DateTime.Now)
                        if ((DateTime.Now - serverDetails.LastUpdated).TotalSeconds > 30)
                        {
                            activeServerSessions[serverDetails.Name].Remove(clientIp); // Removes old session

                            // can be done using minHeap
                            var leastActiveServer = activeServerSessions.OrderBy(kv => kv.Value.Count).FirstOrDefault();
                            leastActiveServer.Value.Add(clientIp); // Adds new session

                            // update client server map with new mapping
                            serverDetails.Name = leastActiveServer.Key;
                            serverDetails.LastUpdated = DateTime.Now;

                            //serverName = leastActiveServer.Key;
                            context.HttpContext.Items.Add(SERVER_NAME, leastActiveServer.Key);
                        }
                        else // sticky session, assign to same server. Just update time stamp
                        {
                            clientServerMap[clientIp].LastUpdated = DateTime.Now;
                            // Already active sessions exist, no need to update

                            //serverName = clientServerMap[clientIp].Name;
                            context.HttpContext.Items.Add(SERVER_NAME, clientServerMap[clientIp].Name);
                        }
                    } // if new client
                    else // Gets server node with least sessions
                    {
                        // can be done using minHeap
                        var leastActiveServer = activeServerSessions.OrderBy(kv => kv.Value.Count).FirstOrDefault();
                        //serverName = leastActiveServer.Key;
                        context.HttpContext.Items.Add(SERVER_NAME, leastActiveServer.Key);

                        leastActiveServer.Value.Add(clientIp); // Adds new session
                        //clientServerMap.Add(clientIp, new ServerLastUpdated(serverName, DateTime.Now));
                        clientServerMap.TryAdd(clientIp, new ServerLastUpdated(context.HttpContext.Items[SERVER_NAME].ToString(), DateTime.Now));
                    }
                }
                else // Assign to any server node, as all of them are available
                {
                    int randomServerId = random.Next(_loadBalancerService.ServerMap.Count);
                    var serverName = randomServerId.ToString();
                    context.HttpContext.Items.Add(SERVER_NAME, serverName);

                    clientServerMap.TryAdd(clientIp, new ServerLastUpdated(serverName, DateTime.Now));
                    activeServerSessions[serverName].Add(clientIp);
                }
            }
            else // No IP Address from Client
            {
                context.HttpContext.Items.Add(CLIENT_IP, null);
            }
        }
    }
}
