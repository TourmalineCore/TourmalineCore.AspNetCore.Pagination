using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Example.NetCore5._0.CommonUsage.Models;
using Example.NetCore5._0.CommonUsage.Queries;

namespace Example.NetCore5._0.CommonUsage
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
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName: "ApplicationDb"));
            services.AddControllers();
            services.AddTransient<ProductsQuery>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseRouting();

            InitDb(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void InitDb(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var vendor1 = new Vendor() { Name = "TestVendor1" };
            var vendor2 = new Vendor() { Name = "TestVendor2" };

            context.Vendors.AddRange(vendor1, vendor2);

            context.Products.AddRange(
                    new Product
                    {
                        Name = "First",
                        Cost = 50,
                        ExpirationDate = DateTime.Today,
                        Vendor = vendor1,
                    },
                    new Product
                    {
                        Name = "Second",
                        Cost = 150,
                        ExpirationDate = DateTime.Today.AddDays(5),
                        Vendor = vendor1,
                    },
                    new Product
                    {
                        Name = "Third",
                        Cost = 250,
                        ExpirationDate = DateTime.Today.AddDays(10),
                        Vendor = vendor2,
                    }
                );

            context.SaveChanges();
        }
    }
}
