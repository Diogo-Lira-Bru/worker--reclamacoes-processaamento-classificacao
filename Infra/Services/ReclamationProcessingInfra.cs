using Microsoft.Extensions.Logging;
using Reclamacoes.Domain.Services;
using ReclamacoesBank.Application.Interfaces;
using ReclamacoesBank.Domain.Entites;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReclamacoesBank.Infra.Services
{
    public class ReclamationProcessingInfra : IReclamationProcessingService
    {
        private readonly IReclamationRepository _repository;
        private readonly IDataMeshService _dataMeshService;
        private readonly INotificationService _notificationService;
        private readonly ClassificadorReclamacao _classifier;
        private readonly ILogger<ReclamationProcessingInfra> _logger;

        public ReclamationProcessingInfra(
            IReclamationRepository repository,
            IDataMeshService dataMeshService,
            INotificationService notificationService,
            ClassificadorReclamacao classifier,
            ILogger<ReclamationProcessingInfra> logger)
        {
            _repository = repository;
            _dataMeshService = dataMeshService;
            _notificationService = notificationService;
            _classifier = classifier;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(string rawMessageBody)
        {
            Reclamation reclamation = null;

            try
            {
                reclamation = DeserializeReclamation(rawMessageBody);
                _logger.LogInformation("Reclamação ID {ReclamationId} desserializada com sucesso. Cliente: {Customer}",
                    reclamation.Id, reclamation.CustomerIdentifier);

                var customerHistoryJson = await _dataMeshService.FetchCustomerHistoryAsync(reclamation.CustomerIdentifier);
                reclamation.CustomerHistory = customerHistoryJson;
                _logger.LogDebug("Histórico do cliente {Customer} anexado à reclamação ID {ReclamationId}",
                    reclamation.CustomerIdentifier, reclamation.Id);

                reclamation.ClassifiedCategories = _classifier.Classify(reclamation);
                _logger.LogInformation("Reclamação ID {ReclamationId} classificada. Categorias: {Categories}",
                    reclamation.Id, string.Join(", ", reclamation.ClassifiedCategories));

                await _repository.SaveAsync(reclamation);

                await _notificationService.NotifyAsync(reclamation);

                _logger.LogInformation("Processamento COMPLETO e bem-sucedido para a reclamação ID {ReclamationId}.", reclamation.Id);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "ERRO NO PROCESSAMENTO (Desserialização): Mensagem SQS não é um JSON válido. Body: {Body}",
                    rawMessageBody.Length > 200 ? rawMessageBody.Substring(0, 200) + "..." : rawMessageBody);
                throw;
            }
            catch (Exception ex) when (reclamation != null)
            {
                _logger.LogCritical(ex, "FALHA CRÍTICA no processamento da reclamação ID {ReclamationId}. O dado não foi salvo/notificado corretamente.", reclamation.Id);
                throw;
            }
        }

        private Reclamation DeserializeReclamation(string rawMessageBody)
        {
            if (string.IsNullOrWhiteSpace(rawMessageBody))
            {
                throw new ArgumentException("Corpo da mensagem SQS está vazio e não pode ser desserializado.");
            }

            var reclamation = JsonSerializer.Deserialize<Reclamation>(
                rawMessageBody,
                new JsonSerializerOptions
                {
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
                }
            );

            if (reclamation == null || string.IsNullOrWhiteSpace(reclamation.Id))
            {
                throw new JsonException("Desserialização bem-sucedida, mas ID da reclamação está ausente.");
            }

            return reclamation;
        }
    }
}
