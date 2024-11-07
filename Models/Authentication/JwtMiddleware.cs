
namespace PFMS_MI04.Models.Authentication
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public JwtMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            string tokenToRevoke;
            while ((tokenToRevoke = AuthRepoUser.getTokenToRevoke()) != null)
            {
                AuthRepoUser.RemoveUser(tokenToRevoke);
            }

            // Check if the current request's token is valid
            var currentToken = context.Request.Cookies["jwt"];
            if (currentToken != null)
            {
                if (AuthRepoUser.IsTokenRevoked(currentToken) || !AuthRepoUser.IsTokenInvoke(currentToken))
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Cookies.Delete("jwt");
                        await context.Response.WriteAsync("Unauthorized: Token has been revoked");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Response has already started, cannot modify headers.");
                    }
                }
            }

            await _next(context);
        }
    }
}
