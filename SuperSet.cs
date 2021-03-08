using System;
using System.Collections.Generic;
using System.Text;

namespace Ski.Dive.Dev.FastSets
{
    public class SuperSet<T> : ISuperSet<T> where T : IEquatable<T>
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

        public IReadOnlyFastSet<T> ActiveMembers => _activeMembers;


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
    }
}
