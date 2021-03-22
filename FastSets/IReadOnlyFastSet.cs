using System;
using System.Collections;
using System.Collections.Generic;

namespace SkiDiveDev.FastSets
{
    /// <summary>
    /// Describes a collection suitable for set theory operations.
    /// </summary>
    /// <typeparam name="T">The type of the members.</typeparam>
    public interface IReadOnlyFastSet<T> : IEnumerable<T>, ISet<T>
        where T : IEquatable<T>
    {
        string Name { get; }


        new bool IsReadOnly { get; }

        IReadOnlyFastSet<T> IntersectedWith(string setName);
        IReadOnlyFastSet<T> IntersectedWith(ICollection<T> members);

        /// <summary>
        /// Produces the set intersection of the given <see cref="IFastSet"/> set with the invoked object.
        /// </summary>
        /// <param name="source"><see cref="IFastSet"/> whose distinct elements that also appear in the invoked
        /// object are returned.</param>
        /// <returns></returns>
        IReadOnlyFastSet<T> IntersectedWith(IReadOnlyFastSet<T> source);

        IReadOnlyFastSet<T> UnionedWith(string setName);
        IReadOnlyFastSet<T> UnionedWith(ICollection<T> members);

        IReadOnlyFastSet<T> UnionedWith(IReadOnlyFastSet<T> source);
        IReadOnlyFastSet<T> DifferenceFrom(IReadOnlyFastSet<T> source);


        /// <summary>
        /// Returns <see langword=""="true"/> if all members of the <see cref="ISuperSet{T}"/> are included within
        /// the invoked <see cref="IReadOnlyFastSet{T}"/>.
        /// </summary>
        bool All();


        /// <summary>
        /// Returns <see langword=""="true"/> if any member of the <see cref="ISuperSet{T}"/> is included within
        /// the invoked <see cref="IReadOnlyFastSet{T}"/>.
        /// </summary>
        bool Any();

        new bool Contains(T member);

        /// <summary>
        /// Returns the number of members within the set.
        /// </summary>
        new int Count { get; }


        string ToBase64();
        byte[] ToByteArray();
        ulong[] ToUlongArray();

        IDictionary<T, bool> ToDictionary();

        IEnumerable<T> AsEnumerable();
    }
}