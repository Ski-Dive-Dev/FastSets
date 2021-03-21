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
        public void WhenMemberNotInSet_Add_ShouldAddMembersToSet(int populationSize, int numMembers,
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



        [TestCase(0, 0, 1, 0)]
        [TestCase(1, 0, 1, 0)]
        [TestCase(64, 0, 1, 0)]
        [TestCase(100, 0, 1, 0)]
        [TestCase(1, 1, 1, 1)]
        [TestCase(64, 10, 1, 5)]
        [TestCase(100, 99, 1, 50)]
        [TestCase(64, 50, 2, 25)]
        [TestCase(100, 99, 2, 25)]
        [TestCase(64, 51, 3, 10)]
        [TestCase(100, 99, 3, 33)]
        public void WhenMembersAreInSet_Remove_ShouldRemoveFromSet(int populationSize, int numMembers,
            int memberSelector, int numToRemove)
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

            testSet1.Add(members);


            // Members to remove
            var membersToRemove = new List<string>(numToRemove);
            var rng = new Random(12345);
            for (var i = 0; i < numToRemove; i++)
            {
                var memberAlreadyRemoved = false;
                do
                {
                    var memberIndexToRemove = rng.Next(0, members.Count);
                    var memberToRemove = "Test Member " + memberIndexToRemove;
                    memberAlreadyRemoved = membersToRemove.Contains(memberToRemove);
                    if (!memberAlreadyRemoved)
                    {
                        membersToRemove.Add(memberToRemove);
                    }
                } while (memberAlreadyRemoved);
            }


            // Act-Assert
            foreach (var memberToRemove in membersToRemove)
            {
                members.Remove(memberToRemove);

                var result = testSet1.Remove(memberToRemove);
                var actual = result.AsEnumerable();
                Assert.That(actual, Is.EqualTo(members));
            }
        }


        [TestCase(1, 1, 1, 1)]
        [TestCase(64, 10, 1, 10)]
        [TestCase(100, 99, 1, 99)]
        [TestCase(64, 50, 2, 25)]
        [TestCase(100, 98, 2, 49)]
        [TestCase(64, 51, 3, 17)]
        [TestCase(100, 99, 3, 33)]
        public void WhenMembersAreInSet_RemoveAll_ShouldLeaveEmptySet(int populationSize, int numMembers,
            int memberSelector, int numToRemove)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var members = new List<string>(numMembers);
            int expected;

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

            if (members.Count != numToRemove)
            {
                throw new Exception("The number of members to remove must be equal to the number of members in" +
                    $" the set.  Pop: {populationSize}, ActualMembers: {members.Count}, ToRemove: {numToRemove}");
            }
            testSet1.Add(members);


            // Members to remove
            var membersToRemove = new List<string>(numToRemove);
            var rng = new Random(12345);
            for (var i = 0; i < numToRemove; i++)
            {
                bool memberAlreadyRemoved;
                do
                {
                    var memberIndexToRemove = rng.Next(0, members.Count);
                    var memberToRemove = members[memberIndexToRemove];
                    memberAlreadyRemoved = membersToRemove.Contains(memberToRemove);
                    if (!memberAlreadyRemoved)
                    {
                        membersToRemove.Add(memberToRemove);
                    }
                } while (memberAlreadyRemoved);
            }


            // Act-Assert
            expected = membersToRemove.Count;
            foreach (var memberToRemove in membersToRemove)
            {
                var result = testSet1.Remove(memberToRemove);
                var actual = result.Count;
                expected--;
                Assert.That(actual, Is.EqualTo(expected));
            }
        }
    }
}
