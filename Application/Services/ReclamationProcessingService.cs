using Reclamacoes.Domain.Services;
using ReclamacoesBank.Application.Interfaces;
using ReclamacoesBank.Domain.Entites;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReclamacoesBank.Application.Services
{
    public class ReclamationProcessingService : IReclamationProcessingService
    {
        private readonly IReclamationRepository _repository;
        private readonly IDataMeshService _dataMeshService;
        private readonly INotificationService _notificationService;
        private readonly ClassificadorReclamacao _classifier;

        public ReclamationProcessingService(
            IReclamationRepository repository,
            IDataMeshService dataMeshService,
            INotificationService notificationService,
            ClassificadorReclamacao classifier)
        {
            _repository = repository;
            _dataMeshService = dataMeshService;
            _notificationService = notificationService;
            _classifier = classifier;
        }

        public async Task ProcessMessageAsync(string rawMessageBody)
        {
            var reclamation = DeserializeReclamation(rawMessageBody);

            var customerHistory = await _dataMeshService.FetchCustomerHistoryAsync(reclamation.CustomerIdentifier);

            reclamation.ClassifiedCategories = _classifier.Classify(reclamation);

            await _repository.SaveAsync(reclamation);

            await _notificationService.NotifyAsync(reclamation);
        }

        private Reclamation DeserializeReclamation(string rawMessageBody)
        {
            if (string.IsNullOrWhiteSpace(rawMessageBody))
            {
                throw new ArgumentException("Corpo da mensagem SQS está vazio e não pode ser desserializado.");
            }

            try
            {
                var reclamation = JsonSerializer.Deserialize<Reclamation>(
                    rawMessageBody,
                    new JsonSerializerOptions
                    {
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
                    }
                );

                if (reclamation == null || string.IsNullOrWhiteSpace(reclamation.ReclamationUserText))
                {
                    throw new JsonException("OS dados essenciais ReclamationText estão faltando ou são nulos.");
                }

                return reclamation;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Falha ao desserializar a mensagem SQS para Reclamation: {ex.Message}", ex);
            }
        }
    }
}
