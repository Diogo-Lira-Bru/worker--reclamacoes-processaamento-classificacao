using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Reclamacoes.Domain.Services;
using ReclamacoesBank.Domain.Entites;

namespace ReclamacoesBank.Infra.Services
{
    public class ComprehendClassifierService : IComprehendClassifierService
    {
        private readonly IAmazonComprehend _comprehendClient;
        private readonly ILogger<ComprehendClassifierService> _logger;
        private readonly ClassificadorReclamacao _manualClassifier;
        private readonly string _customEndpointArn = "ARN_DO_SEU_ENDPOINT_COMPREHEND";

        public ComprehendClassifierService(
            IAmazonComprehend comprehendClient,
            ILogger<ComprehendClassifierService> logger,
            ClassificadorReclamacao manualClassifier)
        {
            _comprehendClient = comprehendClient;
            _logger = logger;
            _manualClassifier = manualClassifier;
        }

        public async Task<List<string>> ClassifyTextAsync(Reclamation reclamation)
        {
            _logger.LogInformation("Chamando o Amazon Comprehend para classificar a reclamação ID: {Id}", reclamation.Id);

            var request = new ClassifyDocumentRequest
            {
                Text = reclamation.ReclamationUserText,
                EndpointArn = _customEndpointArn
            };

            try
            {
                var response = await _comprehendClient.ClassifyDocumentAsync(request);

                var categories = response.Classes
                    .Where(c => c.Score > 0.8)
                    .Select(c => c.Name)
                    .ToList();

                _logger.LogInformation("Comprehend retornou {Count} categorias. ID: {Id}", categories.Count, reclamation.Id);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha CRÍTICA ao chamar o Amazon Comprehend para ID {Id}. Iniciando modo de FALLBACK (Classificação Manual).", reclamation.Id);

                return _manualClassifier.Classify(reclamation);
            }
        }
    }
}