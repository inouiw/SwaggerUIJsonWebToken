using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var googleMetadataAddress = "https://accounts.google.com/.well-known/openid-configuration";
var requiredIssuerName = "accounts.google.com";
var googleClientId = "567570273635-15l48qb6tb2k4km4gqvn2tk1h24g21gs.apps.googleusercontent.com";
var authorizationUrl = "https://accounts.google.com/o/oauth2/v2/auth";
var scopesToRequestFromUser = new[] { "email", "openid", "profile" };

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(options =>
  options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
  var securitySchemeName = "Bearer";
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Implicit JWT Authentication Example",
    Version = "v1"
  });
  options.AddSecurityDefinition(securitySchemeName, new OpenApiSecurityScheme
  {
    Type = SecuritySchemeType.OAuth2,
    Extensions =
    {
      // The name of the value from the response of the auth provider to use as bearer token.
      { "x-tokenName", new OpenApiString("id_token") }
    },
    Flows = new OpenApiOAuthFlows
    {
      Implicit = new OpenApiOAuthFlow
      {
        AuthorizationUrl = new Uri(authorizationUrl),
        Scopes = scopesToRequestFromUser.ToDictionary(x => x, _ => "")
      }
    },
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = securitySchemeName
        }
      },
      new List<string>()
    }
  });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
  {
    options.MetadataAddress = googleMetadataAddress;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuer = requiredIssuerName,
      ValidateAudience = true,
      ValidAudience = googleClientId,
      ValidateIssuerSigningKey = true,
      ValidateLifetime = true,
      RequireExpirationTime = true
    };
  });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpLogging();
app.UseSwagger();

app.UseSwaggerUI(options =>
{
  options.OAuthClientId(googleClientId);
  options.IndexStream = () => new FileStream($"./wwwroot/swagger/index_custom.html", FileMode.Open);
});

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();

app.MapPost("/authentication-info", (IConfiguration config, HttpContext context) =>
{
  var userClaims = context.User.Claims.Select(c => new UserClaimResult(c.Type, c.Value)).ToList();
  bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
  return new AuthenticationInfoResult(isAuthenticated, userClaims);
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

record UserClaimResult(string Type, string Value);
record AuthenticationInfoResult(bool IsAuthenticated, List<UserClaimResult> Claims);