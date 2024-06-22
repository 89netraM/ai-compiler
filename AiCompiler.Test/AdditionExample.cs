using FluentAssertions;

namespace AiCompiler.Test;

[TestClass]
public partial class AdditionExample
{
    [TestMethod]
    public void TestAdd()
    {
        Add(2, 3).Should().Be(5);
    }

    /// <summary>
    /// Adds two integers.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>
    /// The sum of the two integers.
    /// </returns>
    [AiGenerated]
    private partial int Add(int a, int b);
}
