using System;
using System.Collections.Generic;
using System.Text;

namespace SkiDiveDev.FastSets
{
    public interface IMutableFastSet<T> : IReadOnlyFastSet<T>, ISet<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Sets the name of the set to the given <paramref name="setName"/>.
        /// </summary>
        IReadOnlyFastSet<T> SetName(string setName);


        /// <summary>
        /// Adds the given <paramref name="member"/> to the set.
        /// </summary>
        new IMutableFastSet<T> Add(T member);

        IMutableFastSet<T> Add(ICollection<T> members);


        /// <summary>
        /// Removes the given <paramref name="member"/> from the set.
        /// </summary>
        new IMutableFastSet<T> Remove(T member);
    }
}
