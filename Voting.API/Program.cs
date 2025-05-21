using Voting.API.Middlewares;
using Voting.Infrastructure.Blockchain;
using Voting.Infrastructure.Data;
using Voting.Infrastructure.Services;
using Voting.Application;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services
    .AddApplicationServices(configuration)
    .AddDataServices(configuration)
    .AddBlockchainServices(configuration)
    .AddVotingApplication();


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
