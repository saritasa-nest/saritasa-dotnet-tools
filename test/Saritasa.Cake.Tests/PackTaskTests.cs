namespace Saritasa.Cake.Tests;

public class PackTaskTests
{
    private const string TestAssemblyInfo = """
        [assembly: AssemblyVersion("1.0.0.0")]
        [assembly: AssemblyFileVersion("1.0.0.0")]
        [assembly: AssemblyInformationalVersion("1.0.0.0")]
        """;

    [Fact]
    public void Replace_AssemblyVersionAttribute_ShouldReplace()
    {
        // Arrange
        var packTask = new PackTask();
        var expectedAssemblyInfo = """
            [assembly: AssemblyVersion("5.0.0.0")]
            [assembly: AssemblyFileVersion("1.0.0.0")]
            [assembly: AssemblyInformationalVersion("1.0.0.0")]
            """;

        // Act
        var newAssemblyInfo = packTask.ReplaceAttributeValueInAssemblyInfo(TestAssemblyInfo, PackTask.AssemblyVersionAttribute, "5.0.0.0");

        // Assert
        Assert.Equal(expectedAssemblyInfo, newAssemblyInfo);
    }

    [Fact]
    public void Replace_AssemblyFileVersionAttribute_ShouldReplace()
    {
        // Arrange
        var packTask = new PackTask();
        var expectedAssemblyInfo = """
            [assembly: AssemblyVersion("1.0.0.0")]
            [assembly: AssemblyFileVersion("2.3.0.4")]
            [assembly: AssemblyInformationalVersion("1.0.0.0")]
            """;

        // Act
        var newAssemblyInfo = packTask.ReplaceAttributeValueInAssemblyInfo(TestAssemblyInfo, PackTask.AssemblyFileVersionAttribute, "2.3.0.4");

        // Assert
        Assert.Equal(expectedAssemblyInfo, newAssemblyInfo);
    }

    [Fact]
    public void Replace_AssemblyInformationalVersionAttribute_ShouldReplace()
    {
        // Arrange
        var packTask = new PackTask();
        var expectedAssemblyInfo = """
            [assembly: AssemblyVersion("1.0.0.0")]
            [assembly: AssemblyFileVersion("1.0.0.0")]
            [assembly: AssemblyInformationalVersion("4.3.4.5")]
            """;

        // Act
        var newAssemblyInfo = packTask.ReplaceAttributeValueInAssemblyInfo(TestAssemblyInfo, PackTask.AssemblyInformationalVersionAttribute, "4.3.4.5");

        // Assert
        Assert.Equal(expectedAssemblyInfo, newAssemblyInfo);
    }

    [Fact]
    public void Replace_WrongAttribute_ShouldNotReplace()
    {
        // Arrange
        var packTask = new PackTask();

        // Act
        var newAssemblyInfo = packTask.ReplaceAttributeValueInAssemblyInfo(TestAssemblyInfo, "WrongAttributeName", "some value");

        // Assert
        Assert.Equal(TestAssemblyInfo, newAssemblyInfo);
    }
}
