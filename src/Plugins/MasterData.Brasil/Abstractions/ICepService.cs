using System.Threading.Tasks;
using JJMasterData.Brasil.Models;

namespace JJMasterData.Brasil.Abstractions;

public interface ICepService
{ 
    Task<CepResult> SearchCepAsync(string cep);
}