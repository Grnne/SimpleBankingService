using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "http://keycloak:8080/realms/my_realm";
                options.Audience = "my_client";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var mbError = new MbError(HttpStatusCode.Unauthorized, "Unauthorized",
                            "Authentication is required or token is invalid.");
                        var result = new MbResult<object>(mbError);

                        await context.Response.WriteAsJsonAsync(result);
                    }
                };
            });

        return services.AddAuthorization();
    }
}