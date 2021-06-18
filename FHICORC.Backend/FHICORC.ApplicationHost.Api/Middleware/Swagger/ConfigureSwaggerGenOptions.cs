using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using FHICORC.Application.Models.Options;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FHICORC.ApplicationHost.Api.Middleware.Swagger
{
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly SecurityOptions _securityOptions;
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerGenOptions(SecurityOptions securityOptions, IApiVersionDescriptionProvider provider)
        {
            _securityOptions = securityOptions;
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo()
                    {
                        Title = $"FHICORC.ApplicationHost.Api V{description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                    });
            }

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
