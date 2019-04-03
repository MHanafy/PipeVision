using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PipeVision.Application;
using PipeVision.Data;
using PipeVision.Domain;
using PipeVision.GoPipeline;

namespace PipeVision.Web
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                //options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<PipelineContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DbConnection"), opt => opt.CommandTimeout(Configuration.GetValue<int>("DbCommandTimeout")));
            });

            services.AddAutoMapper();

            services.AddMemoryCache();

            services.AddScoped<IPipelineRepository, PipelineRepository>();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<IPipelineService, PipelineService>();
            services.AddTransient<IPipelineUrlResolver, GoPipelineUrlResolver>();
            services.AddSingleton<ITimerService, TimerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Test}/{action=Index}/{id?}");
            });

        }
    }
}
