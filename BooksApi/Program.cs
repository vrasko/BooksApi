using BooksApi;
using BooksApi.Data;
using BooksApi.DbAccess;
using BooksApi.Helpers;
using Serilog;

internal class Program
{
  private static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    //Logger Serilog
    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((context, loggerconfig) => loggerconfig.ReadFrom.Configuration(context.Configuration));//read configuration from appsettings.json
    builder.Services.Configure<ConfigEmailModel>(builder.Configuration.GetSection("SmtpSettings"));

    // Add services to the container.

    //builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSingleton<ISqliteDbAccess, SqliteDbAccess>(); //injection - interface, class
    builder.Services.AddSingleton<IDataTasks, DataTasks>();
    builder.Services.AddTransient<EmailSender>();
    //bezpecne pripojenie api kvoli zakazu - bezpecnostnej politiky CORS problem
    builder.Services.AddCors(options =>
    {
      options.AddPolicy("CorsAllowAll", builder =>
      {
        builder.AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowAnyHeader();
      });
    });
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    //bezpecne pripojenie api kvoli zakazu - bezpecnostnej politiky CORS problem 
    app.UseCors("CorsAllowAll"); 

    //app.UseAuthorization();
    //app.MapControllers();
    app.ConfigureApi();
    app.Run();
  }
}