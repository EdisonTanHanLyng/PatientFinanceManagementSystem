using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using PFMS_MI04.Controllers;
using PFMS_MI04.Models.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SessionCleanupService : BackgroundService
{
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly JwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionCleanupService(ILogger<SessionCleanupService> logger, JwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            cleanupInactiveSessions();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Check every 5 seconds
        }
    }

    private async void cleanupInactiveSessions()
    {
        var currentTime = DateTime.UtcNow;
        var httpContext = _httpContextAccessor.HttpContext;
        foreach (var user in HomeController.LastHeartbeats)
        {
            if ((currentTime - user.Value).TotalSeconds > 3)  //Verify if stop receive sign for 3 seconds 
            {
                _logger.LogInformation($"User {user.Key} inactive for more than 3 seconds. Logging out.");
                HomeController.LastHeartbeats.TryRemove(user.Key, out _);

                // Find and remove the JWT for this user
                var jwtToRemove = AuthRepoUser._userMap.FirstOrDefault(x => x.Value.AccId == user.Key).Key;
                if (jwtToRemove != null)
                {
                    httpContext?.Response.Cookies.Delete(jwtToRemove);
                    AuthRepoUser.RemoveUser(jwtToRemove);
                }
            }
        }
    }
}