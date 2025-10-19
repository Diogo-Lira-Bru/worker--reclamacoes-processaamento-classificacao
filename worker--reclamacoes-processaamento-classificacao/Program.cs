using Amazon.Comprehend;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Reclamacoes.Domain.Services;
using ReclamacoesBank.Application.Interfaces;
using ReclamacoesBank.Application.Services;
using ReclamacoesBank.Domain.Models;
using ReclamacoesBank.Infra.Clients;
using ReclamacoesBank.Infra.Persistence;
using ReclamacoesBank.Infra.Repositories;
using ReclamacoesBank.Infra.Services;

namespace ReclamacoesBank.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // 1. CONFIGURAÇÃO DE LOGGING
                    services.AddLogging(configure =>
                        configure.AddConsole().SetMinimumLevel(LogLevel.Information));

                    // 2. CONFIGURAÇÃO DO RDS
                    var connectionString = hostContext.Configuration.GetConnectionString("ReclamacoesDb");

                    services.AddDbContext<ReclamationContext>(options =>
                        options.UseSqlServer(connectionString));

                    // 3. CONFIGURAÇÃO DE CLIENTES HTTP
                    services.AddHttpClient<IDataMeshService, DatameshClientService>(client =>
                    {
                        client.BaseAddress = new Uri(hostContext.Configuration["ApiSettings:DatameshApiUrl"] ?? throw new InvalidOperationException("Datamesh API URL não configurada."));
                        client.Timeout = TimeSpan.FromSeconds(5);
                    });

                    // 4. CONFIGURAÇÃO DO AWS SDK
                    services.AddAWSService<IAmazonSQS>();
                    services.AddAWSService<IAmazonSimpleNotificationService>();
                    services.AddAWSService<IAmazonComprehend>();

                    // 5. REGISTRO DA LÓGICA DE NEGÓCIO
                    var classificationRules = ClassificationRulesFactory.CreateDefaultRules();

                    services.AddSingleton(classificationRules);
                    services.AddSingleton<ClassificadorReclamacao>();
                    services.AddScoped<IComprehendClassifierService, ComprehendClassifierService>();
                    services.AddScoped<IReclamationProcessingService, ReclamationProcessingService>();

                    // 6. REGISTRO DA INFRAESTRUTURA
                    services.AddScoped<IReclamationRepository, RdsReclamationRepository>();
                    services.AddScoped<INotificationService, SnsNotificationService>();

                    // 7. REGISTRO DO WORKER
                    services.AddHostedService<SqsConsumerService>();
                });
    }
}