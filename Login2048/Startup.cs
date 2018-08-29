    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login2048.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Login2048
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
            services.AddMvc();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENV") == "Production")
            {
                services.AddDbContext<UserContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            }
            else
            {
                services.AddDbContext<UserContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("UserDb")));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
