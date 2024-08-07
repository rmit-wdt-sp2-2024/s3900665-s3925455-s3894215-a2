using MCBA_Web_App.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MCBA_Web_App.Models;
using System;


namespace MCBA_Web_App
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<MCBAContext>(options =>
            {
                options
                //.UseLazyLoadingProxies()
                .UseSqlServer(Configuration.GetConnectionString("BankingContext"));
                //options.UseLazyLoadingProxies();
            });
            //Add Background services
            services.AddHostedService<BillPayService>();
            services.AddScoped<BillPayService>();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Make the session cookie essential.
                options.Cookie.IsEssential = true;
            });
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseSession();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // Seed the database during application startup
            using (var scope = app.ApplicationServices.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MCBAContext>();
                    //SeedData.LoadDataAsync(dbContext).Wait();
                    await SeedData.LoadDataAsync(dbContext);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding Database: {ex.Message}");
                    throw;
                }
            }

            Console.WriteLine("Database seeding complete.");
        }
    }
}