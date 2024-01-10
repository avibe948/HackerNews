using System.Net.Http;
using HackerNewsAPI.Domain;
using HackerNewsAPI.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HackerNewsAPI
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
            services.AddControllers().AddJsonOptions(option=>option.JsonSerializerOptions.PropertyNameCaseInsensitive=true);
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.Configure<HackerNewsStoryProviderSettings>(Configuration.GetSection(nameof(HackerNewsStoryProviderSettings)));
            services.Configure<HackerNewsApiSettings>(Configuration.GetSection(nameof(HackerNewsApiSettings)));

            services.AddLogging(loggingBuilder =>
            {
                // Use the configuration from appsettings.json
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));

                // Add other logging providers if needed, such as Console, Debug, etc.
                loggingBuilder.AddConsole();
            });
            // Register the HackerNewsStoryProvider as the implementation for IStoryProvider
            services.AddScoped<IStoryProvider, HackerNewsStoryProvider>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
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
