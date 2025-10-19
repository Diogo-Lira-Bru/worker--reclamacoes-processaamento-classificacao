namespace ReclamacoesBank.Domain.Models
{
    public static class ClassificationRulesFactory
    {
        public static List<ClassificationRule> CreateDefaultRules()
        {
            return new List<ClassificationRule>
            {
                new ClassificationRule {
                    CategoryName = "imobiliário",
                    Keywords = new List<string> { "credito", "imobiliario", "casa", "apartamento" }
                },
                new ClassificationRule {
                    CategoryName = "seguros",
                    Keywords = new List<string> { "resgate", "capitalizacao", "socorro" }
                },
                new ClassificationRule {
                    CategoryName = "cobrança",
                    Keywords = new List<string> { "fatura", "cobrança", "valor", "indevido" }
                },
                new ClassificationRule {
                    CategoryName = "acesso",
                    Keywords = new List<string> { "acessar", "login", "senha" }
                },
                new ClassificationRule {
                    CategoryName = "aplicativo",
                    Keywords = new List<string> { "app", "aplicativo", "travando", "erro" }
                },
                new ClassificationRule {
                    CategoryName = "fraude",
                    Keywords = new List<string> { "fatura", "nao", "reconhece", "divida", "fraude" }
                }
            };
        }
    }
}
