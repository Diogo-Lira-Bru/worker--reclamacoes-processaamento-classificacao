using Microsoft.Extensions.Logging;
using ReclamacoesBank.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReclamacoesBank.Infra.Clients
{
    public class DatameshClientService : IDataMeshService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DatameshClientService> _logger;
        private readonly string _datameshApiUrl = "https://api.banco.com.br/datamesh/historico/";

        public DatameshClientService(
            HttpClient httpClient,
            ILogger<DatameshClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> FetchCustomerHistoryAsync(string customerIdentifier)
        {
            var endpointUrl = $"{_datameshApiUrl}{customerIdentifier}";

            _logger.LogInformation("Iniciando busca do Histórico do Cliente {Customer} na API Datamesh. URL: {Url}",
                customerIdentifier, endpointUrl);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _httpClient.GetAsync(endpointUrl);

                if (response.IsSuccessStatusCode)
                {
                    var historyJson = await response.Content.ReadAsStringAsync();
                    stopwatch.Stop();

                    _logger.LogInformation("Busca de histórico concluída com sucesso para {Customer}. Tempo de Latência: {ElapsedMs}ms",
                        customerIdentifier, stopwatch.ElapsedMilliseconds);

                    return historyJson;
                }
                else
                {
                    stopwatch.Stop();
                    _logger.LogError("Falha ao buscar histórico do cliente {Customer}. Status: {Status}, Latência: {ElapsedMs}ms",
                        customerIdentifier, response.StatusCode, stopwatch.ElapsedMilliseconds);

                    return "{}";
                }
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _logger.LogCritical(ex, "ERRO CRÍTICO: Falha de conexão/timeout com a API Datamesh para {Customer}. Latência até o erro: {ElapsedMs}ms",
                   customerIdentifier, stopwatch.ElapsedMilliseconds);
                return "{}";
            }
        }
    }
}
