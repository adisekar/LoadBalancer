﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LoadBalancerService.Services;
using LoadBalancerService.Models;
using LoadBalancerService.Filters;
using Microsoft.Extensions.Logging;

namespace LoadBalancerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        static Random random = new Random();
        private readonly ILBService _loadBalancerService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ValuesController> _logger;

        private const string SERVER_NAME = "serverName";
        private const string CLIENT_IP = "clientIp";

        public ValuesController(ILBService loadBalancerService, IHttpClientFactory clientFactory, ILogger<ValuesController> logger)
        {
            _loadBalancerService = loadBalancerService;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        [HttpGet]
        [ServiceFilter(typeof(GetLeastActiveServer))]
        public async Task<ActionResult<string>> GetData()
        {
            try
            {
                _logger.LogInformation("Entering GetData");
                string serverName = null;
                if (HttpContext.Items.ContainsKey(CLIENT_IP) && HttpContext.Items[CLIENT_IP] != null)
                {
                    _logger.LogInformation($"Client IpAddress is {HttpContext.Items[CLIENT_IP]}");
                    if (HttpContext.Items.ContainsKey(SERVER_NAME))
                    {
                        serverName = HttpContext.Items[SERVER_NAME].ToString();
                        _logger.LogInformation($"Server Name is {serverName}");

                        string serverUrl = _loadBalancerService.ServerMap[serverName];
                        string endpoint = "/api/values";
                        string url = serverUrl + endpoint;

                        // Pass the request thru to servers
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        var client = _clientFactory.CreateClient();
                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            _logger.LogError($"Server {serverName} not responding");
                            return BadRequest("Something went wrong");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Server Name could not be found");
                        return BadRequest("Server Name could not be found");
                    }
                }
                else
                {
                    // No Ip Address exist, cannot identify client. Return Error
                    _logger.LogError($"X-Forwarded-For header missing");
                    return BadRequest("X-Forwarded-For header missing");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return BadRequest("Something went wrong");
            }
        }
    }
}