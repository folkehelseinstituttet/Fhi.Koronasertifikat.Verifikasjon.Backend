using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using FHICORC.Application.Models.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FHICORC.ApplicationHost.Api.Middleware.Swagger
{
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly SecurityOptions _securityOptions;

        public ConfigureSwaggerGenOptions(SecurityOptions securityOptions)
        {
            _securityOptions = securityOptions;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "FHICORC.ApplicationHost.Api", Version = "v1" });
            options.OperationFilter<SwaggerHeaderFilter>();

            if (_securityOptions.CheckApiKeyHeader)
            {
                options.AddSecurityDefinition("ApiKeyAuthorization", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = _securityOptions.ApiKeyHeader,
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKeyAuthorization"
                            }
                        },
                        new List<string>()
                    }
                });
            }
        }
    }
}
