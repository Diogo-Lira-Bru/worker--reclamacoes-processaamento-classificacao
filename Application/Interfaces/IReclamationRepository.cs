using ReclamacoesBank.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReclamacoesBank.Application.Interfaces
{
    public interface IReclamationRepository
    {
        Task SaveAsync(Reclamation reclamation);
    }
}
