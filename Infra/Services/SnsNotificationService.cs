using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using ReclamacoesBank.Application.Interfaces;
using ReclamacoesBank.Domain.Entites;
using System.Text.Json;

namespace ReclamacoesBank.Infra.Services
{
    public class SnsNotificationService : INotificationService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly ILogger<SnsNotificationService> _logger;
        private readonly string _snsTopicArn = "arn:aws:sns:us-east-1:123456789012:ReclamacoesFinalizadasTopic";

        public SnsNotificationService(
            IAmazonSimpleNotificationService snsClient,
            ILogger<SnsNotificationService> logger)
        {
            _snsClient = snsClient;
            _logger = logger;
        }

        public async Task NotifyAsync(Reclamation reclamation)
        {
            string messageJson;
            try
            {
                messageJson = JsonSerializer.Serialize(reclamation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO CRÍTICO: Falha ao serializar a reclamação {ReclamationId} para envio via SNS.", reclamation.Id);
                return;
            }

            var publishRequest = new PublishRequest
            {
                TopicArn = _snsTopicArn,
                Message = messageJson,
                MessageAttributes = new Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue>
                {
                    {
                        "Prioridade", new Amazon.SimpleNotificationService.Model.MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = reclamation.ClassifiedCategories.Contains("fraude") ? "ALTA" : "NORMAL"
                        }
                    },
                    {
                        "Categoria", new Amazon.SimpleNotificationService.Model.MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = reclamation.ClassifiedCategories.FirstOrDefault() ?? "Geral"
                        }
                    }
                }
            };

            try
            {
                var response = await _snsClient.PublishAsync(publishRequest);

                _logger.LogInformation(
                    "Reclamação {ReclamationId} publicada com sucesso no SNS. MessageId: {MessageId}",
                    reclamation.Id, response.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao notificar o SNS para a reclamação {ReclamationId}. O sistema legado pode não ter recebido a notificação.",
                    reclamation.Id);
            }
        }
    }
}
