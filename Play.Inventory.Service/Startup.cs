using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common;
using Play.Inventory.Entities;
using Play.Inventory.Service.Clients;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Play.Inventory.Service
{
    public class Startup
    {
        private readonly Dictionary<string, string> catalogServiceDictionary;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            catalogServiceDictionary = _configuration.GetSection("CatalogService").Get<Dictionary<string, string>>();
        }

        public IConfiguration _configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            int APITimeout = 0;
            if (catalogServiceDictionary["APITimeout"] != null)
                int.TryParse(catalogServiceDictionary["APITimeout"].ToString(), out APITimeout);
            services.AddMongo()
                .AddMongoRepository<InventoryItem>("InventoryItems");

            Random jitterer = new Random();

            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri(catalogServiceDictionary["BaseAddress"]);
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
              5,
              retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
              onRetry: (outcome, timespan, retryAttempt) =>
              {
                  var serviceProvider = services.BuildServiceProvider();
                  serviceProvider.GetService<ILogger<CatalogClient>>()?.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
              }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timespan) =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()
                    ?
                    .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
                },
                onReset: () => 
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()
                    ?
                    .LogWarning($"Closing the circuit.");
                }
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(APITimeout));

            services.AddControllers(c =>
            {
                c.SuppressAsyncSuffixInActionNames = false;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
