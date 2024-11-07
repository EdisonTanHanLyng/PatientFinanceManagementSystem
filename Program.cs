using PFMS_MI04.Services;
using PFMS_MI04.Models.Authentication;
using Microsoft.Extensions.Configuration;
using PFMS_MI04.Hubs;

namespace PFMS_MI04
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DinkToPdf converter service
            builder.Services.AddTransient<PDFService>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add SignalR services
            builder.Services.AddSignalR();

            builder.Services.AddCors();

            // Register the EmailService
            builder.Services.AddScoped<EmailService>();

            // Register the ManageAccountService
            builder.Services.AddScoped<ManageAccountService>();

            // Register the ManageUserAccountService
            builder.Services.AddScoped<ManageUserAccountService>();

            //Authentication
            builder.Services.AddScoped<AuthRepoUser>();
            builder.Services.AddSingleton<JwtService>();
            builder.Services.AddHostedService<SessionCleanupService>();
            builder.Services.AddHttpContextAccessor();

            /*
            builder.Services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 443;
            });

            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });
            */

            //Add reminder schedulers
            builder.Services.AddSingleton<EmailService>();
            builder.Services.AddHostedService<ReminderSchedulerService>();

            //Add weekly auto backup check & scheduler

            builder.Services.AddScoped<BackupService>();
            builder.Services.AddHostedService<BackupService>();

            var app = builder.Build();

            /*
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            */
            app.UseStaticFiles();

            app.UseRouting();

            //Authentication
            app.UseMiddleware<JwtMiddleware>();
            app.UseCors(OptionsBuilderConfigurationExtensions => OptionsBuilderConfigurationExtensions
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            );

            app.UseAuthentication();
            app.UseAuthorization();
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "api",
                pattern: "api/{controller=Backup}/{action=Index}/{id?}");

            // Map SignalR hubs
            app.MapHub<ReminderHub>("/reminderHub");
            app.MapHub<BackupHub>("/backupHub");
            app.MapHub<ManageAccountHub>("/ManageAccountHub");

            app.Run();
        }
    }
}
