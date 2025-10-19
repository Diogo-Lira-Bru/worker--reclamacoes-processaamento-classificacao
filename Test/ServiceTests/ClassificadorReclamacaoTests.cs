using Microsoft.Extensions.Logging;
using Moq;
using Reclamacoes.Domain.Services;
using ReclamacoesBank.Domain.Entites;
using ReclamacoesBank.Domain.Models;
namespace Test.ServiceTests
{
    public class ClassificadorReclamacaoTests
    {
        private readonly ClassificadorReclamacao _classifier;

        public ClassificadorReclamacaoTests()
        {
            var rules = ClassificationRulesFactory.CreateDefaultRules();
            var mockLogger = new Mock<ILogger<ClassificadorReclamacao>>();

            _classifier = new ClassificadorReclamacao(rules, mockLogger.Object);
        }

        [Fact]
        public void Classify_DeveRetornarImobiliario_QuandoContemApartamento()
        {
            // ARRANGE
            var reclamation = new Reclamation
            {
                ReclamationUserText = "Estou com problemas no meu crédito imobiliário e no meu apartamento novo."
            };

            // ACT
            var result = _classifier.Classify(reclamation);

            // ASSERT
            Assert.Single(result);
            Assert.Equal("imobiliário", result.First());
        }

        [Fact]
        public void Classify_DeveRetornarSeguros_QuandoContemResgate()
        {
            // ARRANGE
            var reclamation = new Reclamation
            {
                ReclamationUserText = "Quero fazer o resgate da minha capitalização de seguro de vida."
            };

            // ACT
            var result = _classifier.Classify(reclamation);

            // ASSERT
            Assert.Single(result);
            Assert.Equal("seguros", result.First());
        }

        [Fact]
        public void Classify_DeveRetornarCobranca_QuandoContemIndevido()
        {
            // ARRANGE
            var reclamation = new Reclamation
            {
                ReclamationUserText = "Recebi um valor indevido na minha conta e uma cobrança extra."
            };

            // ACT
            var result = _classifier.Classify(reclamation);

            // ASSERT
            Assert.Equal("cobrança", result.First());
            Assert.DoesNotContain("fraude", result);
        }

        [Fact]
        public void Classify_DeveRetornarAcesso_QuandoContemSenha()
        {
            // ARRANGE
            var reclamation = new Reclamation
            {
                ReclamationUserText = "Não consigo fazer o login porque esqueci minha senha para acessar."
            };

            // ACT
            var result = _classifier.Classify(reclamation);

            // ASSERT
            Assert.Equal("acesso", result.First());
        }

        [Fact]
        public void Classify_DeveRetornarFraude_EAnularOutras_QuandoDetectaFraude()
        {
            // ARRANGE
            var reclamation = new Reclamation
            {
                ReclamationUserText = "Recebi uma fatura que eu não reconheço, isso é fraude!"
            };

            // ACT
            var result = _classifier.Classify(reclamation);

            // ASSERT
            Assert.Single(result);
            Assert.Equal("fraude", result.First());
        }

        [Fact]
        public void Classify_DeveRetornarAcessoEAplicativo_EmCasoDeAmbiguidadeNormal()
        {
            // ARRANGE
            var reclamation = new Reclamation
            {
                ReclamationUserText = "Não consigo acessar o app porque ele está travando."
            };

            // ACT
            var result = _classifier.Classify(reclamation);

            // ASSERT
            Assert.Contains("acesso", result);
            Assert.Contains("aplicativo", result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Classify_DeveRetornarGeral_QuandoNenhumaPalavraChaveEncontrada()
        {
            // ARRANGE
            var reclamation = new Reclamation
            {
                ReclamationUserText = "Apenas uma observação sobre o atendimento excelente que recebi."
            };

            // ACT
            var result = _classifier.Classify(reclamation);

            // ASSERT
            Assert.Single(result);
            Assert.Equal("Geral", result.First());
        }
    }
}
