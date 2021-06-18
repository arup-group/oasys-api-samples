using System;
using Moq;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using UnitsNet;
using Oasys.Units;
using Xunit;

namespace SampleProgramTestsUsingMocking
{
    public class ApiMock
    {
        [Fact]
        public void MockSample()
        {
            // GIVEN a serviceability result
            Length length = Length.FromMillimeters(0.5);
            Mock<ICrack> mockCrack = new Mock<ICrack>();
            Mock<IServiceabilityResult> mockServiceabilityResult = new Mock<IServiceabilityResult>();
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