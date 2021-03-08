using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Ski.Dive.Dev.FastSets
{
    class FastSet<T> : EqualityComparer<T>, IReadOnlyFastSet<T>, IMutableFastSet<T> where T : IEquatable<T>
    {
        const int numBitsInMembershipElement = 64;
        const int numBitsPerByte = 8;
        const int numBytesPerElement = numBitsInMembershipElement / numBitsPerByte;

        private ulong[] _membership;
        private int _lastUsedIndexInMembership = 0;
        private int _lastUsedBitInLastMembershipElement = 0;
        private readonly ISuperSet<T> _superSet;


        public FastSet(ISuperSet<T> superSet, string setName, ulong[] presetMembership = null)
        {
            _superSet = superSet;
            Name = setName;
            InitMembership(presetMembership);
        }

        public FastSet(ISuperSet<T> superSet, string setName, string base64EncodedMembership)
        {
            _superSet = superSet;
            Name = setName;
            var presetMembership = FromBase64(base64EncodedMembership);
            InitMembership(presetMembership);
        }

        private void InitMembership(ulong[] presetMembership)
        {
            var minNumMembershipElementsRequired = (int)Math.Max(1,
                Math.Ceiling((double)_superSet.PopulationSize / numBitsInMembershipElement));

            if (presetMembership != null && presetMembership.Length < minNumMembershipElementsRequired)
            {
                throw new Exception(
                    $"The provided {nameof(presetMembership)} does not contain enough members for this SuperSet.");
            }

            _membership = presetMembership ?? new ulong[minNumMembershipElementsRequired];
            AddCapacity(_superSet.PopulationSize - NumElementsInUse);
        }


        /// <summary>
        /// Gets the element at the given <paramref name="elementIndex"/> of
        /// <see cref="ISuperSet{T}"/>.
        /// </summary>
        private ulong GetActiveMembersAtIndex(int elementIndex) =>
            _superSet.ToUlongArray()[elementIndex];

        public bool IsReadOnly => false;


        public string Name { get; private set; }


        public IMutableFastSet<T> SetName(string setName)
        {
            Name = setName;
            return this;
        }



        public IMutableFastSet<T> Add(T member)
        {
            if (!_superSet.Contains(member))
            {
                throw new Exception(
                    "Cannot add a member to a Set when that member does not exist in its enclosing SuperSet.");
            }

            var memberIndex = GetIndexOfMember(member);

            var (elementIndex, bitIndex) = GetElementAndBitIndices(memberIndex);
            _membership[elementIndex] |= GetBitSetAtIndex(bitIndex);

            return this;
        }


        public IMutableFastSet<T> Remove(T member)
        {
            if (!_superSet.Contains(member))
            {
                throw new Exception("Cannot remove a member from a Set that does not exist in the SuperSet.");
            }

            var memberIndex = GetIndexOfMember(member);

            var (elementIndex, bitIndex) = GetElementAndBitIndices(memberIndex);
            _membership[elementIndex] |= GetBitClearedAtIndex(bitIndex);

            return this;
        }


        /// <summary>
        /// An O(n) method to calculate the Hamming weight of a bit array, where n is the total number of bits in
        /// the array.
        /// </summary>
        public int Count
        {
            get
            {
                var numOneBits = 0;

                for (var i = 0; i <= _lastUsedIndexInMembership; i++)
                {
                    var thisElement = _membership[i];
                    while (thisElement != 0)
                    {
                        numOneBits += (int)(thisElement % 2);
                        thisElement >>= 1;
                    }
                }

                return numOneBits;
            }
        }

        /// <summary>
        /// The current member capacity of <see cref="_membership"/>.
        /// </summary>
        private int Capacity => _membership.Length * numBitsInMembershipElement;

        private int NumElementsInUse => _lastUsedIndexInMembership + 1;

        /// <summary>
        /// Due to 0-based positive indices, there must be at least one tracked member.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Four numbers to think about:
        /// </para><para>
        /// 1) <see cref="Capacity"/>: The number of members that the current <see cref="_membership"/> can hold
        /// without expanding.
        /// </para><para>
        /// 2) <see cref="NumElementsInUse"/>: The number of array elements within <see cref="_membership"/> that
        /// are in use, even if the last element is only partially used.
        /// 3) <see cref="NumTrackedMembers"/>: <i>should</i> equal <see cref="ISuperSet.NumberOfMembers"/>.
        /// </para><para>
        /// 4) <see cref="Count"/>: Number of members within the set.
        /// </para>
        /// </remarks>
        private int NumTrackedMembers =>
            _lastUsedIndexInMembership * numBitsInMembershipElement + _lastUsedBitInLastMembershipElement + 1;



        public bool Contains(T item)
        {
            if (!_superSet.Contains(item))
            {
                throw new Exception("A Set cannot determine whether it contains an item when its enclosing" +
                    " SuperSet does not contain the item.");
            }

            var indexOfItem = GetIndexOfMember(item);
            return ((_membership[indexOfItem] & GetBitSetAtIndex(indexOfItem)) != 0);
        }

        private string GenerateNewSetName(string @operator, string operandSetName)
        {
            const int maxNumCharactersInName = 255;
            const int fixedNumCharactersToAddToName = 9;                        // "('' X '')".Length
            const int remainingAvailableNumChars = maxNumCharactersInName - fixedNumCharactersToAddToName;
            var numCharactersToTruncate = remainingAvailableNumChars - Name.Length - operandSetName.Length;
            if (numCharactersToTruncate > 0)
            {
                const int numCharactersInEllipsis = 1;                          // "…".Length
                numCharactersToTruncate += numCharactersInEllipsis;
                var truncatedOriginalName = Name.Substring(Name.Length - numCharactersToTruncate);
                return $"('…{truncatedOriginalName}' {@operator} '{operandSetName}')";
            }
            else
            {
                return $"('{Name}' {@operator} '{operandSetName}')";
            }
        }


        public bool All()
        {
            // TODO: Incorporate _superSet.DeletedMembersSet
            for (var i = 0; i < _lastUsedIndexInMembership; i++)
            {
                if ((_membership[i] & GetActiveMembersAtIndex(i)) != ulong.MaxValue)
                {
                    return false;
                }
            }

            var numMembersInLastElement = _superSet.PopulationSize % numBitsInMembershipElement;
            var lastElementValueWhenAllMembersSet = (ulong)((1 << numMembersInLastElement) - 1);
            return (_membership[_lastUsedIndexInMembership] == lastElementValueWhenAllMembersSet);
        }


        public bool Any()
        {
            // TODO: Incorporate _superSet.DeletedMembersSet
            for (var i = 0; i <= _lastUsedIndexInMembership; i++)
            {
                var thisElement = _membership[i];
                if (thisElement != 0)
                {
                    return true;
                }
            }
            return false;
        }


        public IReadOnlyFastSet<T> DifferenceFrom(IReadOnlyFastSet<T> source) => throw new NotImplementedException();

        public IReadOnlyFastSet<T> IntersectedWith(string setName)
        {
            if (!_superSet.Sets.ContainsKey(setName))
            {
                throw new Exception(
                    $"The given set name, {setName}, does not exist within the enclosing SuperSet.");
            }

            return IntersectedWith(_superSet.Sets[setName]);
        }

        public IReadOnlyFastSet<T> IntersectedWith(IReadOnlyFastSet<T> source)
        {
            var sourceMembership = source.ToUlongArray();
            var intersectedMembers = new ulong[_membership.Length];

            for (var i = 0; i < _membership.Length; i++)
            {
                intersectedMembers[i] = _membership[i] & sourceMembership[i];
            }


            var newIntersectedSetName = GenerateNewSetName("∩", source.Name);

            var intersectionResults = new FastSet<T>(_superSet, newIntersectedSetName, intersectedMembers);
            return intersectionResults;
        }


        public IReadOnlyFastSet<T> UnionedWith(string setName)
        {
            if (!_superSet.Sets.ContainsKey(setName))
            {
                throw new Exception(
                    $"The given set name, {setName}, does not exist within the enclosing SuperSet.");
            }

            return UnionedWith(_superSet.Sets[setName]);
        }


        public IReadOnlyFastSet<T> UnionedWith(IReadOnlyFastSet<T> source)
        {
            var sourceMembership = source.ToUlongArray();
            var unionedMembers = new ulong[_membership.Length];

            for (var i = 0; i < _membership.Length; i++)
            {
                unionedMembers[i] = _membership[i] | sourceMembership[i];
            }

            var newUnionedSetName = GenerateNewSetName("∪", source.Name);
            var unionResults = new FastSet<T>(_superSet, newUnionedSetName, unionedMembers);
            return unionResults;
        }



        /// <summary>
        /// Returns the membership as a little-endian Base-64 encoded string.
        /// </summary>
        public string ToBase64()
        {
            return Convert.ToBase64String(ToByteArray());
        }


        /// <summary>
        /// Returns the membership as a little-endian array of bytes (the least significant bit of any byte
        /// represents the set membership of a member who is arranged before the most significant bit of that same
        /// byte.)
        /// </summary>
        public byte[] ToByteArray()
        {
            var numBits =
                (_lastUsedIndexInMembership * numBitsInMembershipElement + _lastUsedIndexInMembership + 1);

            var byteArray = new byte[IntegerCeilingDivision(numBits, numBitsPerByte)];

            for (var i = 0; i <= _lastUsedIndexInMembership; i++)
            {
                var thisElement = _membership[i];
                var thisElementAsBytes = BitConverter.GetBytes(thisElement);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(thisElementAsBytes);
                }

                Array.Copy(
                    sourceArray: thisElementAsBytes,
                    sourceIndex: 0,
                    destinationArray: byteArray,
                    destinationIndex: i * numBitsInMembershipElement,
                    length: numBitsInMembershipElement / numBitsPerByte);
            }

            return byteArray;
        }


        private ulong[] FromBase64(string base64EncodedMembership)
        {
            var littleEndianMembershipBytes = Convert.FromBase64String(base64EncodedMembership);
            var numMembersDecoded_max = littleEndianMembershipBytes.Length * numBitsPerByte;

            var decodedMembershipIsMoreThan7MembersLargerThanSuperSetMembership =
                (numMembersDecoded_max > _superSet.PopulationSize + numBitsPerByte - 1);
            if (decodedMembershipIsMoreThan7MembersLargerThanSuperSetMembership
                || numMembersDecoded_max < _superSet.PopulationSize)
            {
                throw new ArgumentOutOfRangeException(
                    "The given Base-64 encoded string is not compatible with the SuperSet.");
            }

            var numElementsRequired =
                IntegerCeilingDivision(_superSet.PopulationSize, numBitsInMembershipElement);

            var presetMembership = new ulong[numElementsRequired];

            var thisElementBytes = new byte[numBytesPerElement];

            for (var byteIndex = 0; byteIndex < littleEndianMembershipBytes.Length; byteIndex += numBytesPerElement)
            {
                Array.Copy(
                    sourceArray: littleEndianMembershipBytes,
                    sourceIndex: byteIndex * numBytesPerElement,
                    destinationArray: thisElementBytes,
                    destinationIndex: 0,
                    length: numBytesPerElement);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(thisElementBytes);
                }

                var thisElement = BitConverter.ToUInt64(thisElementBytes, startIndex: 0);
                presetMembership[byteIndex / numBytesPerElement] = thisElement;
            }

            return presetMembership;
        }


        public ulong[] ToUlongArray() => _membership;


        private int IntegerCeilingDivision(int dividend, int divisor) => dividend / divisor
                    + (dividend % divisor == 0
                    ? 1
                    : 0);


        public IDictionary<T, bool> ToDictionary() => throw new NotImplementedException();


        public override int GetHashCode(T obj) => throw new NotImplementedException();
        public override bool Equals(T x, T y) => throw new NotImplementedException();
        public IEnumerator GetEnumerator() => throw new NotImplementedException();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();



        /// <summary>
        /// </summary>
        /// <remarks>
        /// This method could be optimized, but at this point in time, doesn't seem necessary.
        /// </remarks>
        /// <param name="numMembersToAdd"></param>
        public IMutableFastSet<T> AddCapacity(int numMembersToAdd)
        {
            var newTotalCapacity = NumTrackedMembers + numMembersToAdd;

            _lastUsedBitInLastMembershipElement = newTotalCapacity % numBitsInMembershipElement;

            _lastUsedIndexInMembership = IntegerCeilingDivision(newTotalCapacity, numBitsInMembershipElement);

            ConditionallyExpandMembershipSize();
            return this;
        }


        /// <summary>
        /// Expands <see cref="_membership"/> to be at least big enough to hold an element at
        /// <see cref="_lastUsedIndexInMembership"/>.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="_membership"/> was expanded.</returns>
        private bool ConditionallyExpandMembershipSize()
        {
            Debug.Assert(_membership != null, $"{nameof(_membership)} should never be null.");

            if (_lastUsedIndexInMembership >= _membership.Length)
            {
                const double arrayGrowthFactor = 1.2;
                var newArraySize = (int)(_membership.Length * arrayGrowthFactor);
                Array.Resize(ref _membership, newArraySize);
                return true;
            }

            return false;
        }

        private int GetIndexOfMember(T member) => _superSet.Population
            .TakeWhile(m => !m.Equals(member))
            .Count();


        /// <summary>
        /// Given a 0-based index into a list, returns the element index and bit index within the element that
        /// represents that <paramref name="memberIndex"/> (based on <see cref="numBitsInMembershipElement"/>.)
        /// </summary>
        private (int, int) GetElementAndBitIndices(int memberIndex)
        {
            var elementIndex = memberIndex / numBitsInMembershipElement;
            var bitIndex = memberIndex % numBitsInMembershipElement;
            return (elementIndex, bitIndex);
        }

        private ulong GetBitSetAtIndex(int zeroBasedBitIndex) => 1UL << zeroBasedBitIndex;
        private ulong GetBitClearedAtIndex(int zeroBasedBitIndex) => ~(1UL << zeroBasedBitIndex);


        bool ISet<T>.Add(T item) => throw new NotImplementedException();
        public void ExceptWith(IEnumerable<T> other) => throw new NotImplementedException();
        public void IntersectWith(IEnumerable<T> other) => throw new NotImplementedException();
        public bool IsProperSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();
        public bool IsProperSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();
        public bool IsSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();
        public bool IsSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();
        public bool Overlaps(IEnumerable<T> other) => throw new NotImplementedException();
        public bool SetEquals(IEnumerable<T> other) => throw new NotImplementedException();
        public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotImplementedException();
        public void UnionWith(IEnumerable<T> other) => throw new NotImplementedException();
        void ICollection<T>.Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
    }
}
