using System;
using System.Collections.Generic;
using System.Text;

namespace SkiDiveDev.FastSets
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the members.</typeparam>
    public interface ISuperSet<T> : IReadOnlyFastSet<T> where T : IEquatable<T>
    {
        string Description { get; }

        IList<T> Population { get; }

        IDictionary<string, IReadOnlyFastSet<T>> Sets { get; }
        int PopulationSize { get; }

        ISuperSet<T> AddMember(T member); // If member already exists, but is deleted, removes member from _deletedMembers set.

        ISuperSet<T> RemoveMember(T member);


        IMutableFastSet<T> AddSet(string setName, ulong[] presetMembership = null);

        IMutableFastSet<T> AddSet(string setName, string base64EncodedMembership);
        
        ISuperSet<T> RemoveSet(IReadOnlyFastSet<T> set);
    }
}
