using Xunit;

namespace tests
{
    public class SimpleTest
    {
        [Fact]
        public void PrintHello()
        {
            // Print "Hello" to the console
            Console.WriteLine("Hello");

            // Optionally, you can add an assertion
            Assert.True(true); // This will always pass
        }
    }
}
