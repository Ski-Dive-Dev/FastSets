using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SkiDiveDev.FastSets.Test
{
    [TestFixture]
    class MemberTests
    {
        ISuperSet<string> _superSet;

        List<string> _population;


        [SetUp]
        public void Setup()
        {
            List<string> _population = new List<string>();
            _superSet = new SuperSet<string>("Test", "Test Superset", _population);
        }


        // Note: Bits fill from element 0 to element n, and from LSbit (rightmost) to MSbit (leftmost)
        [TestCase(0, 0, 1, new ulong[0])]
        [TestCase(1, 0, 1, new ulong[] { 0 })]
        [TestCase(64, 0, 1, new ulong[] { 0 })]
        [TestCase(100, 0, 1, new ulong[] { 0, 0 })]
        [TestCase(1, 1, 1, new ulong[] { 0x01 })]
        [TestCase(64, 10, 1, new ulong[] { 0x03ff })]
        [TestCase(100, 99, 1, new ulong[] { 0xffffffff_ffffffff, 0x00000007_ffffffff })]
        [TestCase(64, 50, 2, new ulong[] { 0x15555_55555555 })]
        [TestCase(100, 99, 2, new ulong[] { 0x55555555_55555555, 0x00000005_55555555 })]       //0x55 = 0b0101_0101
        [TestCase(64, 51, 3, new ulong[] { 0x00012492_49249249 })]                    // 0x0249 =  0b0010_0100_1001
        [TestCase(100, 99, 3, new ulong[] { 0x92492492_49249249, 0x00000001_24924924 })]
        public void WhenMemberNotInSet_AddMembers_ShouldAddMembersToSet(int populationSize, int numMembers,
            int memberSelector, ulong[] expected)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var members = new List<string>(numMembers);

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            // Set up set members
            for (var i = 0; i < numMembers; i += memberSelector)
            {
                members.Add("Test Member " + i);
            }


            // Act
            testSet1.Add(members);

            // Assert
            var result = testSet1.ToUlongArray();
            Assert.That(result, Is.EqualTo(expected));
        }


        [TestCase(0, 0, 1, "")]
        [TestCase(1, 0, 1, "AA==")]
        [TestCase(64, 0, 1, "AAAAAAAAAAA=")]
        [TestCase(100, 0, 1, "AAAAAAAAAAAAAAAAAA==")]
        [TestCase(1, 1, 1, "AQ==")]
        [TestCase(64, 10, 1, "AAAAAAAAAAA=")]
        [TestCase(100, 99, 1, "////////////////Bw==")]
        [TestCase(64, 50, 2, "AAAAAAAAAAA=")]
        [TestCase(100, 99, 2, "VVVVVVVVVVVVVVVVBQ==")]
        [TestCase(64, 51, 3, "AAAAAAAAAAA=")]
        [TestCase(100, 99, 3, "SZIkSZIkSZIkSZIkAQ==")]
        public void WhenMemberNotInSet_AddMembers_ShouldAddMembersToSet(int populationSize, int numMembers,
            int memberSelector, string expected)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var members = new List<string>(numMembers);

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            // Set up set members
            for (var i = 0; i < numMembers; i += memberSelector)
            {
                members.Add("Test Member " + i);
            }


            // Act
            testSet1.Add(members);

            // Assert
            var result = testSet1.ToBase64();
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
