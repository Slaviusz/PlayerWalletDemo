using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace PlayerWalletAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.AllowTrailingCommas = true;
                });

            // SQlite DbContext
            services.AddDbContextPool<PlayerWalletContext.PlayerWalletContext>(options =>
                options.UseSqlite(PlayerWalletContext.PlayerWalletContext.SqliteConnectionString));

            // Automapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "PlayerWallet API", Version = "v1"});
                c.EnableAnnotations();

                // Set the comments path for the Swagger JSON and UI.
                var apiXmlPath = Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
                );
                c.IncludeXmlComments(apiXmlPath);

                // Uses full schema names to avoid v1/v2/v3 schema collisions
                // see: https://github.com/domaindrivendev/Swashbuckle/issues/442
                c.CustomSchemaIds(x => x.FullName);
            });

            //services.AddSwaggerExamplesFromAssemblyOf<Program>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // configuration for deployment behind application proxy server (nginx, apache, IIS, etc.)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // disable HTTPS redirection in service oriented architecture
            // there probably is probably an application proxy server in front of
            //app.UseHttpsRedirection();

            app.UseRouting();

            // Disable Authorization in service-to-service interfaces
            //app.UseAuthorization();

            // Swagger
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlayerWallet API v1");
                c.DisplayOperationId();
                c.DisplayRequestDuration();
            });

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            // Initial database seed
            DatabaseInitialSeed(app);
        }

        private static void DatabaseInitialSeed(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<PlayerWalletContext.PlayerWalletContext>();
            dbContext.Database.EnsureCreated();
        }
    }
}
