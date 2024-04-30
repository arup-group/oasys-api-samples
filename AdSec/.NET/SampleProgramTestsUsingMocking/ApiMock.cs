using Moq;
using Oasys.AdSec;
using OasysUnits;
using Xunit;

namespace SampleProgramTestsUsingMocking
{
    /// <summary>
    /// This is a very simple example of a mock object.
    /// It shows how we can create a fake <see cref="IServiceabilityResult"/>
    /// and then get fake crack information from it.
    /// </summary>
    public class ApiMock
    {
        [Fact]
        public void MockSample()
        {
            // GIVEN a serviceability result
            Length length = Length.FromMillimeters(0.5);
            Mock<ICrack> mockCrack = new Mock<ICrack>();
            Mock<IServiceabilityResult> mockServiceabilityResult =
                new Mock<IServiceabilityResult>();
            mockServiceabilityResult.Setup(_ => _.MaximumWidthCrack).Returns(mockCrack.Object);
            mockCrack.Setup(_ => _.Width).Returns(length);

            // WHEN we can read the maximum crack width
            double crackWidth = mockServiceabilityResult.Object.MaximumWidthCrack.Width.Millimeters;
            double someExpectedValue = length.Value;

            // THEN we get the expected value
            Assert.Equal(someExpectedValue, crackWidth);
        }
    }
}
