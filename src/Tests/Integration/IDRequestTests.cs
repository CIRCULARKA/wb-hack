using WBBasket.Core;

using Xunit;

namespace WBBasket.Tests.Integration;

[Trait("Type", "Integration")]
public class IDRequestTests
{
    [Theory]
    [InlineData("93550775")]
    public async Task Execute_WhenCalled_AcquiresIDFromExistingProduct(string vendorCode)
    {
        // Arrange
        var r = new IDRequest(vendorCode);
        foreach (var uri in r.PossibleURIs)
            Console.WriteLine(uri.ToString());

        // Act
        var result = await r.Execute();

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("93550775")]
    private async Task AcquiredID_AfterSuccessfullExecute_ObtainsResultWithoutRequest(string vendorCode)
    {
        // Arrange
        var r = new IDRequest(vendorCode);

        // Act
        var expectedValue = await r.Execute();

        // Will fail at runtime if requester will try to make actual
        // requests to services again. That will give us know that caching isn't working
        for (int i = 0; i < r.PossibleURIs.Length; i++)
            r.PossibleURIs[i] = null;

        var actualValue = r.AcquiredID;

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }
}
