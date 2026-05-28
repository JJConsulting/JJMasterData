

using JJMasterData.Commons.Validations;

namespace JJMasterData.Commons.Test.Validations;

public class ValidationsTest
{
    [Theory]
    [InlineData("@@@@@@@@@@@@77")]
    [InlineData("A1B2C3D4E5F6-8")]
    [InlineData("A1B2C3D4E5F6AA")]
    [InlineData("A1B2C3D4E5F@67")]
    [InlineData("V5.ZCD.2ML/0001-78")]
    [InlineData("12.325.678/0001-95")]
    public void ValidateCnpj_WithInvalidAlphanumericPattern_ReturnsFalse(string cnpj)
    {
        Assert.False(BrazilValidations.ValidateCnpj(cnpj));
    }

    [Theory]
    [InlineData("08.243.793/0001-81")]
    [InlineData("11.222.333/0001-81")]
    [InlineData("12.345.678/0001-95")]
    [InlineData("00.000.000/0001-91")]
    [InlineData("V5.ZCD.XML/0001-78")]
    [InlineData("A1B2C3D4E5F668")]
    [InlineData("12ABC34501DE35")]
    [InlineData("abcdefghijkl80")]
    public void ValidateCnpj_WithValidPatternAndVerificationDigits_ReturnsTrue(string cnpj)
    {
        Assert.True(BrazilValidations.ValidateCnpj(cnpj));
    }
}
