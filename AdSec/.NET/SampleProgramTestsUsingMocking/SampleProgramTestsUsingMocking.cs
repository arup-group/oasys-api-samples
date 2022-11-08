using Moq;
using Oasys.AdSec;
using OasysUnits;
using Xunit;

namespace SampleProgramTestsUsingMocking
{
    /// <summary>
    /// This example shows how to mock API objects so you can
    /// unit test part of your application.
    ///
    /// It tests <see cref="SampleProgram.SampleProgram.IsCrackWidthSatisfactory"/>
    /// by mocking <see cref="ISolution"/>, <see cref="IServiceabilityResult"/> etc.
    ///
    /// You might like to run the 'ApiVersion' example first, just to check
    /// that the API is installed correctly.
    /// </summary>
    public class SampleProgramTestsUsingMocking
    {
        [Fact]
        public void IsCrackWidthSatisfactory_True_ForSmallCrack()
        {
            // GIVEN we've mocked the AdSec API
            Length length = Length.FromMillimeters(1);
            Mock<ICrack> crackMock = new Mock<ICrack>();
            Mock<ILoad> loadMock = new Mock<ILoad>();
            Mock<IServiceabilityResult> serviceabilityResultMock = new Mock<IServiceabilityResult>();
            Mock<IServiceability> serviceability = new Mock<IServiceability>();
            Mock<ISolution> solutionMock = new Mock<ISolution>();

            serviceabilityResultMock.SetupGet(_ => _.MaximumWidthCrack).Returns(crackMock.Object);
            serviceabilityResultMock.SetupGet(_ => _.MaximumWidthCrack.Width).Returns(length);
            serviceability.Setup(_ => _.Check(loadMock.Object)).Returns(serviceabilityResultMock.Object);
            solutionMock.SetupGet(_ => _.Serviceability).Returns(serviceability.Object);

            // WHEN we check that the crack width is within our expectations
            bool actualResult = SampleProgram.SampleProgram.IsCrackWidthSatisfactory(solutionMock.Object, loadMock.Object);

            // THEN IsCrackWidthSatisfactory() uses the mock objects
            bool expectedResult = true;
            Assert.Equal(expectedResult, actualResult);
        }
    }
}