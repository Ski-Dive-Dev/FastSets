using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SkiDiveDev.FastSets.Test
{
    [TestFixture]
    public class SerializationTests
    {
        ISuperSet<string> _superSet;

        [SetUp]
        public void Setup()
        {
            var population = new List<string>
            {
                "Allison",
                "Bobby",
                "Charlie",
                "Dorothy",
                "Elaine",
                "Fester",
                "Gordan",
                "Hillary",
                "Iris",
                "Jane" };
            _superSet = new SuperSet<string>("Test", "Test Superset", population);
        }


        [Test]
        public void ShouldBase64EncodeMembership()
        {
            // Arrange
            const string expected = "//////////////8D";

            var testSet = _superSet.AddSet("testSet");

            var members = new List<string>
            {
                "Allison",
                "Bobby",
                "Charlie",
                "Dorothy",
                "Elaine",
                "Fester",
                "Gordan",
                "Hillary",
                "Iris",
                "Jane" };
            testSet.Add(members);

            for (var i = 10; i < 90; i++)
            {
                _superSet.AddMember("Test Member " + i);
                testSet.Add("Test Member " + i);
            }

            // Act
            var result = testSet.ToBase64();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }


        [Test]
        public void ShouldCreateSetFromBase64Membership()
        {
            // Arrange
            for (var i = 10; i < 90; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }

            const string sample = "//////////////8D";
            var expected = new ulong[] { 
                0xFFFFFFFF_FFFFFFFF,
                0x00000000_03FFFFFF
            };

            // Act
            var testSet = _superSet.AddSet("testSet", sample);

            // Assert
            var result = testSet.ToUlongArray();
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
