// using JJMasterData.ReceitaFederal.Abstractions;
// using JJMasterData.ReceitaFederal.Services;
//
// namespace JJMasterData.ReceitaFederal.Test;
//
// public class ServicesHubTest
// {
//     private readonly IReceitaFederal _receitaFederal;
//
//     public ServicesHubTest(ServicesHubService hubService)
//     {
//         _receitaFederal = hubService;
//     }
//
//     [Theory]
//     [InlineData("18243793000181")]
//     public void ValidCnpj_Invalid(string cnpj)
//     {
//         var obj = _receitaFederal.SearchCnpj(cnpj);
//         Assert.Equal("NOK", obj.Return);
//     }
//
//     [Theory]
//     [InlineData("")]
//     [InlineData(null)]
//     public void ValidCnpj_NullArg(string cnpj)
//     {
//         var obj = _receitaFederal.SearchCnpj(cnpj);
//         Assert.Equal("ERROR", obj.Return);
//     }
//
//     [Theory]
//     [InlineData("08243793000181")]
//     [InlineData("08.243.793/0001-81")]
//     public void ValidCnpj_Ok(string cnpj)
//     {
//         var obj = _receitaFederal.SearchCnpj(cnpj);
//         Assert.Equal("OK", obj.Return);
//         Assert.Equal("JJ CONSULTING", obj.Result.Fantasia);
//         Assert.Equal("31/07/2006", obj.Result.Abertura);
//     }
//
//     [Theory]
//     [InlineData("12345678910", "1980-01-26")]
//     [InlineData("123", "26/01/1980")]
//     public void ValidCpf_Invalid(string cpf, string birth)
//     {
//         DateTime birthDate;
//         DateTime.TryParse(birth, out birthDate);
//
//         var obj = _receitaFederal.SearchCpf(cpf, birthDate);
//         Assert.Equal("NOK", obj.Return);
//     }
//
//     [Theory]
//     [InlineData("", "")]
//     [InlineData(null, null)]
//     public void ValidCpf_NullArg(string cpf, string birth)
//     {
//         DateTime birthDate;
//         DateTime.TryParse(birth, out birthDate);
//
//         var obj = _receitaFederal.SearchCpf(cpf, birthDate);
//         Assert.Equal("ERROR", obj.Return);
//     }
//
//     [Theory]
//     [InlineData("29303477839", "1980-01-26")]
//     [InlineData("293.034.778-39", "1980-01-26")]
//     public void ValidCpf_Ok(string cpf, string birth)
//     {
//         DateTime birthDate;
//         DateTime.TryParse(birth, out birthDate);
//
//         var obj = _receitaFederal.SearchCpf(cpf, birthDate);
//         Assert.Equal("OK", obj.Return);
//         Assert.Contains("LUCIO", obj.Result.Nome_Da_Pf);
//     }
//
//     [Theory]
//     [InlineData("P2016040")]
//     public void ValidCep_Invalid(string cep)
//     {
//         var obj = _receitaFederal.SearchCep(cep);
//         Assert.Equal("ERROR", obj.Return);
//     }
//
//     [Theory]
//     [InlineData("")]
//     [InlineData(null)]
//     public void ValidCep_NullArg(string cep)
//     {
//         var obj = _receitaFederal.SearchCep(cep);
//         Assert.Equal("ERROR", obj.Return);
//     }
//
//     [Theory]
//     [InlineData("02016040")]
//     [InlineData("02.016-040")]
//     public void ValidCep_Ok(string cep)
//     {
//         var obj = _receitaFederal.SearchCep(cep);
//         Assert.Equal("OK", obj.Return);
//         Assert.Contains("BENEVOLO", obj.Result.Logradouro.ToUpper());
//     }
//
// }