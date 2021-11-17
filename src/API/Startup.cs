using Hangfire;
using Hangfire.Console;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Scrap.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _= services.AddControllers().AddJsonOptions(options =>
            {
                var jsonConverters = options.JsonSerializerOptions.Converters;
                jsonConverters.Add(new ResourceRepositoryConfigurationJsonConverter());
                jsonConverters.Add(new TimeSpanJsonConverter());
            });
                
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(Configuration["Hangfire:Database"]);
                config.UseConsole();
            });
            services.AddHangfireServer();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" }); });
            services.AddScoped(sc => DependencyInjection.DependencyInjection.BuildJobDefinitionsApplicationService(Configuration, sc.GetRequiredService<ILoggerFactory>()));
            services.AddScoped(sc => DependencyInjection.DependencyInjection.BuildScrapperApplicationService(Configuration, sc.GetRequiredService<ILoggerFactory>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }
            app.UseHangfireDashboard();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}