using System.Collections.ObjectModel;

namespace Sodes.Base
{
  /// <summmary>
  /// Fairly decent sets class implemented using hashtables.
  /// </summmary>
  /// <remarks>
  /// Authors: Richard Bothne, Jim Showalter
  /// </remarks>
  /// <exception>
  /// Throws no exceptions, and propagates untouched all exceptions thrown by callees.
  /// </exception>
	public class Set<KeyType> : Collection<KeyType>
  {
    /// <summary>
    /// Refer to Hashtable constructor documentation.
    /// </summary>
    public Set() : base()
    {
    }

    /// <summary>
    /// Refer to Hashtable constructor documentation.
    /// </summary>
    public Set(Set<KeyType> otherSet) : base(otherSet)
    {
    }

		///// <summary>
		///// Refer to Hashtable constructor documentation.
		///// </summary>
		//public Set(int capacity) : base(capacity)
		//{
		//}

    /// <summary>
    /// Construct a set by passing its initial members.
    /// </summary>
    public Set(KeyType[] members) : base()
    {
      for (int member = 0; member < members.Length; member++)
        Add(members[member]);
    }

    /// <summary>
    ///  Adds an item to the set. Items are stored as keys, with no associated values.
    /// </summary>
//    public void Add(KeyType entry)
//    {
//      base.Add(entry, null);
//    }

    public override string ToString()
    {
      string s = "";
      bool first = true;
      foreach (KeyType member in this)
      {
        if (first) first = false; else s += ", ";
        s += member;
      }
      return s;
    }

    /// <summary>
    /// Helper function that does most of the work in the class.
    /// </summary>
    private static Set<KeyType> Generate(
      Set<KeyType> iterSet,
      Set<KeyType> containsSet,
      Set<KeyType> startingSet,
      bool containment)
    {
      // Returned set either starts out empty or as copy of the starting set.
      Set<KeyType> returnSet = startingSet == null ? new Set<KeyType>() : startingSet;

      foreach (KeyType key in iterSet)
      {
        // (!containment && !containSet.ContainsKey) ||
        //  (containment &&  containSet.ContainsKey)
        if (!(containment ^ containsSet.Contains(key)))
        {
          returnSet.Add(key);
        }
      }

      return returnSet;
    }

    /// <summary>
    /// Union of set1 and set2.
    /// </summary>
    public static Set<KeyType> operator |(Set<KeyType> set1, Set<KeyType> set2)
    {
      // Copy set1, then add items from set2 not already in set 1.
      Set<KeyType> unionSet = new Set<KeyType>(set1);
      return Generate(set2, unionSet, unionSet, false);
    }

    /// <summary>
    /// Union of this set and otherSet.
    /// </summary>
    public Set<KeyType> Union(Set<KeyType> otherSet)
    {
      return this | otherSet;
    }

    /// <summary>
    /// Intersection of set1 and set2.
    /// </summary>
    public static Set<KeyType> operator &(Set<KeyType> set1, Set<KeyType> set2)
    {
      // Find smaller of the two sets, iterate over it
      // to compare to other set.
      return Generate(
        set1.Count > set2.Count ? set2 : set1,
        set1.Count > set2.Count ? set1 : set2,
        null,
        true);
    }

    /// <summary>
    /// Intersection of this set and otherSet.
    /// </summary>
    public Set<KeyType> Intersection(Set<KeyType> otherSet)
    {
      return this & otherSet;
    }

    /// <summary>
    /// Exclusive-OR of set1 and set2.
    /// </summary>
    public static Set<KeyType> operator ^(Set<KeyType> set1, Set<KeyType> set2)
    {
      // Find items in set1 that aren't in set2. Then find
      // items in set2 that aren't in set1. Return combination
      // of those two subresults.
      return Generate(set2, set1, Generate(set1, set2, null, false), false);
    }

    /// <summary>
    /// Exclusive-OR of this set and otherSet.
    /// </summary>
    public Set<KeyType> ExclusiveOr(Set<KeyType> otherSet)
    {
      return this ^ otherSet;
    }

    /// <summary>
    /// The set1 minus set2. This is not associative.
    /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
		public static Set<KeyType> operator -(Set<KeyType> set1, Set<KeyType> set2)
    {
      return Generate(set1, set2, null, false);
    }

    /// <summary>
    /// This set minus otherSet. This is not associative.
    /// </summary>
    public Set<KeyType> Difference(Set<KeyType> otherSet)
    {
      return this - otherSet;
    }
  }
}