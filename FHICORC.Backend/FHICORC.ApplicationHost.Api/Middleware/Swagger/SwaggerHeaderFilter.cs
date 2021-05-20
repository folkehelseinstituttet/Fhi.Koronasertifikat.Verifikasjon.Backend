using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FHICORC.ApplicationHost.Api.Middleware.Swagger
{
    public class SwaggerHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Unwrap complex header parameters
            var parametersToRemove = new List<OpenApiParameter>();
            var parametersToAdd = new List<OpenApiParameter>();
            foreach (var opParm in operation.Parameters)
            {
                if (opParm.In == ParameterLocation.Header)
                {
                    HandleHeaderParameter(context, opParm, parametersToRemove, parametersToAdd);
                }
            }

            foreach (var parm in parametersToRemove)
            {
                operation.Parameters.Remove(parm);
            }

            foreach (var parm in parametersToAdd)
            {
                operation.Parameters.Add(parm);
            }
        }

        private static void HandleHeaderParameter(OperationFilterContext context, OpenApiParameter opParm,
            List<OpenApiParameter> parametersToRemove, List<OpenApiParameter> parametersToAdd)
        {
            var desc = context.ApiDescription.ParameterDescriptions.FirstOrDefault(d =>
                d.ParameterDescriptor?.Name == opParm.Name);

            if (desc != null && desc.ModelMetadata.IsComplexType)
            {
                parametersToRemove.Add(opParm);
                foreach (var boundParm in desc.ModelMetadata.Properties)
                {
                    if (boundParm.BindingSource == BindingSource.Header)
                    {
                        parametersToAdd.Add(new OpenApiParameter
                        {
                            Name = boundParm.Name,
                            In = ParameterLocation.Header,
                            Schema = new OpenApiSchema
                            {
                                Type = boundParm.DataTypeName
                            }
                        });
                    }
                }
            }
        }
    }
}
