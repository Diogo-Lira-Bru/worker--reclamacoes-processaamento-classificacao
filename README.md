🛠️ worker--reclamacoes-processamento-classificacao
Serviço responsável por processar, interpretar e classificar automaticamente reclamações recebidas por canais digitais e físicos, garantindo integração com sistemas legados, rastreabilidade e visibilidade via portal interno.

📌 Propósito
Automatizar o tratamento de reclamações de clientes, padronizando dados de entrada, extraindo informações relevantes e classificando demandas de forma inteligente e escalável.

🧩 Visão Geral da Arquitetura
O serviço está inserido em uma arquitetura distribuída baseada em componentes da AWS e microsserviços. Abaixo, o fluxo principal:

Entrada de reclamações via:
Aplicativos digitais
Agências físicas (documentos digitalizados)
Amazon API Gateway recebe as requisições
Amazon S3 armazena documentos e dados legados

Lambda Functions realizam:
Triagem digital
Enriquecimento físico (OCR, extração)
Amazon Comprehend interpreta o conteúdo textual das reclamações
AWS EKS (.NET Worker) executa a lógica de classificação e persistência
Amazon RDS armazena os dados processados
Portal Interno exibe os dados para usuários internos
API de Dados fornece histórico e datasheets
CheckWatch monitora o SLA e gera alertas

⚙️ Funcionalidades do Worker
Recebe reclamações padronizadas

Interpreta conteúdo textual com Amazon Comprehend:
Extração de entidades (nomes, datas, valores)
Identificação de canal (site ou físico)
Análise de sentimento

Classifica automaticamente por tipo de demanda usando palavras-chave e IA
Persiste dados em RDS
Gera eventos para sistemas legados
Expõe dados para o portal interno e API

🧠 Classificação Inteligente
A classificação é feita com base em palavras-chave e análise semântica. Exemplo:

json
{
  "reclamacao": "Estou com problemas para acessar minha conta e o aplicativo está travando muito.",
  "categorias": {
    "acesso": ["acessar", "login", "senha"],
    "aplicativo": ["app", "aplicativo", "travando", "erro"]
  }
}
Resultado: ["acesso", "aplicativo"]

🧱 Arquitetura de Software
O projeto adota Clean Architecture, com separação clara entre:

Entidades: Modelos de domínio
Aplicação: Casos de uso e orquestração
Infraestrutura: Repositórios, integração com AWS e sistemas legados
Apresentação: Interfaces e APIs (quando aplicável)

📁 Estrutura de Diretórios
Código
worker--reclamacoes-processamento-classificacao/
├── Entidades/
├── Aplicacao/
├── Dominio/
├── Infraestrutura/
├── Testes/
│   ├── Aplicacao/
│   └── Infraestrutura/
└── Diagramas/

🧪 Testes e Monitoramento
Testes unitários e de integração
Simulações de SLA com dados sintéticos
Monitoramento com CheckWatch

Alertas para:
Falhas de processamento
Reclamações próximas do vencimento

🧰 Tecnologias Utilizadas
Camada	Tecnologia	Justificativa
Backend	.NET Worker (C#)	Performance e integração com legado
Orquestração	AWS EKS (Kubernetes)	Escalabilidade e resiliência
NLP	Amazon Comprehend	Interpretação textual e análise semântica
Armazenamento	Amazon RDS (PostgreSQL)	Persistência relacional confiável
Eventos	Amazon SQS / SNS	Comunicação assíncrona
Monitoramento	CheckWatch + Grafana	Visibilidade e alertas

🔐 Requisitos Não Funcionais
Escalabilidade para 1.000+ reclamações/dia
Rastreabilidade completa do fluxo
Segurança de dados sensíveis
Absorção de dados legados
SLA de 10 dias com alertas proativos

🤖 Como a IA acelera o processo
O uso do Amazon Comprehend permite:
Redução do tempo de triagem manual
Extração automática de dados relevantes
Classificação mais precisa e contextual
Identificação do canal de origem
Geração de insights para priorização de atendimento
