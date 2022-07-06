using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var googleMetadataAddress = "https://accounts.google.com/.well-known/openid-configuration";
var requiredIssuerName = "accounts.google.com";
var googleClientId = "567570273635-15l48qb6tb2k4km4gqvn2tk1h24g21gs.apps.googleusercontent.com";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(options =>
  options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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