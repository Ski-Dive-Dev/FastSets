using System;
using System.Collections.Generic;
using System.Text;

namespace Ski.Dive.Dev.FastSets
{
    public interface IMutableFastSet<T> : IReadOnlyFastSet<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Sets the name of the set to the given <paramref name="setName"/>.
        /// </summary>
        IReadOnlyFastSet<T> SetName(string setName);


        /// <summary>
        /// Adds capacity to the set for when members are added to the SuperSet Population.
        /// </summary>
        IMutableFastSet<T> AddCapacity(int numberMembersToAdd);


        /// <summary>
        /// Adds the given <paramref name="member"/> to the set.
        /// </summary>
        new IMutableFastSet<T> Add(T member);

        /// <summary>
        /// Removes the given <paramref name="member"/> from the set.
        /// </summary>
        new IMutableFastSet<T> Remove(T member);
    }
}
