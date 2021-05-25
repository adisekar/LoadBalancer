using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadBalancerService.Services;
using LoadBalancerService.Filters;

namespace LoadBalancerService
{
    public class Startup
    {
        private ILBService _loadBalancerService;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(config =>
            {
                config.Filters.Add<CleanupProcess>();
            });
            services.AddSingleton<CleanupProcess>();
            services.AddScoped<GetLeastActiveServer>();
            services.AddSingleton<ILBService, LBService>();
            services.AddHttpClient();
            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, ILBService loadBalancerService)
        {
            _loadBalancerService = loadBalancerService;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            lifetime.ApplicationStarted.Register(OnApplicationStarted);
        }

        public void OnApplicationStarted()
        {
            var servers = _loadBalancerService.ServiceDiscovery(Configuration);
            _loadBalancerService.UpdateServerMapping(servers);
        }
    }
}
