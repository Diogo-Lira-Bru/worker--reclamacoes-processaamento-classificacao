using Microsoft.Extensions.Logging;
using Moq;
using Reclamacoes.Domain.Services;
using ReclamacoesBank.Application.Interfaces;
using ReclamacoesBank.Domain.Entites;
using ReclamacoesBank.Infra.Services;

namespace Test.ServiceTests
{
    public class ReclamationProcessingServiceTests
    {
        private readonly Mock<IReclamationRepository> _mockRepository = new();
        private readonly Mock<IDataMeshService> _mockDataMeshService = new();
        private readonly Mock<INotificationService> _mockNotificationService = new();
        private readonly Mock<ILogger<ReclamationProcessingInfra>> _mockLogger = new();

        private readonly ClassificadorReclamacao _classifier;

        public ReclamationProcessingServiceTests()
        {
            var rules = ReclamacoesBank.Domain.Models.ClassificationRulesFactory.CreateDefaultRules();
            var mockClassifierLogger = new Mock<ILogger<ClassificadorReclamacao>>();
            _classifier = new ClassificadorReclamacao(rules, mockClassifierLogger.Object);
        }

        [Fact]
        public async Task ProcessMessageAsync_DeveExecutarOFluxoCompleto_ComSucesso()
        {
            // ARRANGE

            var inputJson = @"{
            ""Id"": ""123-abc"",
            ""CustomerIdentifier"": ""99988877766"",
            ""ReclamationUserText"": ""Estou com problemas para acessar minha conta e o aplicativo está travando muito."",
            ""ReceivedDate"": ""2025-01-01T12:00:00Z"",
            ""SourceChannel"": ""Digital""
            }";

            var mockHistoryJson = "{ \"history_count\": 5, \"last_product\": \"seguro\" }";
            _mockDataMeshService.Setup(s =>
                s.FetchCustomerHistoryAsync(It.IsAny<string>()))
                .ReturnsAsync(mockHistoryJson);

            Reclamation? savedReclamation = null;
            _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Reclamation>()))
                           .Callback<Reclamation>(r => savedReclamation = r)
                           .Returns(Task.CompletedTask);

            var service = new ReclamationProcessingInfra(
                _mockRepository.Object,
                _mockDataMeshService.Object,
                _mockNotificationService.Object,
                _classifier,
                _mockLogger.Object
            );

            // ACT
            await service.ProcessMessageAsync(inputJson);

            // ASSERT
            _mockDataMeshService.Verify(s =>
                s.FetchCustomerHistoryAsync("99988877766"), Times.Once);

            _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Reclamation>()), Times.Once);

            Assert.NotNull(savedReclamation);
            Assert.Contains("acesso", savedReclamation!.ClassifiedCategories);
            Assert.Contains("aplicativo", savedReclamation.ClassifiedCategories);
            Assert.Equal(mockHistoryJson, savedReclamation.CustomerHistory);

            _mockNotificationService.Verify(n =>
                n.NotifyAsync(It.IsAny<Reclamation>()), Times.Once);

            _mockLogger.Verify(l =>
                l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtLeast(3));
        }
    }
}