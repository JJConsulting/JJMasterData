namespace JJMasterData.ReceitaFederal.Models;

internal class SintegraCpfDto
{
    public string code { get; set; }
    public string status { get; set; }
    public string message { get; set; }
    public string cpf { get; set; }
    public string nome { get; set; }
    public string data_nascimento { get; set; }
    public string situacao_cadastral { get; set; }
    public string data_inscricao { get; set; }
    public string genero { get; set; }
    public string[] uf { get; set; }
    public string digito_verificador { get; set; }
    public string comprovante { get; set; }
    public string html { get; set; }
    public string version { get; set; }
}