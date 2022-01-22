using FluentValidation;
using KITT.Core.Commands;
using KITT.Core.Persistence;
using KITT.Core.ReadModels;
using KITT.Core.Validators;
using KITT.Web.Models.Streamings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace LemonBot.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<KittDbContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("KittDatabase")));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.Configure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.TokenValidationParameters.NameClaimType = "name";
            });

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

        builder.Services
            .AddValidatorsFromAssemblyContaining<StreamingValidator>()
            .AddScoped<IDatabase, Database>()
            .AddScoped<ISettingsCommands, SettingsCommands>()
            .AddScoped<IStreamingCommands, StreamingCommands>();

        builder.Services
            .AddScoped<Areas.Console.Services.StreamingsControllerServices>()
            .AddScoped<Areas.Console.Services.SettingsControllerServices>();

        //builder.Services.AddScoped<AccountControllerbuilder.Services>();

        builder.Services.AddSignalR();
        builder.Services.AddControllersWithViews();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Description = "Set di API per la gestione della console di gestione del canale",
                Title = "KITT - API",
                Contact = new OpenApiContact
                {
                    Name = "albx87",
                    Url = new Uri("https://twitch.tv/albx87")
                },
                License = new OpenApiLicense
                {
                    Name = "AGPL v3.0",
                    Url = new Uri("https://github.com/albx/KITT/blob/main/LICENSE")
                }
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.OAuth2,
                Scheme = "Bearer"
            };

            //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //{
            //    Name = "Authorization",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.OAuth2,
            //    Scheme = "Bearer"
            //});

            //options.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    { 
            //        new OpenApiSecurityScheme
            //        {
            //            Name = "Bearer",
            //            In = ParameterLocation.Header,
            //            Reference = new OpenApiReference
            //            {
            //                Id = "Bearer",
            //                Type = ReferenceType.SecurityScheme
            //            }
            //        }, 
            //        new List<string>() 
            //    }
            //});

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            var modelsXmlFilename = $"{typeof(ScheduleStreamingModel).Assembly.GetName().Name}.xml";
            var assemblyPath = Path.GetDirectoryName(typeof(ScheduleStreamingModel).Assembly.Location);
            options.IncludeXmlComments(Path.Combine(assemblyPath, modelsXmlFilename));
        });

        return builder;
    }
}
