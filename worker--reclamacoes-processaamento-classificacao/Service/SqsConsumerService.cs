using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReclamacoesBank.Infra.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReclamacoesBank.Worker
{
    public class SqsConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly string _highPriorityQueueUrl = "SQS_ALTA_PRIORIDADE_URL";
        private readonly string _centralQueueUrl = "SQS_CENTRAL_URL";

        public SqsConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessQueueAsync(_highPriorityQueueUrl, stoppingToken);

                await ProcessQueueAsync(_centralQueueUrl, stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessQueueAsync(string queueUrl, CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var sqsClient = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();
                var processingService = scope.ServiceProvider.GetRequiredService<ReclamationProcessingInfra>();

                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 5
                };

                var receiveResponse = await sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

                foreach (var message in receiveResponse.Messages)
                {
                    try
                    {
                        await processingService.ProcessMessageAsync(message.Body);

                        await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("A mensagem não foi deletada devido a falhas, voltando para a fila para nova tentativa");
                    }
                }
            }
        }
    }
}