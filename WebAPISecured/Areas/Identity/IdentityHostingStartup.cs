using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAPISecured.Areas.Identity.Data;
using WebAPISecured.Data;

[assembly: HostingStartup(typeof(WebAPISecured.Areas.Identity.IdentityHostingStartup))]
namespace WebAPISecured.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<WebAPISecuredContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("WebAPISecuredContextConnection")));

                services.AddDefaultIdentity<WebAPISecuredUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddEntityFrameworkStores<WebAPISecuredContext>();
            });
        }
    }
}