using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.PackageTemplate
{
    public class PackageTemplateSyntaxTest : TestBase
    {
        [Fact]
        public void WarmUp()
        {
            var text = @"
class HelloWorld
{
    public static void main(String[] args)
    {
        Console.WriteLine(""Hello, world!"");
    }
}";
            Assert.Empty(text);
        }
    }
}
