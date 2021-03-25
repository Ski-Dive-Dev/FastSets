using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SkiDiveDev.FastSets
{
    public class FastSet<T> : EqualityComparer<T>, IReadOnlyFastSet<T>, IMutableFastSet<T> where T : IEquatable<T>
    {
        const int numBitsInMembershipElement = 64;
        const int numBitsPerByte = 8;
        const int numBytesPerElement = numBitsInMembershipElement / numBitsPerByte;

        /// <summary>
        /// Bit encodings of the members, specified in-order within <see cref="ISuperSet{T}.Population"/>, of this
        /// set.  The least-significant-bit (LSb) of <see cref="_membership"/>[0] represents the membership of the
        /// first member within <see cref="ISuperSet{T}"/>.
        /// </summary>
        private ulong[] _membership;

        private const ulong allBitsSetInElement = ulong.MaxValue;
        private const int codeForNoTrackedMembers = -1;
        private int _lastUsedIndexInMembership = codeForNoTrackedMembers;
        private int _numBitsUsedInLastElement = 0;
        private readonly ISuperSet<T> _superSet;


        internal static IMutableFastSet<T> Create(ISuperSet<T> superSet, string setName,
             ulong[] presetMembership = null) => new FastSet<T>(superSet, setName, presetMembership);

        internal static IMutableFastSet<T> Create(ISuperSet<T> superSet, string setName,
             string base64EncodedMembership) => new FastSet<T>(superSet, setName, base64EncodedMembership);

        internal static IMutableFastSet<T> Create(ISuperSet<T> superSet, string setName,
             ICollection<T> members) => new FastSet<T>(superSet, setName, members);


        protected FastSet(ISuperSet<T> superSet, string setName, ulong[] presetMembership = null)
        {
            _superSet = superSet ?? throw new ArgumentNullException(nameof(superSet));
            Name = setName;
            InitMembership(presetMembership);
        }


        protected FastSet(ISuperSet<T> superSet, string setName, string base64EncodedMembership)
        {
            if (base64EncodedMembership == null)
            {
                throw new ArgumentNullException(nameof(base64EncodedMembership));
            }

            _superSet = superSet ?? throw new ArgumentNullException(nameof(superSet));
            Name = setName;
            var presetMembership = FromBase64(base64EncodedMembership);
            InitMembership(presetMembership);
        }


        protected FastSet(ISuperSet<T> superSet, string setName, ICollection<T> members)
        {
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            _superSet = superSet ?? throw new ArgumentNullException(nameof(superSet));
            Name = setName;
            const ulong[] nullToGenerateMembershipFromPopulation = null;
            InitMembership(nullToGenerateMembershipFromPopulation);
            Add(members);

        }


        private void InitMembership(ulong[] presetMembership)
        {
            _membership = presetMembership;
            AddCapacity(_superSet.PopulationSize - NumTrackedMembers);
        }


        /// <summary>
        /// Gets the element at the given <paramref name="elementIndex"/> of
        /// <see cref="ISuperSet{T}"/> (the active members element.)
        /// </summary>
        private ulong GetActiveMembersAtIndex(int elementIndex) =>
            _superSet.ToUlongArray()[elementIndex];

        public bool IsReadOnly => (this is IReadOnlyFastSet<T>);


        public string Name { get; private set; }


        /// <summary>
        /// A one-time chance to set the name of the set outside of the constructor.
        /// </summary>
        public IReadOnlyFastSet<T> SetName(string setName)
        {
            Name = setName;
            return this;
        }



        /// <summary>
        /// The superset's Population must contain the member to be added to this set.
        /// </summary>
        /// <param name="member">The member (that exists in the superset) to add to this set.</param>
        public IMutableFastSet<T> Add(T member)
        {
            AddCapacity(_superSet.PopulationSize - NumTrackedMembers);

            var memberIndex = GetIndexOfMember(member);

            // Short-circuit note: Name must be checked first or a deadly embrace exists due to circular logic.
            if (Name != "__activeMembers" && memberIndex == -1)
            {
                throw new Exception(
                    "Cannot add a member to a Set when that member does not exist in its enclosing SuperSet.");

            }
            var (elementIndex, bitIndex) = GetElementAndBitIndices(memberIndex);
            _membership[elementIndex] |= GetBitSetAtIndex(bitIndex);

            return this;
        }


        /// <summary>
        /// The members to add to this set.  Only members that are within the superset's Population will be added.
        /// </summary>
        /// <param name="members">The members (that exist in the superset) to add to this set.</param>
        public IMutableFastSet<T> Add(ICollection<T> members)
        {
            AddCapacity(_superSet.PopulationSize - NumTrackedMembers);

            if (Name == "__activeMembers")
            {
                return AddToActiveMembers(members);
            }

            // TODO: This loop could be removed with a simple mask of consecutive bits (must, however, accommodate
            // element boundary crossings):
            foreach (var thisMember in members)
            {
                var memberIndex = GetIndexOfMember(thisMember);
                var (elementIndex, bitIndex) = GetElementAndBitIndices(memberIndex);
                _membership[elementIndex] |= GetBitSetAtIndex(bitIndex);
            }

            return this;
        }

        private IMutableFastSet<T> AddToActiveMembers(ICollection<T> members)
        {
            foreach (var thisMember in members)
            {
                var memberIndex = GetIndexOfMember(thisMember);

                var (elementIndex, bitIndex) = GetElementAndBitIndices(memberIndex);
                _membership[elementIndex] |= GetBitSetAtIndex(bitIndex);
            }

            return this;
        }


        public IMutableFastSet<T> Remove(T member)
        {
            var memberIndex = GetIndexOfMember(member);

            if (memberIndex == -1)
            {
                throw new Exception("Cannot remove a member from a Set that does not exist in the SuperSet.");
            }

            var (elementIndex, bitIndex) = GetElementAndBitIndices(memberIndex);
            _membership[elementIndex] &= GetBitClearedAtIndex(bitIndex);

            return this;
        }

        bool ICollection<T>.Remove(T member)
        {
            var memberIndex = GetIndexOfMember(member);

            if (memberIndex == -1)
            {
                const bool falseToIndicateItemWasNotFoundInSet = false;
                return falseToIndicateItemWasNotFoundInSet;
            }

            var (elementIndex, bitIndex) = GetElementAndBitIndices(memberIndex);
            _membership[elementIndex] &= GetBitClearedAtIndex(bitIndex);

            const bool trueToIndicateItemWasSuccessfullyRemovedFromSet = true;
            return trueToIndicateItemWasSuccessfullyRemovedFromSet;
        }


        /// <summary>
        /// An O(n) method to calculate the Hamming weight of a bit array, where n is the total number of bits in
        /// the array.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Requires that unused bits in the last element be set to zero.
        /// </para><para>
        /// Also requires that all members in this set are active in the superset (this is not checked for
        /// performance reasons.)
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// The number of elements in use within <see cref="_membership"/>.
        /// </summary>
        /// <remarks>
        /// Based on the 0-based <see cref="_lastUsedIndexInMembership"/>.  Will return <c>0</c> if both
        /// <see cref="_lastUsedIndexInMembership"/> and <see cref="_numBitsUsedInLastElement"/> are both <c>0</c>.
        /// </remarks>
        private int NumElementsInUse => (_lastUsedIndexInMembership == codeForNoTrackedMembers)
            ? 0
            : (_lastUsedIndexInMembership + (_numBitsUsedInLastElement == 0 ? 0 : 1));

        private int NumBitsInUse => NumTrackedMembers;


        /// <summary>
        /// When the object is stable, returns the same value as <see cref="ISuperSet{T}.PopulationSize"/>.
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
        /// 3) <see cref="NumTrackedMembers"/>: <i>should</i> equal <see cref="ISuperSet.PopulationSize"/>.
        /// </para><para>
        /// 4) <see cref="Count"/>: Number of members within the set.
        /// </para>
        /// </remarks>
        private int NumTrackedMembers => (_lastUsedIndexInMembership == codeForNoTrackedMembers)
            ? 0
            : _lastUsedIndexInMembership * numBitsInMembershipElement + _numBitsUsedInLastElement;




        public bool Contains(T item)
        {
            var indexOfItem = GetIndexOfMember(item);

            if (Name != "__activeMembers" && indexOfItem == -1)
            {
                throw new Exception("A Set cannot determine whether it contains an item when its enclosing" +
                    " SuperSet does not contain the item.");
            }

            var (elementIndex, bitIndex) = GetElementAndBitIndices(indexOfItem);
            return ((_membership[elementIndex] & GetBitSetAtIndex(indexOfItem)) != 0);
        }


        // TODO: Inject
        private string GenerateNewSetName(string @operator, string operandSetName)
        {
            var nameIsAlreadyQuoted = (Name[0] == '\'');
            var generatedName = Name;

            const int maxNumCharactersInName = 255;
            const int fixedNumCharactersToAddToName = 9;                        // "('' X '')".Length
            const int remainingAvailableNumChars = maxNumCharactersInName - fixedNumCharactersToAddToName;
            var numCharactersToTruncate = Name.Length + operandSetName.Length - remainingAvailableNumChars;
            if (numCharactersToTruncate > 0)
            {
                const int numCharactersInEllipsis = 1;                          // "…".Length
                numCharactersToTruncate += numCharactersInEllipsis;
                var truncatedOriginalName = Name.Substring(0, Name.Length - numCharactersToTruncate);
                generatedName = $"…{truncatedOriginalName}";
            }

            var quote = nameIsAlreadyQuoted
                ? ""
                : "'";

            return $"({quote}{generatedName}{quote} {@operator} '{operandSetName}')";
        }


        private bool MembershipIsEmpty => (_lastUsedIndexInMembership == codeForNoTrackedMembers);
        private int LengthOfMembership_OrZero => (MembershipIsEmpty) ? 0 : _membership.Length;


        /// <summary>
        /// Returns <see langword="true"/> if all members within the superset's population are within this set.
        /// </summary>
        public bool All()
        {
            if (MembershipIsEmpty)
            {
                var trueIfThereIsNoPopulation_ThereforeAllInSetAreInPopulation = (_superSet.PopulationSize == 0);
                return trueIfThereIsNoPopulation_ThereforeAllInSetAreInPopulation;
            }

            var activeMembers = _superSet.ToUlongArray();

            for (var i = 0; i < _lastUsedIndexInMembership; i++)
            {
                if ((_membership[i] & activeMembers[i]) != allBitsSetInElement)
                {
                    return false;
                }
            }

            var lastElementValueWhenAllMembersSet = GetLsbMask(_numBitsUsedInLastElement);
            return (_membership[_lastUsedIndexInMembership] == lastElementValueWhenAllMembersSet);
        }


        /// <summary>
        /// Returns <see langword="true"/> if there are any members within the set.  (This method is more
        /// performant than using <see cref="Count"/>.)
        /// </summary>
        public bool Any()
        {
            if (MembershipIsEmpty)
            {
                return false;
            }

            var activeMembers = _superSet.ToUlongArray();

            // This method can produce a false-positive if the unused bits in the last element are improperly set.
            for (var i = 0; i <= _lastUsedIndexInMembership; i++)
            {
                var thisElement = _membership[i];
                if ((thisElement & activeMembers[i]) != 0)
                {
                    return true;
                }
            }
            return false;
        }


        public void Clear()
        {
            for (var i = 0; i < LengthOfMembership_OrZero; i++)
            {
                _membership[i] = 0;
            }
            _lastUsedIndexInMembership = codeForNoTrackedMembers;
            _numBitsUsedInLastElement = 0;
        }


        public IReadOnlyFastSet<T> DifferenceFrom(IReadOnlyFastSet<T> source) => throw new NotImplementedException();


        /// <summary>
        /// Returns an immutable set that is the result of the Set Intersection function between this set and the
        /// given set.  The set that the method is invoked on is unmodified.
        /// </summary>
        /// <param name="setName">The name of a set that is a member of the enclosing SuperSet.</param>
        public IReadOnlyFastSet<T> IntersectedWith(string setName)
        {
            if (!_superSet.Sets.ContainsKey(setName))
            {
                throw new Exception(
                    $"The given set name, {setName}, does not exist within the enclosing SuperSet.");
            }

            return IntersectedWith(_superSet.Sets[setName]);
        }

        public IReadOnlyFastSet<T> IntersectedWith(ICollection<T> members)
        {
            members = members ?? throw new ArgumentNullException(nameof(members));
            var tempSet = new FastSet<T>(_superSet, "temp", members);
            return IntersectedWith(tempSet);
        }

        /// <summary>
        /// Returns an immutable set that is the result of the Set Intersection function between this set and the
        /// given set.  The set that the method is invoked on is unmodified.
        /// </summary>
        public IReadOnlyFastSet<T> IntersectedWith(IReadOnlyFastSet<T> source)
        {
            var sourceMembership = source?.ToUlongArray()
                ?? throw new ArgumentNullException(nameof(source));

            var activeMembers = _superSet.ToUlongArray();

            var intersectedMembers = new ulong[LengthOfMembership_OrZero];

            for (var i = 0; i < _membership.Length; i++)
            {
                intersectedMembers[i] = _membership[i] & activeMembers[i] & sourceMembership[i];
            }


            var newIntersectedSetName = GenerateNewSetName("∩", source.Name);

            var intersectionResults = new FastSet<T>(_superSet, newIntersectedSetName, intersectedMembers);
            return intersectionResults;
        }


        /// <summary>
        /// Returns an immutable set that is the result of the Set Union function between this set and the
        /// given set.  The set that the method is invoked on is unmodified.
        /// </summary>
        /// <param name="setName">The name of a set that is a member of the enclosing SuperSet.</param>
        public IReadOnlyFastSet<T> UnionedWith(string setName)
        {
            if (!_superSet.Sets.TryGetValue(setName, out var sourceSet))
            {
                throw new Exception(
                    $"The given set name, {setName}, does not exist within the enclosing SuperSet.");
            }

            return UnionedWith(sourceSet);
        }

        public IReadOnlyFastSet<T> UnionedWith(ICollection<T> members)
        {
            members = members ?? throw new ArgumentNullException(nameof(members));
            var tempSet = new FastSet<T>(_superSet, "temp", members);
            return UnionedWith(tempSet);
        }


        /// <summary>
        /// Returns an immutable set that is the result of the Set Union function between this set and the
        /// given set.  The set that the method is invoked on is unmodified.
        /// </summary>
        public IReadOnlyFastSet<T> UnionedWith(IReadOnlyFastSet<T> source)
        {
            var sourceMembership = source?.ToUlongArray()
                ?? throw new ArgumentNullException(nameof(source));

            var activeMembers = _superSet.ToUlongArray();

            var unionedMembers = new ulong[LengthOfMembership_OrZero];

            for (var i = 0; i < _membership.Length; i++)
            {
                unionedMembers[i] = _membership[i] & activeMembers[i] | sourceMembership[i];
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
            // TODO: This Converter likely already considers endianess of the platform, therefore, the
            // ToByteArray() will redundantly and erroneously change the endianess of the bytes -- verify!
            return Convert.ToBase64String(ToByteArray());
        }


        /// <summary>
        /// Returns the membership as a little-endian array of bytes (the least significant bit of any byte
        /// represents the set membership of a member who is arranged before the most significant bit of that same
        /// byte.)
        /// </summary>
        public byte[] ToByteArray()
        {
            const int numBytesInMembershipElement = numBitsInMembershipElement / numBitsPerByte;

            var numBits = NumBitsInUse;

            var numBytesInLastElementToCopy =
                IntegerCeilingDivision(numBits % numBitsInMembershipElement, numBitsPerByte);

            var byteArray = new byte[IntegerCeilingDivision(numBits, numBitsPerByte)];
            var needToConvertToLittleEndian = (!BitConverter.IsLittleEndian);

            for (var i = 0; i <= _lastUsedIndexInMembership; i++)
            {
                var thisElement = _membership[i];
                var thisElementAsBytes = BitConverter.GetBytes(thisElement);

                if (needToConvertToLittleEndian)
                {
                    Array.Reverse(thisElementAsBytes);
                }

                Array.Copy(
                    sourceArray: thisElementAsBytes,
                    sourceIndex: 0,
                    destinationArray: byteArray,
                    destinationIndex: i * numBytesInMembershipElement,
                    length: (i < _lastUsedIndexInMembership)
                        ? numBitsInMembershipElement / numBitsPerByte
                        : numBytesInLastElementToCopy
                );
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
            var numBytesInLastElementToCopy =
                littleEndianMembershipBytes.Length % numBytesPerElement;

            for (var byteIndex = 0; byteIndex < littleEndianMembershipBytes.Length; byteIndex += numBytesPerElement)
            {
                var thisElementBytes = new byte[numBytesPerElement];
                Array.Copy(
                    sourceArray: littleEndianMembershipBytes,
                    sourceIndex: byteIndex,
                    destinationArray: thisElementBytes,
                    destinationIndex: 0,
                    length: byteIndex < littleEndianMembershipBytes.Length - numBytesPerElement
                        ? numBytesPerElement
                        : numBytesInLastElementToCopy);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(thisElementBytes);
                }

                var thisElement = BitConverter.ToUInt64(thisElementBytes, startIndex: 0);
                presetMembership[byteIndex / numBytesPerElement] = thisElement;
            }

            return presetMembership;
        }


        public ulong[] ToUlongArray()
        {
            var activeElements = new ulong[NumElementsInUse];

            if (!MembershipIsEmpty)
            {
                Array.Copy(
                    sourceArray: _membership,
                    sourceIndex: 0,
                    destinationArray: activeElements,
                    destinationIndex: 0,
                    length: NumElementsInUse);
            }
            return activeElements;
        }


        /// <summary>
        /// Divides <paramref name="dividend"/> by <paramref name="divisor"/>, and returns the Ceiling of the
        /// the result.
        /// </summary>
        private int IntegerCeilingDivision(int dividend, int divisor) => dividend / divisor
                    + (dividend % divisor == 0
                    ? 0
                    : 1);


        /// <summary>
        /// Returns the superset's Population and a <see langword="bool"/> which indicates each superset's
        /// membership within this set.
        /// </summary>
        public IDictionary<T, bool> ToDictionary()
        {
            var members = new Dictionary<T, bool>(this.Count);

            var activeMembers = _superSet.ToUlongArray();

            for (var i = 0; i <= _lastUsedIndexInMembership; i++)
            {
                var activeMembersInThisElement = _membership[i] & activeMembers[i];
                for (var b = 0; b < numBitsInMembershipElement; b++)
                {
                    var memberIndex = GetIndexOfMember(elementIndex: i, bitIndex: b);
                    var member = _superSet.Population[memberIndex];
                    var isMember = ((GetBitSetAtIndex(b) & activeMembersInThisElement) != 0);
                    members.Add(member, isMember);
                }
            }

            return members;
        }


        /// <summary>
        /// Returns the set members as an <see cref="IEnumerable{T}"/>.
        /// </summary>
        public IEnumerable<T> AsEnumerable()
        {
            var activeMembers = _superSet.ToUlongArray();

            for (var i = 0; i <= _lastUsedIndexInMembership; i++)
            {
                var activeMembersInThisElement = _membership[i] & activeMembers[i];
                for (var b = 0; b < numBitsInMembershipElement; b++)
                {
                    if ((GetBitSetAtIndex(b) & activeMembersInThisElement) != 0)
                    {
                        var memberIndex = GetIndexOfMember(elementIndex: i, bitIndex: b);
                        yield return _superSet.Population[memberIndex];
                    }
                }
            }
        }


        public override int GetHashCode(T obj) => throw new NotImplementedException();
        public override bool Equals(T x, T y) => throw new NotImplementedException();
        public IEnumerator GetEnumerator() => throw new NotImplementedException();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();



        /// <summary>
        /// Adds capacity to the set for when members are added to the SuperSet Population.
        /// </summary>
        private IMutableFastSet<T> AddCapacity(int numMembersToAdd)
        {
            if (numMembersToAdd > 0)
            {
                var newTotalCapacity = NumTrackedMembers + numMembersToAdd;

                _numBitsUsedInLastElement = (newTotalCapacity - 1) % numBitsInMembershipElement + 1;

                // If newTotalCapacity == 0, _lastUsedIndexInMembership will = -1, which is the
                // codeForNoTrackedMembers.
                _lastUsedIndexInMembership =
                    IntegerCeilingDivision(newTotalCapacity, numBitsInMembershipElement) - 1;

                ConditionallyExpandMembershipSize();
            }
            return this;
        }


        /// <summary>
        /// Expands <see cref="_membership"/> to be at least big enough to hold an element at
        /// <see cref="_lastUsedIndexInMembership"/>.
        /// </summary>
        /// <returns><see langword="true"/> if <see cref="_membership"/> was expanded.</returns>
        private bool ConditionallyExpandMembershipSize()
        {
            const double arrayGrowthFactor = 1.2;
            var newArraySize = (int)((_lastUsedIndexInMembership + 1) * arrayGrowthFactor);

            if (_membership == null)
            {
                _membership = new ulong[newArraySize];
                return true;
            }
            else if (_lastUsedIndexInMembership >= _membership.Length)
            {
                Array.Resize(ref _membership, newArraySize);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets the index of the given member within the superset's Population.
        /// </summary>
        private int GetIndexOfMember(T member) => _superSet.Population.IndexOf(member);

        /// <summary>
        /// Gets the index of the member given its index into <see cref="_membership"/> and its 0-based bit index
        /// into that element.
        /// </summary>
        private int GetIndexOfMember(int elementIndex, int bitIndex) =>
            elementIndex * numBitsInMembershipElement + bitIndex;

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

        /// <summary>
        /// Returns a <see langword="ulong"/> in which the given bit index is the only one set (set to 1).
        /// </summary>
        private ulong GetBitSetAtIndex(int zeroBasedBitIndex) => 1UL << zeroBasedBitIndex;

        /// <summary>
        /// Returns a <see langword="ulong"/> in which the given bit index is the only one cleared (set to 0).
        /// </summary>
        private ulong GetBitClearedAtIndex(int zeroBasedBitIndex) => ~(1UL << zeroBasedBitIndex);

        private ulong GetLsbMask(int numLeastSignificantBitsToMask) =>
            (numLeastSignificantBitsToMask == numBitsInMembershipElement)
            ? allBitsSetInElement
            : ~(allBitsSetInElement << numLeastSignificantBitsToMask);


        bool ISet<T>.Add(T item) => throw new NotImplementedException();
        void ICollection<T>.Add(T item) => throw new NotImplementedException();
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


        public void CopyTo(T[] array, int arrayIndex)
        {
            array = array ?? throw new ArgumentNullException(nameof(array));
            if (array.Length == 0)
            {
                return;
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            var setMembers = AsEnumerable()
                .ToArray();

            var numMembersToCopy = Math.Min(setMembers.Length, array.Length - arrayIndex);
            Array.Copy(
                sourceArray: setMembers,
                sourceIndex: 0,
                destinationArray: array,
                destinationIndex: arrayIndex,
                length: numMembersToCopy);
        }
    }
}
