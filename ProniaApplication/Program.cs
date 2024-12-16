using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProniaApplication.DAL;
using ProniaApplication.Models;
using ProniaApplication.Services.Impelementations;
using ProniaApplication.Services.Interfaces;

namespace ProniaApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDBContext>(
                opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))

                );
            builder.Services.AddIdentity<AppUser, IdentityRole>(
                opt =>
                {
                    opt.Password.RequiredLength = 8;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz0123456789_@";
                    opt.User.RequireUniqueEmail = true;

                }
                ).AddEntityFrameworkStores<AppDBContext>().AddDefaultTokenProviders();

            builder.Services.AddScoped<ILayoutService,LayoutService>();
            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllerRoute(
               name: "admin",
               pattern: "{area:exists}/{controller=home}/{action=index}/{id?}"
               );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=home}/{action=index}/{id?}"
                );

            app.Run();
        }
    }
}
