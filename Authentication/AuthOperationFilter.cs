using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Demo1.Authentication
{
    public class AuthOperationFilter : IOperationFilter
    {
        public void Apply(
            OpenApiOperation operation,
            OperationFilterContext context)
        {
            var authorizeAttributes =
                context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .ToList();

            authorizeAttributes.AddRange(
                context.MethodInfo
                    .DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                ?? []);

            var allowAnonymous =
                context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AllowAnonymousAttribute>()
                    .Any();

            if (allowAnonymous)
            {
                operation.Security.Clear();
                return;
            }


            // =============================================
            // Explicit API Key authentication
            // =============================================

            var apiKeyRequired =
                authorizeAttributes.Any(
                    x =>
                        x.AuthenticationSchemes?
                            .Split(',')
                            .Any(
                                s =>
                                    s.Trim() == "ApiKey")
                        == true);

            if (apiKeyRequired)
            {
                operation.Security.Clear();

                operation.Security.Add(
                    CreateRequirement("ApiKey"));

                return;
            }


            // =============================================
            // Explicit JWT authentication
            // =============================================

            var jwtRequired =
                authorizeAttributes.Any(
                    x =>
                        x.AuthenticationSchemes?
                            .Split(',')
                            .Any(
                                s =>
                                    s.Trim() ==
                                    JwtBearerDefaults
                                        .AuthenticationScheme)
                        == true);

            if (jwtRequired)
            {
                operation.Security.Clear();

                operation.Security.Add(
                    CreateRequirement("Bearer"));

                return;
            }


            // =============================================
            // Policy-based authentication
            // Policies in this app use JWT
            // =============================================

            if (authorizeAttributes.Any(
                    x =>
                        !string.IsNullOrWhiteSpace(
                            x.Policy)))
            {
                operation.Security.Clear();

                operation.Security.Add(
                    CreateRequirement("Bearer"));
            }
        }


        private static OpenApiSecurityRequirement
            CreateRequirement(
                string scheme)
        {
            return new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type =
                                    ReferenceType
                                        .SecurityScheme,

                                Id = scheme
                            }
                    },

                    Array.Empty<string>()
                }
            };
        }
    }
}