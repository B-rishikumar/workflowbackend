using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WorkflowManagement.API.Middleware;
using WorkflowManagement.Infrastructure.Data.Context;
using WorkflowManagement.Infrastructure.Data.Seed;
using WorkflowManagement.Application.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace WorkflowManagement.API.Extensions
{
    /// <summary>
    /// Extension methods for IApplicationBuilder to configure the request pipeline
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure the application pipeline with all necessary middleware
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The web host environment</param>
        /// <returns>The configured application builder</returns>
        public static IApplicationBuilder UseWorkflowManagementPipeline(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure pipeline based on environment
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Management API V1");
                    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    c.ShowExtensions();
                    c.EnableValidator();
                    c.SupportedSubmitMethods(Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, 
                                           Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post,
                                           Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put,
                                           Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete);
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Security headers
            app.UseSecurityHeaders();

            // HTTPS redirection
            app.UseHttpsRedirection();

            // Request logging
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.GetLevel = (httpContext, elapsed, ex) => ex != null ? Serilog.Events.LogEventLevel.Error : Serilog.Events.LogEventLevel.Information;
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
                };
            });

            // Custom middleware
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<RateLimitingMiddleware>();

            // CORS
            app.UseCors("WorkflowManagementPolicy");

            // Static files (if needed)
            app.UseStaticFiles();

            // Routing
            app.UseRouting();

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Health checks
            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = WriteHealthCheckResponse
            });

            // API endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            return app;
        }

        /// <summary>
        /// Configure security headers
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The configured application builder</returns>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // X-Frame-Options
                context.Response.Headers.Add("X-Frame-Options", "DENY");

                // X-Content-Type-Options
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                // X-XSS-Protection
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

                // Referrer-Policy
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

                // Content-Security-Policy
                context.Response.Headers.Add("Content-Security-Policy", 
                    "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'");

                // Permissions-Policy
                context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

                await next();
            });

            return app;
        }

        /// <summary>
        /// Initialize and seed the database
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The configured application builder</returns>
        public static async Task<IApplicationBuilder> InitializeDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Initializing database...");

                var context = services.GetRequiredService<WorkflowDbContext>();
                
                // Apply pending migrations
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    logger.LogInformation("Applying pending database migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully");
                }

                // Seed initial data
                var dataSeeder = services.GetRequiredService<DataSeeder>();
                await dataSeeder.SeedAsync();

                logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }

            return app;
        }

        /// <summary>
        /// Configure global exception handling
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The configured application builder</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;

                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exception, "Unhandled exception occurred");

                    var response = ResponseDto<object>.Failure("An internal server error occurred");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var json = JsonSerializer.Serialize(response, options);
                    await context.Response.WriteAsync(json);
                });
            });

            return app;
        }

        /// <summary>
        /// Configure API versioning
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The configured application builder</returns>
        public static IApplicationBuilder UseApiVersioning(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // Add API version to response headers
                context.Response.Headers.Add("API-Version", "1.0");
                context.Response.Headers.Add("API-Supported-Versions", "1.0");
                
                await next();
            });

            return app;
        }

        /// <summary>
        /// Configure request/response compression
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The configured application builder</returns>
        public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var originalBodyStream = context.Response.Body;

                try
                {
                    var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString();
                    
                    if (acceptEncoding.Contains("gzip") && context.Response.ContentType?.Contains("application/json") == true)
                    {
                        context.Response.Headers.Add("Content-Encoding", "gzip");
                    }

                    await next();
                }
                finally
                {
                    context.Response.Body = originalBodyStream;
                }
            });

            return app;
        }

        /// <summary>
        /// Configure request timeout
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="timeoutMinutes">Timeout in minutes</param>
        /// <returns>The configured application builder</returns>
        public static IApplicationBuilder UseRequestTimeout(this IApplicationBuilder app, int timeoutMinutes = 5)
        {
            app.Use(async (context, next) =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(timeoutMinutes));
                context.RequestAborted = cts.Token;

                try
                {
                    await next();
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    await context.Response.WriteAsync("Request timeout");
                }
            });

            return app;
        }

        /// <summary>
        /// Configure maintenance mode
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="isMaintenanceMode">Whether maintenance mode is enabled</param>
        /// <returns>The configured application builder</returns>
        public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder app, bool isMaintenanceMode = false)
        {
            if (isMaintenanceMode)
            {
                app.Use(async (context, next) =>
                {
                    // Skip maintenance mode for health checks
                    if (context.Request.Path.StartsWithSegments("/health"))
                    {
                        await next();
                        return;
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.ContentType = "application/json";

                    var response = ResponseDto<object>.Failure("Service is temporarily unavailable for maintenance");
                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await context.Response.WriteAsync(json);
                });
            }

            return app;
        }

        /// <summary>
        /// Write custom health check response
        /// </summary>
        private static async Task WriteHealthCheckResponse(Microsoft.AspNetCore.Http.HttpContext context, 
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = result.Status.ToString(),
                checks = result.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception?.Message,
                    duration = entry.Value.Duration.ToString(),
                    data = entry.Value.Data
                }),
                duration = result.TotalDuration.ToString()
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}