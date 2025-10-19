using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Reclamacoes.Domain.Services;
using ReclamacoesBank.Application.Interfaces;
using ReclamacoesBank.Domain.Models;
using ReclamacoesBank.Infra.Clients;
using ReclamacoesBank.Infra.Persistence;
using ReclamacoesBank.Infra.Repositories;
using ReclamacoesBank.Infra.Services;
using ReclamacoesBank.Worker;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        //  1. CONFIGURAÇÃO DE LOGGING
        services.AddLogging(configure =>
            configure.AddConsole().SetMinimumLevel(LogLevel.Information));

        //  2. CONFIGURAÇÃO DO RDS
        var connectionString = hostContext.Configuration.GetConnectionString("ReclamacoesDb");
        services.AddDbContext<ReclamationContext>(options =>
            options.UseSqlServer(connectionString));

        //  3. CONFIGURAÇÃO DE CLIENTES HTTP NO Datamesh 
        services.AddHttpClient<IDataMeshService, DatameshClientService>(client =>
        {
            client.BaseAddress = new Uri(hostContext.Configuration["ApiSettings:DatameshApiUrl"] ?? throw new InvalidOperationException("Datamesh API URL não configurada."));
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        //  4. CONFIGURAÇÃO DO AWS (SQS/SNS) 
        services.AddAWSService<IAmazonSQS>();
        services.AddAWSService<IAmazonSimpleNotificationService>();

        //  5. REGISTRO DA LÓGICA DE NEGÓCIO
        var classificationRules = ClassificationRulesFactory.CreateDefaultRules();
        services.AddSingleton(classificationRules);
        services.AddSingleton<ClassificadorReclamacao>();
        services.AddScoped<IReclamationProcessingService, ReclamacoesBank.Application.Services.ReclamationProcessingService>();

        //  6. REGISTRO DA INFRAESTRUTURA
        services.AddScoped<IReclamationRepository, RdsReclamationRepository>();
        services.AddScoped<INotificationService, SnsNotificationService>();

        //  7. REGISTRO DO WORKER
        services.AddHostedService<SqsConsumerService>();
    });

var host = builder.Build();
await host.RunAsync();