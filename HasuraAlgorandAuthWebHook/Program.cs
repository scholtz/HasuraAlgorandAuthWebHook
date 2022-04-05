using AlgorandAuthentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Comment Service API",
        Version = "v1",
        Description = File.ReadAllText("doc/readme.txt")
    });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "ARC-0014 Algorand authentication transaction",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        
    });

    c.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});


builder.Services
 .AddAuthentication(AlgorandAuthentication.AlgorandAuthenticationHandler.ID)
 .AddAlgorand(o =>
 {
     o.CheckExpiration = true;
     o.AlgodServer = builder.Configuration["algod:server"];
     o.AlgodServerToken = builder.Configuration["algod:token"];
     o.Realm = builder.Configuration["algod:realm"];
     o.NetworkGenesisHash = builder.Configuration["algod:networkGenesisHash"];
     o.Debug = true;
     o.EmptySuccessOnFailure = true;
 });


var corsConfig = builder.Configuration.GetSection("Cors").AsEnumerable().Select(k => k.Value).Where(k => !string.IsNullOrEmpty(k)).ToArray();
if (corsConfig.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins(corsConfig)
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();
if (corsConfig.Length > 0)
{
    app.UseCors();
}

app.MapControllers();

app.Run();
