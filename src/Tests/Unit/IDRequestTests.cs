using System.ComponentModel.DataAnnotations;
using WBBasket.Core;

using Xunit;

namespace WBBasket.Tests.Unit;

[Trait("Type", "Unit")]
public class IDRequestTests
{
    [Theory]
    [InlineData("12345")]
    [InlineData("1234567891111")]
    [InlineData("12346s7")]
    [InlineData("hellowr")]
    [InlineData(null)]
    public void Ctor_VendorCodeIsNullOrConsistsNotOnlyOfDigitsOrLengthIsLessThan6AndMoreThan12_Throws(string invalidCode)
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new IDRequest(invalidCode));
    }

    [Fact]
    public void Ctor_BasketInstancesIsBelow1AndMoreThan99_Throws()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() => new IDRequest("1234567", 0));
        Assert.Throws<InvalidOperationException>(() => new IDRequest("1234567", 100));
    }

    [Theory]
    [InlineData("10045678", 10)]
    [InlineData("10045678", 5)]
    [InlineData("10045678", 1)]
    [InlineData("10045678", 3)]
    public void PossibleURIs_AfterConstruction_BuildExpectedPossibleURIsCount(
        string vendorCode, int maxBasketInstances
    )
    {
        // Arrange
        var r = new IDRequest(vendorCode, (uint)maxBasketInstances);

        // Act
        var actualUriCount = r.PossibleURIs.Length;

        // Assert
        Assert.Equal(maxBasketInstances, actualUriCount);
    }

    [Fact]
    public void PossibleUris_AfterConstruction_ReflectsCalculatedInstanceNumberInTwoDigitFormat()
    {
        // Arrange
        var maxBasketInstances = 16u;
        var r = new IDRequest("12367899", maxBasketInstances);
        var expectedUriCount = maxBasketInstances;

        // Act
        var actualUris = r.PossibleURIs;

        // Assert
        for (int i = 0; i < maxBasketInstances; i++)
            Assert.Contains($"-{(i + 1).ToString("00")}", actualUris[i].ToString());
    }

    [Theory]
    [InlineData("123456", "vol1/part123/123456")]
    [InlineData("1234567", "vol12/part1234/1234567")]
    [InlineData("12345678", "vol123/part12345/12345678")]
    [InlineData("123456789", "vol1234/part123456/123456789")]
    public void PossibleURIsPath_AfterConstruction_UsesVendorCodeInUriProperly(string code, string expectedPath)
    {
        // Act
        var r = new IDRequest(code);

        // Assert
        Assert.Contains(expectedPath, r.PossibleURIs[0].PathAndQuery);
    }

    [Fact]
    public void VendorCode_AfterConstruction_EqualsToConstructorParameter()
    {
        // // Arrange
        var expectedCode = "1234567";

        // // Act
        var r = new IDRequest(expectedCode);

        // // Assert
        Assert.Equal(expectedCode, r.VendorCode);
    }

    [Fact]
    public void AcquiredID_ByDefault_Null()
    {
        // // Act & Arrange
        var r = new IDRequest("123456");

        // // Assert
        Assert.Null(r.AcquiredID);
    }

    // TODO: insert interaction tests using mocked HttpClient to test the following:
    //     Is client's "Send" method called all possible times if none of requests are not successfull?
    //     Is client's "Send" method called only specific amount of times before successfull request?
    //     Other interation things. For now, all of the above is tested only in integration tests
}
