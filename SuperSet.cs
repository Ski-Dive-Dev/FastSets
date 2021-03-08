using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ski.Dive.Dev.FastSets
{
    public class SuperSet<T> : ISuperSet<T>, IReadOnlyFastSet<T> where T : IEquatable<T>
    {
        const int numBitsInMembershipElement = 64;

        IMutableFastSet<T> _activeMembers;
        IDictionary<string, IMutableFastSet<T>> sets;

        public SuperSet(string description, IEnumerable<T> population)
        {
            Description = description;
            Population = (IList<T>)population;

            var numElementsInUse = IntegerCeilingDivision(Population.Count, numBitsInMembershipElement);
            var activeMembers = new ulong[numElementsInUse];
            for (var i = 0; i < numElementsInUse; i++)
            {
                const ulong allBitsSet = ulong.MaxValue;
                activeMembers[i] = allBitsSet;
            }
            _activeMembers = new FastSet<T>(this, "__activeMembers", activeMembers);
        }


        public string Description { get; private set; }

        public IList<T> Population { get; private set; }

        public IDictionary<string, IReadOnlyFastSet<T>> Sets { get; private set; }
            = new Dictionary<string, IReadOnlyFastSet<T>>();

        public int PopulationSize => Population.Count;


        public string Name => _activeMembers.Name;

        public int Count => _activeMembers.Count;

        public bool IsReadOnly => _activeMembers.IsReadOnly;

        public ISuperSet<T> AddSet(IMutableFastSet<T> set)
        {
            sets.Add(set.Name, set);
            return this;
        }

        public ISuperSet<T> RemoveSet(IReadOnlyFastSet<T> set)
        {
            if (sets.ContainsKey(set.Name))
            {
                sets.Remove(set.Name);
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
                foreach (var set in sets.Values)
                {
                    set.AddCapacity(1);
                }
            }
            else
            {
                _activeMembers.Add(member);                                     // reactivate
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
            ? 1
            : 0);


        public IReadOnlyFastSet<T> IntersectedWith(string setName) => _activeMembers.IntersectedWith(setName);
        public IReadOnlyFastSet<T> IntersectedWith(IReadOnlyFastSet<T> source)
            => _activeMembers.IntersectedWith(source);
        public IReadOnlyFastSet<T> UnionedWith(string setName) => _activeMembers.UnionedWith(setName);
        public IReadOnlyFastSet<T> UnionedWith(IReadOnlyFastSet<T> source) => _activeMembers.UnionedWith(source);
        public IReadOnlyFastSet<T> DifferenceFrom(IReadOnlyFastSet<T> source)
            => _activeMembers.DifferenceFrom(source);
        public bool All() => _activeMembers.All();
        public bool Any() => _activeMembers.Any();
        public string ToBase64() => _activeMembers.ToBase64();
        public byte[] ToByteArray() => _activeMembers.ToByteArray();
        public ulong[] ToUlongArray() => _activeMembers.ToUlongArray();
        public IDictionary<T, bool> ToDictionary() => _activeMembers.ToDictionary();
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
