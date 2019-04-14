using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.IO;

namespace Goober.Swagger
{
    public static class ConfigExtensions
    {
        public static void AddSwaggerGenWithDocs(this IServiceCollection service, string version = "v1", string title = "Web API")
        {
            service.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, new Info { Title = title, Version = version });

                c.DescribeAllEnumsAsStrings();
            });
        }

        public static void AddSwaggerGenWithXmlDocs(this IServiceCollection service, IEnumerable<string> xmlDocFileNameList, string version = "v1", string title = "Web API")
        {
            service.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, new Info { Title = title, Version = version });

                c.DescribeAllEnumsAsStrings();

                var applicationPath = PlatformServices.Default.Application.ApplicationBasePath;
                foreach (var iFileName in xmlDocFileNameList)
                {
                    var xmlPath = Path.Combine(applicationPath, iFileName);
                    c.IncludeXmlComments(xmlPath);
                }
            });
        }

        public static void UseSwaggerUIWithDocs(this IApplicationBuilder app, string version = "v1", string title = "Web API")
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{title} {version}");
            });
        }
    }
}
