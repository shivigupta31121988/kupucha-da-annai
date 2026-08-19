using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "dev_secret_key_change_me";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
	.AddCookie()
	.AddGoogle("Google", options =>
	{
		options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
		options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
		options.CallbackPath = "/auth/google/callback";
		options.Scope.Add("email");
		options.Events.OnCreatingTicket = ctx =>
		{
			var email = ctx.Principal?.FindFirst(ClaimTypes.Email)?.Value ?? ctx.Principal?.FindFirst("email")?.Value;
			var token = CreateJwt(email, jwtKey);
			var redirect = frontendUrl + $"/?token={token}";
			ctx.Response.Redirect(redirect);
			return Task.CompletedTask;
		};
	})
	.AddOpenIdConnect("Apple", options =>
	{
		// Apple requires additional configuration (keys, team id, etc.).
		options.Authority = "https://appleid.apple.com";
		options.ClientId = Environment.GetEnvironmentVariable("APPLE_CLIENT_ID");
		options.CallbackPath = "/auth/apple/callback";
		options.ResponseType = "code";
		options.Scope.Add("email");
		options.SaveTokens = true;
		options.Events = new OpenIdConnectEvents
		{
			OnTokenValidated = ctx =>
			{
				var email = ctx.Principal?.FindFirst(ClaimTypes.Email)?.Value ?? ctx.Principal?.FindFirst("email")?.Value;
				var token = CreateJwt(email, jwtKey);
				var redirect = frontendUrl + $"/?token={token}";
				ctx.Response.Redirect(redirect);
				return Task.CompletedTask;
			}
		};
	});

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/auth/google", (HttpContext http) =>
{
	return Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Google");
});

app.MapGet("/auth/apple", (HttpContext http) =>
{
	return Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Apple");
});

app.Run();

static string CreateJwt(string? email, string key)
{
	var claims = new[] { new Claim(ClaimTypes.Email, email ?? ""), new Claim("sub", email ?? "") };
	var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
	var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
	var token = new JwtSecurityToken(issuer: "kupucha", audience: "kupucha-client", claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
	return new JwtSecurityTokenHandler().WriteToken(token);
}
