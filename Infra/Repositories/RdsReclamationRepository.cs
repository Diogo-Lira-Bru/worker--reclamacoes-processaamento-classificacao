using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReclamacoesBank.Application.Interfaces;
using ReclamacoesBank.Domain.Entites;
using ReclamacoesBank.Infra.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReclamacoesBank.Infra.Repositories
{
    public class RdsReclamationRepository : IReclamationRepository
    {
        private readonly ReclamationContext _context;
        private readonly ILogger<RdsReclamationRepository> _logger;

        public RdsReclamationRepository(
            ReclamationContext context,
            ILogger<RdsReclamationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveAsync(Reclamation reclamation)
        {
            _logger.LogInformation(
                "Iniciando a persistência da reclamação {ReclamationId} no RDS. Canal de Origem: {Channel}",
                reclamation.Id, reclamation.SourceChannel);

            try
            {
                if (await _context.Reclamations.FindAsync(reclamation.Id) != null)
                {
                    _logger.LogWarning("Reclamação já existe no banco de dados. Pulando a inserção.", reclamation.Id);
                    return;
                }

                await _context.Reclamations.AddAsync(reclamation);

                int rowsAffected = await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Persistência concluída com sucesso para a reclamação. Linhas afetadas: {Rows}",
                    reclamation.Id, rowsAffected);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "ERRO CRÍTICO: Falha ao salvar a reclamação {ReclamationId} no RDS. Isso pode indicar um gargalo na conexão ou no throughput do banco.",
                    reclamation.Id);
                throw;
            }
        }
    }
}
