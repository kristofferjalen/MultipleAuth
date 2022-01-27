using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// infinite loop if: first Add..WebApp then Add..WebApi
// ok if: first Add..WebApi then Add..WebApp
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization(options =>
{
    // Need this for [Authorize] on controllers to use bearer token for authentication
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger") && !(context.User.Identity?.IsAuthenticated ?? false))
    {
        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
    else
    {
        await next();
    }
});

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
