using Microsoft.Extensions.Logging;
using ReclamacoesBank.Domain.Entites;
using ReclamacoesBank.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reclamacoes.Domain.Services
{
    public class ClassificadorReclamacao
    {
        private readonly List<ClassificationRule> _rules;
        private readonly ILogger<ClassificadorReclamacao> _logger;

        public ClassificadorReclamacao(
            List<ClassificationRule> rules,
            ILogger<ClassificadorReclamacao> logger)
        {
            _rules = rules;
            _logger = logger;
            _logger.LogInformation("Classificador de Reclamações inicializado com {RuleCount} regras.", _rules.Count);
        }

        public List<string> Classify(Reclamation reclamation)
        {
            _logger.LogInformation("Iniciando classificação da reclamação ID: {ReclamationId}", reclamation.Id);

            var detectedCategories = new List<string>();

            if (string.IsNullOrWhiteSpace(reclamation.ReclamationUserText))
            {
                _logger.LogWarning("Texto da reclamação ID: {ReclamationId} está vazio. Retornando 'Vazia'.", reclamation.Id);
                return new List<string> { "Não Classificada - Vazia" };
            }

            var cleanedText = reclamation.ReclamationUserText.ToLowerInvariant();
            //limpa as palavras para serem comparadas com a lista de palavras chave
            var tokens = Regex.Replace(cleanedText, "[^a-zA-Z0-9áéíóúãõâêîôûç ]", "")
                             .Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                             .Distinct()
                             .ToList();

            _logger.LogDebug("Texto processado em {TokenCount} tokens. Tokens principais: {Tokens}",
                             tokens.Count, string.Join(", ", tokens.Take(10)));


            foreach (var rule in _rules)
            {
                var matchingKeywords = rule.Keywords
                    .Intersect(tokens, System.StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (matchingKeywords.Any())
                {
                    detectedCategories.Add(rule.CategoryName);
                    _logger.LogInformation("Categoria '{Category}' detectada. Palavra-chave acionadora: '{Keyword}'",
                                           rule.CategoryName, matchingKeywords.First());
                }
            }

            if (detectedCategories.Contains("fraude"))
            {
                _logger.LogCritical("CLASSIFICAÇÃO FINAL: FRAUDE detectada. Forçando escalonamento imediato e anulando outras categorias.", reclamation.Id);
                return new List<string> { "fraude" };
            }

            if (!detectedCategories.Any())
            {
                detectedCategories.Add("Geral");
                _logger.LogInformation("Nenhuma categoria específica detectada para ID: {ReclamationId}. Classificando como 'Geral'.", reclamation.Id);
            }

            var finalCategories = detectedCategories.Distinct().ToList();
            _logger.LogInformation("Classificação final para ID: {ReclamationId}: {Categories}",
                                   reclamation.Id, string.Join(", ", finalCategories));

            return finalCategories;
        }
    }
}