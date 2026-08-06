using Billing_Software_Api.Helpers;
using Billing_Software_Api.Middleware;

namespace Billing_Software_Api.Extensions;

/// <summary>
/// Application builder extension methods configuring HTTP request pipeline and Swagger UI middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures global exception handling, Swagger UI, CORS, authentication, and API endpoints.
    /// </summary>
    public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 1. Global Exception Handling (Must be first to catch all pipeline errors)
        app.UseMiddleware<ExceptionMiddleware>();

        // 2. Swagger & Swagger UI (Enabled for Development & testing)
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing Software API v1");
                options.RoutePrefix = "swagger"; // Accessible at http://localhost:5213/swagger
                options.DocumentTitle = "Billing Software API Documentation";
                options.DisplayRequestDuration();
                options.EnablePersistAuthorization();
            });
        }

        // 3. HTTPS Redirection
        app.UseHttpsRedirection();

        // 4. Routing
        app.UseRouting();

        // 5. CORS (Placed before Authentication to handle browser preflight OPTIONS requests)
        app.UseCors(AppConstants.Cors.PolicyName);

        // 6. Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // 7. Controller Endpoint Mapping & Root Redirection
        app.UseEndpoints(endpoints =>
        {
            // Redirect root "/" directly to Swagger UI
            endpoints.MapGet("/", () => Results.Redirect("/swagger"));
            endpoints.MapControllers();
        });

        return app;
    }
}
