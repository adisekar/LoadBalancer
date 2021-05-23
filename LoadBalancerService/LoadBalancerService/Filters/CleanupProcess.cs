using LoadBalancerService.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoadBalancerService.Filters
{
    public class CleanupProcess : ActionFilterAttribute
    {
        private readonly ILBService _loadBalancerService;
        static DateTime lastCleanup = DateTime.Now;

        public CleanupProcess(ILBService lBService)
        {
            _loadBalancerService = lBService;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Fire and Forget Cleanup process, every 10 seconds. Can be done as a separate process. If data is stored in a cache
            if ((DateTime.Now - lastCleanup).TotalSeconds > 10)
            {
                _ = Task.Run(() =>
                {
                    var activeServerSessions = _loadBalancerService.GetServerClientMapping(_loadBalancerService.ServerMap.Keys.ToList());
                    _loadBalancerService.CleanUp(_loadBalancerService.GetClientServerMapping(), activeServerSessions);
                    lastCleanup = DateTime.Now;
                }).ConfigureAwait(false);
            }
        }
    }
}