using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReclamacoesBank.Application.Interfaces
{
    public interface IDataMeshService
    {
        Task<string> FetchCustomerHistoryAsync(string customerIdentifier);
    }
}
