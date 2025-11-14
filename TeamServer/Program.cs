using Microsoft.EntityFrameworkCore;
using TeamServer.Application.Services.AgentServices.AgentCore;
using TeamServer.Application.Services.AgentServices.AgentCRUD;
using TeamServer.Application.Services.ListenerServices.HttpListenerService.HttpCore;
using TeamServer.Application.Services.ListenerServices.HttpListenerService.HttpCRUD;
using TeamServer.Infrastructure.Data;

namespace TeamServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(o =>
            {
                o.ListenAnyIP(5225);              
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(o => 
                    o.UseSqlServer(builder.Configuration.GetConnectionString("TeamServerDb")));

            builder.Services.AddSingleton<IHttpCore, HttpCore>();
            builder.Services.AddScoped<IHttpCRUD, HttpCRUD>();

            builder.Services.AddScoped<IAgentCRUD, AgentCRUD>();
            builder.Services.AddScoped<IAgentCore, AgentCore>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
