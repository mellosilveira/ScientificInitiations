using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace IcVibracoes.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder app)
        {
            string assemblyTitle = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{assemblyTitle}/swagger.json", $"{assemblyTitle} API");
                c.EnableValidator(null);
            });

            return app;
        }
    }
}
