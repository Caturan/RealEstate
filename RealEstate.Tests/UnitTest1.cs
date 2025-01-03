using Xunit;

namespace RealEstate.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test_Addition()
        {
            // Arrange
            var a = 5;
            var b = 10;

            // Act
            var result = a + b;

            // Assert
            Assert.Equal(15, result);
        }
    }
}