using System;
using System.Collections.Generic;
using System.Text;

namespace Ski.Dive.Dev.FastSets
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the members.</typeparam>
    public interface ISuperSet<T> where T : IEquatable<T>
    {
        string Description { get; }

        IList<T> Population { get; }

        IDictionary<string, IReadOnlyFastSet<T>> Sets { get; }
        int NumberOfMembers { get; }

        ISuperSet<T> AddMember(T member); // If member already exists, but is deleted, removes member from _deletedMembers set.

        ISuperSet<T> RemoveMember(T member);

        bool Contains(T member);

        ISuperSet<T> AddSet(IMutableFastSet<T> set);
        ISuperSet<T> RemoveSet(IReadOnlyFastSet<T> set);

        /// <summary>
        /// Members in the SuperSet Population that are active (not deleted.)
        /// </summary>
        IReadOnlyFastSet<T> ActiveMembers { get; }
    }
}
