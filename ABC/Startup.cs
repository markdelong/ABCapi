using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ABC.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ABC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //MUST BE BEFORE services.addMvc();
            services.AddCors();

            // Auth----------
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {

                        // TODO: determine which of these to set
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = false,
                        ValidIssuer = "yourdomain.com",
                        ValidAudience = "yourdomain.com",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Key"]))
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TrainedStaffOnly",
                    policy => policy
                    .RequireClaim("CompletedBasicTraining"));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsLoggedIn",
                    policy => policy.RequireClaim("LoggedIn"));
                options.AddPolicy("IsVSMadmin",
                    policy => policy.RequireClaim("VSMadmin"));
            });

            services.AddMvc();

            ConnectionString.Credentials = Configuration["ConnectionStrings:ABC"];
            ConnectionString.VSMCredentials = Configuration["ConnectionStrings:VSM"];
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
