using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkiDiveDev.FastSets
{
    public class SuperSet<T> : ISuperSet<T>, IReadOnlyFastSet<T>, ISet<T> where T : IEquatable<T>
    {
        const int numBitsInMembershipElement = 64;
        const ulong allBitsSetInElement = ulong.MaxValue;
        readonly IMutableFastSet<T> _activeMembers;
        readonly IDictionary<string, IMutableFastSet<T>> _sets = new Dictionary<string, IMutableFastSet<T>>();

        public SuperSet(string name, string description, ICollection<T> population)
        {
            Name = name;
            Description = description;
            Population = population.ToList();

            var activeMembers = GetArrayWithBitsSet(Population.Count);
            _activeMembers = FastSet<T>.Create(this, "__activeMembers", activeMembers);
            _sets.Add("__activeMembers", _activeMembers);
        }

        private ulong[] GetArrayWithBitsSet(int numBitsToSet)
        {
            var numElementsInUse = IntegerCeilingDivision(numBitsToSet, numBitsInMembershipElement);
            var arrayOfBits = new ulong[numElementsInUse];

            if (numElementsInUse == 0)
            {
                return arrayOfBits;
            }


            for (var i = 0; i < numElementsInUse - 1; i++)
            {
                arrayOfBits[i] = allBitsSetInElement;
            }

            var numMembersInLastElement = numBitsToSet % numBitsInMembershipElement;
            var setAllUsedBits = (ulong)((1 << numMembersInLastElement) - 1);
            arrayOfBits[numElementsInUse - 1] = setAllUsedBits;
            return arrayOfBits;
        }


        public string Description { get; private set; }

        public IList<T> Population { get; private set; }

        public IDictionary<string, IReadOnlyFastSet<T>> Sets => (IDictionary<string, IReadOnlyFastSet<T>>)_sets;

        public int PopulationSize => Population.Count;


        public string Name { get; private set; }

        public IMutableFastSet<T> AddSet(string setName, ulong[] presetMembership = null)
        {
            var set = FastSet<T>.Create(this, setName, presetMembership);
            _sets.Add(set.Name, set);
            return set;
        }

        public IMutableFastSet<T> AddSet(string setName, string base64EncodedMembership)
        {
            var set = FastSet<T>.Create(this, setName, base64EncodedMembership);
            _sets.Add(set.Name, set);
            return set;
        }

        public IMutableFastSet<T> AddSet(string setName, ICollection<T> members)
        {
            var set = FastSet<T>.Create(this, setName, members);
            _sets.Add(set.Name, set);
            return set;
        }


        public ISuperSet<T> RemoveSet(IReadOnlyFastSet<T> set)
        {
            if (_sets.ContainsKey(set.Name))
            {
                _sets.Remove(set.Name);
                return this;
            }
            else
            {
                throw new Exception($"The SuperSet did not contain the set that was to be removed.");
            }
        }


        public bool Contains(T member) => Population.Contains(member) && _activeMembers.Contains(member);


        public ISuperSet<T> AddMember(T member)
        {
            if (!Population.Contains(member))
            {
                Population.Add(member);
            }

            // Will re-activate the member if it already exists:
            _activeMembers.Add(member);

            return this;
        }

        public ISuperSet<T> AddMembers(ICollection<T> members)
        {
        //    var activeMembers = _activeMembers.ToUlongArray();
        //    var deletedMembers = new ulong[_activeMembers.Count];
        //    for (var i = 0; i < activeMembers.Length; i++)
        //    {
        //        deletedMembers[i] = ~activeMembers[i];
        //    }
        //    var deletedMembersSet = FastSet<T>.Create(this, "temporary", deletedMembers);

            // TODO: The following will exclude deleted members who are to be re-activated ???
            var uniqueMembers = members.Except(Population).ToList();

            foreach (var thisMember in uniqueMembers)
            {
                Population.Add(thisMember);

                // Will re-activate the member if it already exists:
                _activeMembers.Add(thisMember);
            }

            return this;
        }


        public ISuperSet<T> RemoveMember(T member)
        {
            _activeMembers.Remove(member);
            return this;
        }


        private int IntegerCeilingDivision(int dividend, int divisor) => dividend / divisor
            + (dividend % divisor == 0
            ? 0
            : 1);


        public int Count => _activeMembers.Count;

        public bool IsReadOnly => _activeMembers.IsReadOnly;


        public IReadOnlyFastSet<T> IntersectedWith(string setName) => _activeMembers.IntersectedWith(setName);
        public IReadOnlyFastSet<T> IntersectedWith(ICollection<T> members) => 
            _activeMembers.IntersectedWith(members);
        public IReadOnlyFastSet<T> IntersectedWith(IReadOnlyFastSet<T> source)
            => _activeMembers.IntersectedWith(source);
        public IReadOnlyFastSet<T> UnionedWith(string setName) => _activeMembers.UnionedWith(setName);
        public IReadOnlyFastSet<T> UnionedWith(ICollection<T> members) => _activeMembers.UnionedWith(members);
        public IReadOnlyFastSet<T> UnionedWith(IReadOnlyFastSet<T> source) => _activeMembers.UnionedWith(source);
        public IReadOnlyFastSet<T> DifferenceFrom(IReadOnlyFastSet<T> source)
            => _activeMembers.DifferenceFrom(source);
        public bool All() => _activeMembers.All();
        public bool Any() => _activeMembers.Any();
        public string ToBase64() => _activeMembers.ToBase64();
        public byte[] ToByteArray() => _activeMembers.ToByteArray();
        public ulong[] ToUlongArray() => _activeMembers.ToUlongArray();
        public IDictionary<T, bool> ToDictionary() => _activeMembers.ToDictionary();
        public IEnumerable<T> AsEnumerable() => _activeMembers.AsEnumerable();
        public bool Add(T item) => ((ISet<T>)_activeMembers).Add(item);
        public void ExceptWith(IEnumerable<T> other) => _activeMembers.ExceptWith(other);
        public void IntersectWith(IEnumerable<T> other) => _activeMembers.IntersectWith(other);
        public bool IsProperSubsetOf(IEnumerable<T> other) => _activeMembers.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => _activeMembers.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => _activeMembers.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => _activeMembers.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => _activeMembers.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => _activeMembers.SetEquals(other);
        public void SymmetricExceptWith(IEnumerable<T> other) => _activeMembers.SymmetricExceptWith(other);
        public void UnionWith(IEnumerable<T> other) => _activeMembers.UnionWith(other);
        void ICollection<T>.Add(T item) => ((ICollection<T>)_activeMembers).Add(item);
        public void Clear() => _activeMembers.Clear();
        public void CopyTo(T[] array, int arrayIndex) => _activeMembers.CopyTo(array, arrayIndex);
        public bool Remove(T item) => ((ICollection<T>)_activeMembers).Remove(item);
        public IEnumerator<T> GetEnumerator() => _activeMembers.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_activeMembers).GetEnumerator();
    }
}
