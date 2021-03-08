using System;
using System.Collections.Generic;
using System.Text;

namespace Ski.Dive.Dev.FastSets
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the members.</typeparam>
    interface ISuperSet<T> where T: IEquatable<T>
    {
        string Description { get;  }

        IList<T> Population { get; }

        IDictionary<string, IReadOnlyFastSet<T>> Sets { get; }
        int NumberOfMembers { get; }

        ISuperSet<T> AddMember(T member); // If member already exists, but is deleted, removes member from _deletedMembers set.
        //private void AddMembers(IEnumerable<T> membersToAdd)
        //{
        //    var newMembersToAdd = _superSet.Population.Except(membersToAdd);        // No duplicates
        //    var numNewMembers = newMembersToAdd.Count();
        //    AddCapacity(numNewMembers);
        //}
        ISuperSet<T> RemoveMember(T member);

        bool Contains(T member);

        ISuperSet<T> AddSet(IReadOnlyFastSet<T> set);
        ISuperSet<T> RemoveSet(IReadOnlyFastSet<T> set);

        /// <summary>
        /// Members in the SuperSet Population that are active (not deleted.)
        /// </summary>
        IReadOnlyFastSet<T> ActiveMembers { get; }
    }
}
