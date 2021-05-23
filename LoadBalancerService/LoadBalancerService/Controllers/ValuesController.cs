using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LoadBalancerService.Services;
using LoadBalancerService.Models;
using LoadBalancerService.Filters;

namespace LoadBalancerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        static Random random = new Random();
        private readonly ILBService _loadBalancerService;

        private const string SERVER_NAME = "serverName";
        private const string CLIENT_IP = "clientIp";

        public ValuesController(ILBService loadBalancerService)
        {
            _loadBalancerService = loadBalancerService;
        }

        [HttpGet]
        [ServiceFilter(typeof(GetLeastActiveServer))]
        public async Task<ActionResult<string>> GetData()
        {
            string serverName = null;
            if (HttpContext.Items.ContainsKey(CLIENT_IP) && HttpContext.Items[CLIENT_IP] != null)
            {
                if (HttpContext.Items.ContainsKey(SERVER_NAME))
                {
                    serverName = HttpContext.Items[SERVER_NAME].ToString();

                    string serverUrl = _loadBalancerService.ServerMap[serverName];
                    string endpoint = "/api/values";
                    string url = serverUrl + endpoint;

                    // Pass the request thru to servers
                    using (HttpClient client = new HttpClient())
                    {
                        return await client.GetStringAsync(url);
                    }
                }
                else
                {
                    return BadRequest("Server Name could not be found");
                }
            }
            else
            {
                // No Ip Address exist, cannot identify client. Return Error
                return BadRequest("X-Forwarded-For header missing");
            }
        }
    }
}