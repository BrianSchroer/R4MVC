﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R4MvcHostApp.Data;
using R4MvcHostApp.Models;
using R4MvcHostApp.Services;

namespace R4MvcHostApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddRazorOptions(razorOptions =>
                {
                    // Standard feature views
                    razorOptions.ViewLocationFormats.Add("/Features/{1}/{0}.cshtml");
                    razorOptions.ViewLocationFormats.Add("/Features/{0}.cshtml");
                    razorOptions.ViewLocationFormats.Add("/Features/Shared/{0}.cshtml");

                    // Area feature views
                    razorOptions.AreaViewLocationFormats.Add("/Areas/{2}/Features/{1}/{0}.cshtml");
                    razorOptions.AreaViewLocationFormats.Add("/Areas/{2}/Features/{0}.cshtml");
                    razorOptions.AreaViewLocationFormats.Add("/Areas/{2}/Features/Shared/{0}.cshtml");

                    // Feature folder area views
                    razorOptions.AreaViewLocationFormats.Add("/Areas/{2}/{1}/{0}.cshtml");
                    razorOptions.AreaViewLocationFormats.Add("/Areas/{2}/{0}.cshtml");
                    razorOptions.AreaViewLocationFormats.Add("/Areas/{2}/Shared/{0}.cshtml");
                });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(Areas.Admin.Models.Index2ViewModel), new SimplePropertyModelUnbinder());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
