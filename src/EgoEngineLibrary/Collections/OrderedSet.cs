// Based on .NET runtime code
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/runtime/blob/cf4d2b0594a7af4a5ff39ffd4fe5cfb0c5577a3d/src/libraries/System.Collections/src/System/Collections/Generic/OrderedDictionary.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using EgoEngineLibrary.Resources;

namespace EgoEngineLibrary.Collections;

/// <summary>
/// Represents a set of elements that are accessible by index.
/// </summary>
/// <typeparam name="T">The type of the elements in the set.</typeparam>
/// <remarks>
/// Operations on the collection have algorithmic complexities that are similar to that of the <see cref="List{T}"/>
/// class, except with lookups by value similar in complexity to that of <see cref="HashSet{T}"/>.
/// </remarks>
//[DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
    public class OrderedSet<T> :
    ISet<T>, IReadOnlySet<T>,
    IList<T>, IReadOnlyList<T>
{
    /// <summary>Cutoff point for stackallocs. This corresponds to the number of ints.</summary>
    private const int StackAllocThreshold = 100;
    /// <summary>The comparer used by the collection. May be null if the default comparer is used.</summary>
    private IEqualityComparer<T>? _comparer;
    /// <summary>Indexes into <see cref="_entries"/> for the start of chains; indices are 1-based.</summary>
    private int[]? _buckets;
    /// <summary>Ordered entries in the set.</summary>
    /// <remarks>
    /// Unlike <see cref="HashSet{T}"/>, removed entries are actually removed rather than left as holes
    /// that can be filled in by subsequent additions. This is done to retain ordering.
    /// </remarks>
    private Entry[]? _entries;
    /// <summary>The number of items in the collection.</summary>
    private int _count;
    /// <summary>Version number used to invalidate an enumerator.</summary>
    private int _version;
    /// <summary>Multiplier used on 64-bit to enable faster % operations.</summary>
    private ulong _fastModMultiplier;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that is empty,
    /// has the default initial capacity, and uses the default equality comparer for the set type.
    /// </summary>
    public OrderedSet() : this(0, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that is empty,
    /// has the specified initial capacity, and uses the default equality comparer for the set type.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see cref="OrderedSet{T}"/> can contain.</param>
    /// <exception cref="ArgumentOutOfRangeException">capacity is less than 0.</exception>
    public OrderedSet(int capacity) : this(capacity, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that is empty,
    /// has the default initial capacity, and uses the specified <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements,
    /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the set.
    /// </param>
    public OrderedSet(IEqualityComparer<T>? comparer) : this(0, comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that is empty,
    /// has the specified initial capacity, and uses the specified <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see cref="OrderedSet{T}"/> can contain.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements,
    /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the set.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">capacity is less than 0.</exception>
    public OrderedSet(int capacity, IEqualityComparer<T>? comparer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (capacity > 0)
        {
            EnsureBucketsAndEntriesInitialized(capacity);
        }

        // Initialize the comparer:
        // - Strings: Special-case EqualityComparer<string>.Default, StringComparer.Ordinal, and
        //   StringComparer.OrdinalIgnoreCase. We start with a non-randomized comparer for improved throughput,
        //   falling back to a randomized comparer if the hash buckets become sufficiently unbalanced to cause
        //   more collisions than a preset threshold.
        // - Other reference types: we always want to store a comparer instance, either the one provided,
        //   or if one wasn't provided, the default (accessing EqualityComparer<T>.Default
        //   with shared generics on every dictionary access can add measurable overhead).
        // - Value types: if no comparer is provided, or if the default is provided, we'd prefer to use
        //   EqualityComparer<T>.Default.Equals on every use, enabling the JIT to
        //   devirtualize and possibly inline the operation.
        if (!typeof(T).IsValueType)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;

#if SYSTEM_COLLECTIONS
            if (typeof(T) == typeof(string) &&
                NonRandomizedStringEqualityComparer.GetStringComparer(_comparer) is IEqualityComparer<string>
                    stringComparer)
            {
                _comparer = (IEqualityComparer<T>)stringComparer;
            }
#endif
        }
        else if (comparer is not null && // first check for null to avoid forcing default comparer instantiation unnecessarily
                 comparer != EqualityComparer<T>.Default)
        {
            _comparer = comparer;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that contains elements copied
    /// from the specified <see cref="IEnumerable{T}"/> and uses the default equality comparer for the set type.
    /// </summary>
    /// <param name="collection">
    /// The <see cref="IEnumerable{T}"/> whose elements are copied to the new <see cref="OrderedSet{T}"/>.
    /// The initial order of the elements in the new collection is the order the elements are enumerated from the supplied collection.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public OrderedSet(IEnumerable<T> collection) : this(collection, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that contains elements copied
    /// from the specified <see cref="IEnumerable{T}"/> and uses the specified <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <param name="collection">
    /// The <see cref="IEnumerable{T}"/> whose elements are copied to the new <see cref="OrderedSet{T}"/>.
    /// The initial order of the elements in the new collection is the order the elements are enumerated from the supplied collection.
    /// </param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements,
    /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the set.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public OrderedSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer) :
        this((collection as ICollection<T>)?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(collection);

        AddRange(collection);
    }

    /// <summary>Initializes the <see cref="_buckets"/>/<see cref="_entries"/>.</summary>
    /// <param name="capacity"></param>
    [MemberNotNull(nameof(_buckets))]
    [MemberNotNull(nameof(_entries))]
    private void EnsureBucketsAndEntriesInitialized(int capacity)
    {
        Resize(HashHelpers.GetPrime(capacity));
    }

    /// <summary>Gets the total number of elements the internal data structure can hold without resizing.</summary>
    public int Capacity => _entries?.Length ?? 0;

    /// <summary>Gets the <see cref="IEqualityComparer{T}"/> that is used to determine equality for the elements in the set.</summary>
    public IEqualityComparer<T> Comparer
    {
        get
        {
            IEqualityComparer<T>? comparer = _comparer;

#if SYSTEM_COLLECTIONS
            // If the value is a string, we may have substituted a non-randomized comparer during construction.
            // If we did, fish out and return the actual comparer that had been provided.
            if (typeof(T) == typeof(string) &&
                (comparer as NonRandomizedStringEqualityComparer)?.GetUnderlyingEqualityComparer() is IEqualityComparer<T> ec)
            {
                return ec;
            }
#endif

            // Otherwise, return whatever comparer we have, or the default if none was provided.
            return comparer ?? EqualityComparer<T>.Default;
        }
    }

    /// <summary>Gets the number of elements contained in the <see cref="OrderedSet{T}"/>.</summary>
    public int Count => _count;

    /// <inheritdoc/>
    bool ICollection<T>.IsReadOnly => false;

    /// <inheritdoc/>
    T IList<T>.this[int index]
    {
        get => GetAt(index);
        set => SetAt(index, value);
    }

    /// <inheritdoc/>
    T IReadOnlyList<T>.this[int index] => GetAt(index);

    /// <summary>Gets or sets the element as the specified index.</summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
    /// <remarks>Setting the value of an existing element does not impact its order in the collection.</remarks>
    public T this[int index]
    {
        get => GetAt(index);
        set => SetAt(index, value);
    }

    /// <summary>Insert the element at the specified index.</summary>
    /// <param name="index">The index at which to insert the pair, or -1 to append.</param>
    /// <param name="item">The item to insert.</param>
    /// <param name="behavior">
    /// The behavior controlling insertion behavior with respect to item duplication:
    /// - IgnoreInsertion: Immediately ends the operation, returning false, if the item already exists, e.g. Add(item)
    /// - ThrowOnExisting: If the item already exists, throws an exception, e.g. Insert(index, item)
    /// </param>
    /// <param name="itemIndex">The index of the added or existing item. This is always a valid index into the set.</param>
    /// <returns>true if the collection was updated; otherwise, false.</returns>
    private bool TryInsert(int index, T item, InsertionBehavior behavior, out int itemIndex)
    {
        // Search for the element in the dictionary.
        uint hashCode = 0, collisionCount = 0;
        int i = IndexOf(item, ref hashCode, ref collisionCount);

        // Handle the case where the element already exists, based on the requested behavior.
        if (i >= 0)
        {
            itemIndex = i;
            Debug.Assert(0 <= itemIndex && itemIndex < _count);

            Debug.Assert(_entries is not null);

            switch (behavior)
            {
                case InsertionBehavior.ThrowOnExisting:
                    ThrowHelper.ThrowDuplicateKey(item);
                    break;

                default:
                    Debug.Assert(behavior is InsertionBehavior.IgnoreInsertion, $"Unknown behavior: {behavior}");
                    Debug.Assert(index < 0, "Expected index to be unspecied when ignoring a duplicate element.");
                    return false;
            }
        }

        // The element doesn't exist. If a non-negative index was provided, that is the desired index at which to insert,
        // which should have already been validated by the caller. If negative, we're appending.
        if (index < 0)
        {
            index = _count;
        }
        Debug.Assert(index <= _count);

        // Ensure the collection has been initialized.
        if (_buckets is null)
        {
            EnsureBucketsAndEntriesInitialized(0);
        }

        // As we just initialized the collection, _entries must be non-null.
        Entry[]? entries = _entries;
        Debug.Assert(entries is not null);

        // Grow capacity if necessary to accomodate the extra entry.
        if (entries.Length == _count)
        {
            Resize(HashHelpers.ExpandPrime(entries.Length));
            entries = _entries;
        }

        // The _entries array is ordered, so we need to insert the new entry at the specified index. That means
        // not only shifting up all elements at that index and higher, but also updating the buckets and chains
        // to record the newly updated indices.
        for (i = _count - 1; i >= index; --i)
        {
            entries[i + 1] = entries[i];
            UpdateBucketIndex(i, shiftAmount: 1);
        }

        // Store the new element.
        ref Entry entry = ref entries[index];
        entry.HashCode = hashCode;
        entry.Value = item;
        PushEntryIntoBucket(ref entry, index);
        _count++;
        _version++;

        RehashIfNecessary(collisionCount, entries);

        itemIndex = index;
        Debug.Assert(0 <= itemIndex && itemIndex < _count);

        return true;
    }

    /// <summary>Adds the specified element to the set.</summary>
    /// <param name="item">The element to add.</param>
    /// <returns>true if the element is added; false if the element is already present.</returns>
    public bool Add(T item)
    {
        return TryInsert(index: -1, item, InsertionBehavior.IgnoreInsertion, out _);
    }

    /// <summary>Adds each element of the enumerable to the set.</summary>
    private void AddRange(IEnumerable<T> collection)
    {
        Debug.Assert(collection is not null);

        if (collection is T[] array)
        {
            foreach (T item in array)
            {
                Add(item);
            }
        }
        else
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }
    }

    /// <summary>Removes all elements from the <see cref="OrderedSet{T}"/>.</summary>
    public void Clear()
    {
        if (_buckets is not null && _count != 0)
        {
            Debug.Assert(_entries is not null);

            Array.Clear(_buckets, 0, _buckets.Length);
            Array.Clear(_entries, 0, _count);
            _count = 0;
            _version++;
        }
    }

    /// <summary>Determines whether the <see cref="OrderedSet{T}"/> contains the specified element.</summary>
    /// <param name="item">The element to locate in the <see cref="OrderedSet{T}"/>.</param>
    /// <returns>true if the <see cref="OrderedSet{T}"/> contains the specified element; otherwise, false.</returns>
    public bool Contains(T item)
    {
        int count = _count;

        Entry[]? entries = _entries;
        if (entries is null)
        {
            return false;
        }

        if (typeof(T).IsValueType)
        {
            for (int i = 0; i < count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(item, entries[i].Value))
                {
                    return true;
                }
            }
        }
        else
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(item, entries[i].Value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Gets the element at the specified index.</summary>
    /// <param name="index">The zero-based index of the pair to get.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
    private T GetAt(int index)
    {
        if ((uint)index >= (uint)_count)
        {
            ThrowHelper.ThrowIndexArgumentOutOfRange();
        }

        Debug.Assert(_entries is not null, "count must be positive, which means we must have entries");

        ref Entry e = ref _entries[index];
        return e.Value;
    }

    /// <summary>Determines the index of a specific element in the <see cref="OrderedSet{T}"/>.</summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>The index of <paramref name="item"/> if found; otherwise, -1.</returns>
    public int IndexOf(T item)
    {
        uint _ = 0;
        return IndexOf(item, ref _, ref _);
    }

    private int IndexOf(T item, ref uint outHashCode, ref uint outCollisionCount)
    {
        uint hashCode;
        uint collisionCount = 0;
        IEqualityComparer<T>? comparer = _comparer;

        if (_buckets is null)
        {
            hashCode = item is null ? 0 : (uint)(comparer?.GetHashCode(item) ?? item.GetHashCode());
            collisionCount = 0;
            goto ReturnNotFound;
        }

        int i = -1;
        ref Entry entry = ref Unsafe.NullRef<Entry>();

        Entry[]? entries = _entries;
        Debug.Assert(entries is not null, "expected entries to be is not null");

        if (typeof(T).IsValueType && // comparer can only be null for value types; enable JIT to eliminate entire if block for ref types
            comparer is null)
        {
            // ValueType: Devirtualize with EqualityComparer<T>.Default intrinsic

            hashCode = (uint)item!.GetHashCode();
            i = GetBucket(hashCode) - 1; // Value in _buckets is 1-based; subtract 1 from i. We do it here so it fuses with the following conditional.
            do
            {
                // Test in if to drop range check for following array access
                if ((uint)i >= (uint)entries.Length)
                {
                    goto ReturnNotFound;
                }

                entry = ref entries[i];
                if (entry.HashCode == hashCode && EqualityComparer<T>.Default.Equals(entry.Value, item))
                {
                    goto Return;
                }

                i = entry.Next;

                collisionCount++;
            }
            while (collisionCount <= (uint)entries.Length);

            // The chain of entries forms a loop; which means a concurrent update has happened.
            // Break out of the loop and throw, rather than looping forever.
            goto ConcurrentOperation;
        }
        else
        {
            Debug.Assert(comparer is not null);
            hashCode = item != null ? (uint)comparer.GetHashCode(item) : 0;
            i = GetBucket(hashCode) - 1; // Value in _buckets is 1-based; subtract 1 from i. We do it here so it fuses with the following conditional.
            do
            {
                // Test in if to drop range check for following array access
                if ((uint)i >= (uint)entries.Length)
                {
                    goto ReturnNotFound;
                }

                entry = ref entries[i];
                if (entry.HashCode == hashCode && comparer.Equals(entry.Value, item))
                {
                    goto Return;
                }

                i = entry.Next;

                collisionCount++;
            }
            while (collisionCount <= (uint)entries.Length);

            // The chain of entries forms a loop; which means a concurrent update has happened.
            // Break out of the loop and throw, rather than looping forever.
            goto ConcurrentOperation;
        }

        ReturnNotFound:
        i = -1;
        outCollisionCount = collisionCount;
        goto Return;

        ConcurrentOperation:
        // We examined more entries than are actually in the list, which means there's a cycle
        // that's caused by erroneous concurrent use.
        ThrowHelper.ThrowConcurrentOperation();

        Return:
        outHashCode = hashCode;
        return i;
    }

    /// <summary>Inserts an item into the collection at the specified index.</summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The element to insert.</param>
    /// <exception cref="ArgumentException">The element already exists in the <see cref="OrderedSet{T}"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.</exception>
    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)_count)
        {
            ThrowHelper.ThrowIndexArgumentOutOfRange();
        }

        TryInsert(index, item, InsertionBehavior.ThrowOnExisting, out _);
    }

    /// <summary>Removes the specified element from the <see cref="OrderedSet{T}"/>.</summary>
    /// <param name="item">The element to remove.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        // Find the element.
        int index = IndexOf(item);
        if (index >= 0)
        {
            // It exists. Remove it.
            Debug.Assert(_entries is not null);

            RemoveAt(index);

            return true;
        }

        return false;
    }

    /// <summary>Removes the element at the specified index of the <see cref="OrderedSet{T}"/>.</summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        int count = _count;
        if ((uint)index >= (uint)count)
        {
            ThrowHelper.ThrowIndexArgumentOutOfRange();
        }

        // Remove from the associated bucket chain the entry that lives at the specified index.
        RemoveEntryFromBucket(index);

        // Shift down all entries above this one, and fix up the bucket chains to reflect the new indices.
        Entry[]? entries = _entries;
        Debug.Assert(entries is not null);
        for (int i = index + 1; i < count; i++)
        {
            entries[i - 1] = entries[i];
            UpdateBucketIndex(i, shiftAmount: -1);
        }

        entries[--_count] = default;
        _version++;
    }

    /// <summary>Sets the value for the element at the specified index.</summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <param name="item">The item to store at the specified index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
    private void SetAt(int index, T item)
    {
        if ((uint)index >= (uint)_count)
        {
            ThrowHelper.ThrowIndexArgumentOutOfRange();
        }

        Debug.Assert(_entries is not null);
        ref Entry e = ref _entries[index];

        // If the element matches the one that's already in that slot, just update the value.
        if (typeof(T).IsValueType && _comparer is null)
        {
            if (EqualityComparer<T>.Default.Equals(item, e.Value))
            {
                e.Value = item;
                return;
            }
        }
        else
        {
            Debug.Assert(_comparer is not null);
            if (_comparer.Equals(item, e.Value))
            {
                e.Value = item;
                return;
            }
        }

        // The element doesn't match that index. If it exists elsewhere in the collection, fail.
        uint hashCode = 0, collisionCount = 0;
        if (IndexOf(item, ref hashCode, ref collisionCount) >= 0)
        {
            ThrowHelper.ThrowDuplicateKey(item);
        }

        // The element doesn't exist in the collection. Update the element, but also update
        // the bucket chains, as the new element may not hash to the same bucket as the old element
        // (we could check for this, but in a properly balanced set the chances should
        // be low for a match, so it's not worth it).
        RemoveEntryFromBucket(index);
        e.HashCode = hashCode;
        e.Value = item;
        PushEntryIntoBucket(ref e, index);

        _version++;

        RehashIfNecessary(collisionCount, _entries);
    }

    /// <summary>Ensures that the set can hold up to <paramref name="capacity"/> entries without resizing.</summary>
    /// <param name="capacity">The desired minimum capacity of the set. The actual capacity provided may be larger.</param>
    /// <returns>The new capacity of the set.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is negative.</exception>
    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (Capacity < capacity)
        {
            if (_buckets is null)
            {
                EnsureBucketsAndEntriesInitialized(capacity);
            }
            else
            {
                Resize(HashHelpers.GetPrime(capacity));
            }

            _version++;
        }

        return Capacity;
    }

    /// <summary>Sets the capacity of this dictionary to what it would be if it had been originally initialized with all its entries.</summary>
    public void TrimExcess() => TrimExcess(_count);

    /// <summary>Sets the capacity of this dictionary to hold up a specified number of entries without resizing.</summary>
    /// <param name="capacity">The desired capacity to which to shrink the set.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <see cref="Count"/>.</exception>
    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);

        int currentCapacity = _entries?.Length ?? 0;
        capacity = HashHelpers.GetPrime(capacity);
        if (capacity < currentCapacity)
        {
            Resize(capacity);
        }
    }

    /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">The value from the set that the search found, or the default value of <typeparamref name="T"/> when the search yielded no match.</param>
    /// <returns>A value indicating whether the search was successful.</returns>
    /// <remarks>
    /// This can be useful when you want to reuse a previously stored reference instead of
    /// a newly constructed one (so that more sharing of references can occur) or to look up
    /// a value that has more complete data than the value you currently have, although their
    /// comparer functions indicate they are equal.
    /// </remarks>
    public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue) =>
        TryGetValue(equalValue, out actualValue, out _);
 
    /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">The value from the set that the search found, or the default value of <typeparamref name="T"/> when the search yielded no match.</param>
    /// <param name="index">The index of <paramref name="actualValue"/> if found; otherwise, -1.</param>
    /// <returns>A value indicating whether the search was successful.</returns>
    /// <remarks>
    /// This can be useful when you want to reuse a previously stored reference instead of
    /// a newly constructed one (so that more sharing of references can occur) or to look up
    /// a value that has more complete data than the value you currently have, although their
    /// comparer functions indicate they are equal.
    /// </remarks>
    public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue, out int index)
    {
        // Find the element.
        index = IndexOf(equalValue);
        if (index >= 0)
        {
            // It exists. Return its value.
            Debug.Assert(_entries is not null);
            actualValue = _entries[index].Value;
            return true;
        }

        actualValue = default;
        return false;
    }

    /// <summary>Modifies the current <see cref="OrderedSet{T}"/> object to contain all elements that are present in itself, the specified collection, or both.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (T item in other)
        {
            TryInsert(index: -1, item, InsertionBehavior.IgnoreInsertion, out _);
        }
    }

    /// <summary>Modifies the current <see cref="OrderedSet{T}"/> object to contain only elements that are present in that object and in the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    public void IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Intersection of anything with empty set is empty set, so return if count is 0.
        // Same if the set intersecting with itself is the same set.
        if (Count == 0 || other == this)
        {
            return;
        }

        // If other is known to be empty, intersection is empty set; remove all elements, and we're done.
        if (other is ICollection<T> otherAsCollection)
        {
            if (otherAsCollection.Count == 0)
            {
                Clear();
                return;
            }

            // Faster if other is a set using same equality comparer; so check
            // that other is a set using the same equality comparer.
            if (other is OrderedSet<T> otherAsSet && EqualityComparersAreEqual(this, otherAsSet))
            {
                IntersectWithSetWithSameComparer(otherAsSet);
                return;
            }
        }

        IntersectWithEnumerable(other);
    }

    /// <summary>Removes all elements in the specified collection from the current <see cref="OrderedSet{T}"/> object.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    public void ExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // This is already the empty set; return.
        if (Count == 0)
        {
            return;
        }

        // Special case if other is this; a set minus itself is the empty set.
        if (other == this)
        {
            Clear();
            return;
        }

        // Remove every element in other from this.
        foreach (T element in other)
        {
            Remove(element);
        }
    }

    /// <summary>Modifies the current <see cref="OrderedSet{T}"/> object to contain only elements that are present either in that object or in the specified collection, but not both.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // If set is empty, then symmetric difference is other.
        if (Count == 0)
        {
            UnionWith(other);
            return;
        }

        // Special-case this; the symmetric difference of a set with itself is the empty set.
        if (other == this)
        {
            Clear();
            return;
        }

        // If other is a OrderedSet, it has unique elements according to its equality comparer,
        // but if they're using different equality comparers, then assumption of uniqueness
        // will fail. So first check if other is a set using the same equality comparer;
        // symmetric except is a lot faster and avoids bit array allocations if we can assume
        // uniqueness.
        if (other is OrderedSet<T> otherAsSet && EqualityComparersAreEqual(this, otherAsSet))
        {
            SymmetricExceptWithUniqueSet(otherAsSet);
        }
        else
        {
            SymmetricExceptWithEnumerable(other);
        }
    }

    /// <summary>Determines whether a <see cref="OrderedSet{T}"/> object is a subset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    /// <returns>true if the <see cref="OrderedSet{T}"/> object is a subset of <paramref name="other"/>; otherwise, false.</returns>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // The empty set is a subset of any set, and a set is a subset of itself.
        // Set is always a subset of itself.
        if (Count == 0 || other == this)
        {
            return true;
        }

        if (other is ICollection<T> otherAsCollection)
        {
            // If this has more elements then it can't be a subset.
            if (Count > otherAsCollection.Count)
            {
                return false;
            }

            // Faster if other has unique elements according to this equality comparer; so check
            // that other is a set using the same equality comparer.
            if (other is OrderedSet<T> otherAsSet && EqualityComparersAreEqual(this, otherAsSet))
            {
                return IsSubsetOfSetWithSameComparer(otherAsSet);
            }
        }

        (int uniqueCount, int unfoundCount) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
        return uniqueCount == Count && unfoundCount >= 0;
    }

    /// <summary>Determines whether a <see cref="OrderedSet{T}"/> object is a proper subset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    /// <returns>true if the <see cref="OrderedSet{T}"/> object is a proper subset of <paramref name="other"/>; otherwise, false.</returns>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // No set is a proper subset of itself.
        if (other == this)
        {
            return false;
        }

        if (other is ICollection<T> otherAsCollection)
        {
            // No set is a proper subset of a set with less or equal number of elements.
            if (otherAsCollection.Count <= Count)
            {
                return false;
            }

            // The empty set is a proper subset of anything but the empty set.
            if (Count == 0)
            {
                // Based on check above, other is not empty when Count == 0.
                return true;
            }

            // Faster if other is a set (and we're using same equality comparer).
            if (other is OrderedSet<T> otherAsSet && EqualityComparersAreEqual(this, otherAsSet))
            {
                // This has strictly less than number of items in other, so the following
                // check suffices for proper subset.
                return IsSubsetOfSetWithSameComparer(otherAsSet);
            }
        }

        (int uniqueCount, int unfoundCount) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: false);
        return uniqueCount == Count && unfoundCount > 0;
    }

    /// <summary>Determines whether a <see cref="OrderedSet{T}"/> object is a proper superset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    /// <returns>true if the <see cref="OrderedSet{T}"/> object is a superset of <paramref name="other"/>; otherwise, false.</returns>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // A set is always a superset of itself.
        if (other == this)
        {
            return true;
        }

        // Try to fall out early based on counts.
        if (other is ICollection<T> otherAsCollection)
        {
            // If other is the empty set then this is a superset.
            if (otherAsCollection.Count == 0)
            {
                return true;
            }

            // Try to compare based on counts alone if other is a set with same equality comparer.
            if (other is OrderedSet<T> otherAsSet &&
                EqualityComparersAreEqual(this, otherAsSet) &&
                otherAsSet.Count > Count)
            {
                return false;
            }
        }

        foreach (T element in other)
        {
            if (!Contains(element))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether a <see cref="OrderedSet{T}"/> object is a proper superset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    /// <returns>true if the <see cref="OrderedSet{T}"/> object is a proper superset of <paramref name="other"/>; otherwise, false.</returns>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // The empty set isn't a proper superset of any set, and a set is never a strict superset of itself.
        if (Count == 0 || other == this)
        {
            return false;
        }

        if (other is ICollection<T> otherAsCollection)
        {
            // If other is the empty set then this is a superset.
            if (otherAsCollection.Count == 0)
            {
                // Note that this has at least one element, based on above check.
                return true;
            }

            // Faster if other is a set with the same equality comparer
            if (other is OrderedSet<T> otherAsSet && EqualityComparersAreEqual(this, otherAsSet))
            {
                if (otherAsSet.Count >= Count)
                {
                    return false;
                }

                // Now perform element check.
                return otherAsSet.IsSubsetOfSetWithSameComparer(this);
            }
        }

        // Couldn't fall out in the above cases; do it the long way
        (int uniqueCount, int unfoundCount) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
        return uniqueCount < Count && unfoundCount == 0;
    }

    /// <summary>Determines whether the current <see cref="OrderedSet{T}"/> object and a specified collection share common elements.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    /// <returns>true if the <see cref="OrderedSet{T}"/> object and <paramref name="other"/> share at least one common element; otherwise, false.</returns>
    public bool Overlaps(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Count == 0)
        {
            return false;
        }

        // Set overlaps itself
        if (other == this)
        {
            return true;
        }

        foreach (T element in other)
        {
            if (Contains(element))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether a <see cref="OrderedSet{T}"/> object and the specified collection contain the same elements.</summary>
    /// <param name="other">The collection to compare to the current <see cref="OrderedSet{T}"/> object.</param>
    /// <returns>true if the <see cref="OrderedSet{T}"/> object is equal to <paramref name="other"/>; otherwise, false.</returns>
    public bool SetEquals(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // A set is equal to itself.
        if (other == this)
        {
            return true;
        }

        if (other is ICollection<T> otherAsCollection)
        {
            // If this is empty, they are equal iff other is empty.
            if (Count == 0)
            {
                return otherAsCollection.Count == 0;
            }

            // Faster if other is a set and we're using same equality comparer.
            if (other is OrderedSet<T> otherAsSet && EqualityComparersAreEqual(this, otherAsSet))
            {
                // Attempt to return early: since both contain unique elements, if they have
                // different counts, then they can't be equal.
                if (Count != otherAsSet.Count)
                {
                    return false;
                }

                // Already confirmed that the sets have the same number of distinct elements, so if
                // one is a subset of the other then they must be equal.
                return IsSubsetOfSetWithSameComparer(otherAsSet);
            }

            // Can't be equal if other set contains fewer elements than this.
            if (Count > otherAsCollection.Count)
            {
                return false;
            }
        }

        (int uniqueCount, int unfoundCount) = CheckUniqueAndUnfoundElements(other, returnIfUnfound: true);
        return uniqueCount == Count && unfoundCount == 0;
    }

    /// <summary>Pushes the entry into its bucket.</summary>
    /// <remarks>
    /// The bucket is a linked list by index into the <see cref="_entries"/> array.
    /// The new entry's <see cref="Entry.Next"/> is set to the bucket's current
    /// head, and then the new entry is made the new head.
    /// </remarks>
    private void PushEntryIntoBucket(ref Entry entry, int entryIndex)
    {
        ref int bucket = ref GetBucket(entry.HashCode);
        entry.Next = bucket - 1;
        bucket = entryIndex + 1;
    }

    /// <summary>Removes an entry from its bucket.</summary>
    private void RemoveEntryFromBucket(int entryIndex)
    {
        // We're only calling this method if there's an entry to be removed, in which case
        // entries must have been initialized.
        Entry[]? entries = _entries;
        Debug.Assert(entries is not null);

        // Get the entry to be removed and the associated bucket.
        Entry entry = entries[entryIndex];
        ref int bucket = ref GetBucket(entry.HashCode);

        if (bucket == entryIndex + 1)
        {
            // If the entry was at the head of its bucket list, to remove it from the list we
            // simply need to update the next entry in the list to be the new head.
            bucket = entry.Next + 1;
        }
        else
        {
            // The entry wasn't the head of the list. Walk the chain until we find the entry,
            // updating the previous entry's Next to point to this entry's Next.
            int i = bucket - 1;
            int collisionCount = 0;
            while (true)
            {
                ref Entry e = ref entries[i];
                if (e.Next == entryIndex)
                {
                    e.Next = entry.Next;
                    return;
                }

                i = e.Next;

                if (++collisionCount > entries.Length)
                {
                    // We examined more entries than are actually in the list, which means there's a cycle
                    // that's caused by erroneous concurrent use.
                    ThrowHelper.ThrowConcurrentOperation();
                }
            }
        }
    }

    /// <summary>
    /// Updates the bucket chain containing the specified entry (by index) to shift indices
    /// by the specified amount.
    /// </summary>
    /// <param name="entryIndex">The index of the target entry.</param>
    /// <param name="shiftAmount">
    /// 1 if this is part of an insert and the values are being shifted one higher.
    /// -1 if this is part of a remove and the values are being shifted one lower.
    /// </param>
    private void UpdateBucketIndex(int entryIndex, int shiftAmount)
    {
        Debug.Assert(shiftAmount is 1 or -1);

        Entry[]? entries = _entries;
        Debug.Assert(entries is not null);

        Entry entry = entries[entryIndex];
        ref int bucket = ref GetBucket(entry.HashCode);

        if (bucket == entryIndex + 1)
        {
            // If the entry was at the head of its bucket list, the only thing that needs to be updated
            // is the bucket head value itself, since no other entries' Next will be referencing this node.
            bucket += shiftAmount;
        }
        else
        {
            // The entry wasn't the head of the list. Walk the chain until we find the entry, updating
            // the previous entry's Next that's pointing to the target entry.
            int i = bucket - 1;
            int collisionCount = 0;
            while (true)
            {
                ref Entry e = ref entries[i];
                if (e.Next == entryIndex)
                {
                    e.Next += shiftAmount;
                    return;
                }

                i = e.Next;

                if (++collisionCount > entries.Length)
                {
                    // We examined more entries than are actually in the list, which means there's a cycle
                    // that's caused by erroneous concurrent use.
                    ThrowHelper.ThrowConcurrentOperation();
                }
            }
        }
    }

    /// <summary>
    /// Checks to see whether the collision count that occurred during lookup warrants upgrading to a non-randomized comparer,
    /// and does so if necessary.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Is no-op in certain targets")]
    private void RehashIfNecessary(uint collisionCount, Entry[] entries)
    {
#if SYSTEM_COLLECTIONS
        // If we exceeded the hash collision threshold and we're using a randomized comparer, rehash.
        // This is only ever done for string elements, so we can optimize it all away for value type elements.
        if (!typeof(T).IsValueType &&
            collisionCount > HashHelpers.HashCollisionThreshold &&
            _comparer is NonRandomizedStringEqualityComparer)
        {
            // Switch to a randomized comparer and rehash.
            Resize(entries.Length, forceNewHashCodes: true);
        }
#endif
    }

    /// <summary>Grow or shrink <see cref="_buckets"/> and <see cref="_entries"/> to the specified capacity.</summary>
    [MemberNotNull(nameof(_buckets))]
    [MemberNotNull(nameof(_entries))]
    private void Resize(int newSize, bool forceNewHashCodes = false)
    {
        Debug.Assert(!forceNewHashCodes || !typeof(T).IsValueType, "Value types never rehash.");
        Debug.Assert(newSize >= _count, "The requested size must accomodate all of the current elements.");

        // Create the new arrays. We allocate both prior to storing either; in case one of the allocation fails,
        // we want to avoid corrupting the data structure.
        int[] newBuckets = new int[newSize];
        Entry[] newEntries = new Entry[newSize];
        if (IntPtr.Size == 8)
        {
            // Any time the capacity changes, that impacts the divisor of modulo operations,
            // and we need to update our fast modulo multiplier.
            _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newSize);
        }

        // Copy the existing entries to the new entries array.
        int count = _count;
        if (_entries is not null)
        {
            Array.Copy(_entries, newEntries, count);
        }

#if SYSTEM_COLLECTIONS
        // If we're being asked to upgrade to a non-randomized comparer due to too many collisions, do so.
        if (!typeof(T).IsValueType && forceNewHashCodes)
        {
            // Store the original randomized comparer instead of the non-randomized one.
            Debug.Assert(_comparer is NonRandomizedStringEqualityComparer);
            IEqualityComparer<T> comparer = _comparer = (IEqualityComparer<T>)((NonRandomizedStringEqualityComparer)_comparer).GetUnderlyingEqualityComparer();
            Debug.Assert(_comparer is not null);
            Debug.Assert(_comparer is not NonRandomizedStringEqualityComparer);

            // Update all the entries' hash codes based on the new comparer.
            for (int i = 0; i < count; i++)
            {
                ref Entry entry = ref newEntries[i];
                entry.HashCode = entry.Value != null ? (uint)comparer.GetHashCode(entry.Value) : 0;
            }
        }
#endif

        // Now publish the buckets array. It's necessary to do this prior to the below loop,
        // as PushEntryIntoBucket will be populating _buckets.
        _buckets = newBuckets;

        // Populate the buckets.
        for (int i = 0; i < count; i++)
        {
            PushEntryIntoBucket(ref newEntries[i], i);
        }

        _entries = newEntries;
    }

    /// <summary>Gets the bucket assigned to the specified hash code.</summary>
    /// <remarks>
    /// Buckets are 1-based. This is so that the default initialized value of 0
    /// maps to -1 and is usable as a sentinel.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucket(uint hashCode)
    {
        int[]? buckets = _buckets;
        Debug.Assert(buckets is not null);

        if (IntPtr.Size == 8)
        {
            return ref buckets[HashHelpers.FastMod(hashCode, (uint)buckets.Length, _fastModMultiplier)];
        }
        else
        {
            return ref buckets[(uint)hashCode % buckets.Length];
        }
    }

    /// <summary>
    /// Implementation Notes:
    /// If other is a set and is using same equality comparer, then checking subset is
    /// faster. Simply check that each element in this is in other.
    ///
    /// Note: if other doesn't use same equality comparer, then Contains check is invalid,
    /// which is why callers must take are of this.
    ///
    /// If callers are concerned about whether this is a proper subset, they take care of that.
    /// </summary>
    internal bool IsSubsetOfSetWithSameComparer(OrderedSet<T> other)
    {
        foreach (T item in this)
        {
            if (!other.Contains(item))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// If other is a set that uses same equality comparer, intersect is much faster
    /// because we can use other's Contains
    /// </summary>
    private void IntersectWithSetWithSameComparer(OrderedSet<T> other)
    {
        Entry[]? entries = _entries;
        for (int i = 0; i < _count; i++)
        {
            ref Entry entry = ref entries![i];
            if (entry.Next >= -1)
            {
                T item = entry.Value;
                if (!other.Contains(item))
                {
                    Remove(item);
                }
            }
        }
    }

    /// <summary>
    /// Iterate over other. If contained in this, mark an element in bit array corresponding to
    /// its position in _slots. If anything is unmarked (in bit array), remove it.
    ///
    /// This attempts to allocate on the stack, if below StackAllocThreshold.
    /// </summary>
    private void IntersectWithEnumerable(IEnumerable<T> other)
    {
        Debug.Assert(_buckets != null, "_buckets shouldn't be null; callers should check first");

        // Keep track of current last index; don't want to move past the end of our bit array
        // (could happen if another thread is modifying the collection).
        int originalCount = _count;
        int intArrayLength = BitHelper.ToIntArrayLength(originalCount);

        Span<int> span = stackalloc int[StackAllocThreshold];
        BitHelper bitHelper = intArrayLength <= StackAllocThreshold
            ? new BitHelper(span.Slice(0, intArrayLength), clear: true)
            : new BitHelper(new int[intArrayLength], clear: false);

        // Mark if contains: find index of in slots array and mark corresponding element in bit array.
        foreach (T item in other)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                bitHelper.MarkBit(index);
            }
        }

        // If anything unmarked, remove it. Perf can be optimized here if BitHelper had a
        // FindFirstUnmarked method.
        for (int i = 0; i < originalCount; i++)
        {
            ref Entry entry = ref _entries![i];
            if (entry.Next >= -1 && !bitHelper.IsMarked(i))
            {
                Remove(entry.Value);
            }
        }
    }

    /// <summary>
    /// if other is a set, we can assume it doesn't have duplicate elements, so use this
    /// technique: if can't remove, then it wasn't present in this set, so add.
    ///
    /// As with other methods, callers take care of ensuring that other is a set using the
    /// same equality comparer.
    /// </summary>
    /// <param name="other"></param>
    private void SymmetricExceptWithUniqueSet(OrderedSet<T> other)
    {
        foreach (T item in other)
        {
            if (!Remove(item))
            {
                TryInsert(index: -1, item, InsertionBehavior.IgnoreInsertion, out _);
            }
        }
    }

    /// <summary>
    /// Implementation notes:
    ///
    /// Used for symmetric except when other isn't a set. This is more tedious because
    /// other may contain duplicates. set technique could fail in these situations:
    /// 1. Other has a duplicate that's not in this: set technique would add then
    /// remove it.
    /// 2. Other has a duplicate that's in this: set technique would remove then add it
    /// back.
    /// In general, its presence would be toggled each time it appears in other.
    ///
    /// This technique uses bit marking to indicate whether to add/remove the item. If already
    /// present in collection, it will get marked for deletion. If added from other, it will
    /// get marked as something not to remove.
    ///
    /// </summary>
    /// <param name="other"></param>
    private void SymmetricExceptWithEnumerable(IEnumerable<T> other)
    {
        int originalCount = _count;
        int intArrayLength = BitHelper.ToIntArrayLength(originalCount);

        Span<int> itemsToRemoveSpan = stackalloc int[StackAllocThreshold / 2];
        BitHelper itemsToRemove = intArrayLength <= StackAllocThreshold / 2
            ? new BitHelper(itemsToRemoveSpan.Slice(0, intArrayLength), clear: true)
            : new BitHelper(new int[intArrayLength], clear: false);

        Span<int> itemsAddedFromOtherSpan = stackalloc int[StackAllocThreshold / 2];
        BitHelper itemsAddedFromOther = intArrayLength <= StackAllocThreshold / 2
            ? new BitHelper(itemsAddedFromOtherSpan.Slice(0, intArrayLength), clear: true)
            : new BitHelper(new int[intArrayLength], clear: false);

        foreach (T item in other)
        {
            int location;
            if (TryInsert(index: -1, item, InsertionBehavior.IgnoreInsertion, out location))
            {
                // wasn't already present in collection; flag it as something not to remove
                // *NOTE* if location is out of range, we should ignore. BitHelper will
                // detect that it's out of bounds and not try to mark it. But it's
                // expected that location could be out of bounds because adding the item
                // will increase _lastIndex as soon as all the free spots are filled.
                itemsAddedFromOther.MarkBit(location);
            }
            else
            {
                // already there...if not added from other, mark for remove.
                // *NOTE* Even though BitHelper will check that location is in range, we want
                // to check here. There's no point in checking items beyond originalCount
                // because they could not have been in the original collection
                if (location < originalCount && !itemsAddedFromOther.IsMarked(location))
                {
                    itemsToRemove.MarkBit(location);
                }
            }
        }

        // if anything marked, remove it
        for (int i = 0; i < originalCount; i++)
        {
            if (itemsToRemove.IsMarked(i))
            {
                Remove(_entries![i].Value);
            }
        }
    }

    /// <summary>
    /// Determines counts that can be used to determine equality, subset, and superset. This
    /// is only used when other is an IEnumerable and not a set. If other is a set
    /// these properties can be checked faster without use of marking because we can assume
    /// other has no duplicates.
    ///
    /// The following count checks are performed by callers:
    /// 1. Equals: checks if unfoundCount = 0 and uniqueFoundCount = _count; i.e. everything
    /// in other is in this and everything in this is in other
    /// 2. Subset: checks if unfoundCount >= 0 and uniqueFoundCount = _count; i.e. other may
    /// have elements not in this and everything in this is in other
    /// 3. Proper subset: checks if unfoundCount > 0 and uniqueFoundCount = _count; i.e
    /// other must have at least one element not in this and everything in this is in other
    /// 4. Proper superset: checks if unfound count = 0 and uniqueFoundCount strictly less
    /// than _count; i.e. everything in other was in this and this had at least one element
    /// not contained in other.
    ///
    /// An earlier implementation used delegates to perform these checks rather than returning
    /// an ElementCount struct; however this was changed due to the perf overhead of delegates.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="returnIfUnfound">Allows us to finish faster for equals and proper superset
    /// because unfoundCount must be 0.</param>
    private (int UniqueCount, int UnfoundCount) CheckUniqueAndUnfoundElements(IEnumerable<T> other,
        bool returnIfUnfound)
    {
        // Need special case in case this has no elements.
        if (_count == 0)
        {
            int numElementsInOther = 0;
            foreach (T _ in other)
            {
                numElementsInOther++;
                break; // break right away, all we want to know is whether other has 0 or 1 elements
            }

            return (UniqueCount: 0, UnfoundCount: numElementsInOther);
        }

        Debug.Assert((_buckets != null) && (_count > 0), "_buckets was null but count greater than 0");

        int originalCount = _count;
        int intArrayLength = BitHelper.ToIntArrayLength(originalCount);

        Span<int> span = stackalloc int[StackAllocThreshold];
        BitHelper bitHelper = intArrayLength <= StackAllocThreshold
            ? new BitHelper(span.Slice(0, intArrayLength), clear: true)
            : new BitHelper(new int[intArrayLength], clear: false);

        int unfoundCount = 0; // count of items in other not found in this
        int uniqueFoundCount = 0; // count of unique items in other found in this

        foreach (T item in other)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                if (!bitHelper.IsMarked(index))
                {
                    // Item hasn't been seen yet.
                    bitHelper.MarkBit(index);
                    uniqueFoundCount++;
                }
            }
            else
            {
                unfoundCount++;
                if (returnIfUnfound)
                {
                    break;
                }
            }
        }

        return (uniqueFoundCount, unfoundCount);
    }

    /// <summary>
    /// Checks if equality comparers are equal. This is used for algorithms that can
    /// speed up if it knows the other item has unique elements. I.e. if they're using
    /// different equality comparers, then uniqueness assumption between sets break.
    /// </summary>
    internal static bool EqualityComparersAreEqual(OrderedSet<T> set1, OrderedSet<T> set2) =>
        set1.Comparer.Equals(set2.Comparer);

    /// <summary>Returns an enumerator that iterates through the <see cref="OrderedSet{T}"/>.</summary>
    /// <returns>A <see cref="OrderedSet{T}.Enumerator"/> structure for the <see cref="OrderedSet{T}"/>.</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
        Count == 0 ? EnumerableHelpers.GetEmptyEnumerator<T>() :
            GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

    /// <inheritdoc/>
    void ICollection<T>.Add(T item) => Add(item);

    /// <inheritdoc/>
    bool ICollection<T>.Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    /// <inheritdoc/>
    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        if (array.Length - arrayIndex < _count)
        {
            throw new ArgumentException(Strings.Arg_ArrayPlusOffTooSmall);
        }

        for (int i = 0; i < _count; i++)
        {
            ref Entry entry = ref _entries![i];
            array[arrayIndex++] = entry.Value;
        }
    }

    /// <inheritdoc/>
    bool ICollection<T>.Remove(T item) => Remove(item);

    /// <summary>Represents an element in the set.</summary>
    private struct Entry
    {
        /// <summary>The index of the next entry in the chain, or -1 if this is the last entry in the chain.</summary>
        public int Next;
        /// <summary>Cached hash code of <see cref="Value"/>.</summary>
        public uint HashCode;
        /// <summary>The value.</summary>
        public T Value;
    }

    /// <summary>Enumerates the elements of a <see cref="OrderedSet{T}"/>.</summary>
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator : IEnumerator<T>
    {
        /// <summary>The dictionary being enumerated.</summary>
        private readonly OrderedSet<T> _set;
        /// <summary>A snapshot of the dictionary's version when enumeration began.</summary>
        private readonly int _version;
        /// <summary>The current index.</summary>
        private int _index;

        /// <summary>Initialize the enumerator.</summary>
        internal Enumerator(OrderedSet<T> set)
        {
            _set = set;
            _version = _set._version;
            Current = default!;
        }

        /// <inheritdoc/>
        public T Current { get; private set; }

        /// <inheritdoc/>
        readonly object? IEnumerator.Current => Current;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            OrderedSet<T> set = _set;

            if (_version != set._version)
            {
                ThrowHelper.ThrowVersionCheckFailed();
            }

            if (_index < set._count)
            {
                Debug.Assert(set._entries is not null);
                ref Entry entry = ref set._entries[_index];
                Current = entry.Value;
                _index++;
                return true;
            }

            Current = default!;
            return false;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset()
        {
            if (_version != _set._version)
            {
                ThrowHelper.ThrowVersionCheckFailed();
            }

            _index = 0;
            Current = default!;
        }

        /// <inheritdoc/>
        readonly void IDisposable.Dispose() { }
    }
}

/// <summary>Used to control behavior of insertion into a <see cref="OrderedSet{T}"/>.</summary>
/// <remarks>Not nested in <see cref="OrderedSet{T}"/> to avoid multiple generic instantiations.</remarks>
internal enum InsertionBehavior
{
    /// <summary>Skip the insertion operation.</summary>
    IgnoreInsertion = 0,

    /// <summary>Specifies that if an existing entry with the same element is encountered, an exception should be thrown.</summary>
    ThrowOnExisting = 2
}
