using JJMasterData.Commons.Validations;

namespace JJMasterData.Commons.Test.Validations;

public class ValidateTest
{
    [Theory]
    [InlineData("19.131.243/0001-97")]
    [InlineData("7G.9IZ.AVN/DLVH-20")]
    public void ValidCnpj_ShouldAcceptNumericAndAlphanumericValues(string input)
    {
        Assert.True(Validate.ValidCnpj(input));
    }

    [Theory]
    [InlineData("19.131.243/0001-00")]
    [InlineData("7G.9IZ.AVN/DLVH-00")]
    [InlineData("7G.9IZ.AVN/DLVH-AA")]
    public void ValidCnpj_ShouldRejectInvalidValues(string input)
    {
        Assert.False(Validate.ValidCnpj(input));
    }
}
