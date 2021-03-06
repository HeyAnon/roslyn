﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Concepts;
using System.Concepts.Countable;
using System.Concepts.Enumerable;


/*
 What we need to implement.
 
 7.16.3 The query expression pattern
The Query expression pattern establishes a pattern of methods that types can implement to support query expressions. Because query expressions are translated to method invocations by means of a syntactic mapping, types have considerable flexibility in how they implement the query expression pattern. For example, the methods of the pattern can be implemented as instance methods or as extension methods because the two have the same invocation syntax, and the methods can request delegates or expression trees because anonymous functions are convertible to both.
The recommended shape of a generic type C<T> that supports the query expression pattern is shown below. A generic type is used in order to illustrate the proper relationships between parameter and result types, but it is possible to implement the pattern for non-generic types as well.
delegate R Func<T1,R>(T1 arg1);
delegate R Func<T1,T2,R>(T1 arg1, T2 arg2);
class C
{
	public C<T> Cast<T>();
}
class C<T> : C
{
	public C<T> Where(Func<T,bool> predicate);
	public C<U> Select<U>(Func<T,U> selector);
	public C<V> SelectMany<U,V>(Func<T,C<U>> selector,
		Func<T,U,V> resultSelector);
	public C<V> Join<U,K,V>(C<U> inner, Func<T,K> outerKeySelector,
		Func<U,K> innerKeySelector, Func<T,U,V> resultSelector);
	public C<V> GroupJoin<U,K,V>(C<U> inner, Func<T,K> outerKeySelector,
		Func<U,K> innerKeySelector, Func<T,C<U>,V> resultSelector);
	public O<T> OrderBy<K>(Func<T,K> keySelector);
	public O<T> OrderByDescending<K>(Func<T,K> keySelector);
	public C<G<K,T>> GroupBy<K>(Func<T,K> keySelector);
	public C<G<K,E>> GroupBy<K,E>(Func<T,K> keySelector,
		Func<T,E> elementSelector);
}
class O<T> : C<T>
{
	public O<T> ThenBy<K>(Func<T,K> keySelector);
	public O<T> ThenByDescending<K>(Func<T,K> keySelector);
}
class G<K,T> : C<T>
{
	public K Key { get; }
}
The methods above use the generic delegate types Func<T1, R> and Func<T1, T2, R>, but they could equally well have used other delegate or expression tree types with the same relationships in parameter and result types.
Notice the recommended relationship between C<T> and O<T> which ensures that the ThenBy and ThenByDescending methods are available only on the result of an OrderBy or OrderByDescending. Also notice the recommended shape of the result of GroupBy—a sequence of sequences, where each inner sequence has an additional Key property.
The System.Linq namespace provides an implementation of the query operator pattern for any type that implements the System.Collections.Generic.IEnumerable<T> interface.

 */

namespace TinyLinq
{
    /// <summary>
    /// Concept for types that can be converted to arrays.
    /// </summary>
    /// <typeparam name="TFrom">
    /// Type that is being converted to an array.
    /// </typeparam>
    /// <typeparam name="TElem">
    /// Type of elements in the array.
    /// </typeparam>
    public concept CToArray<TFrom, [AssociatedType] TElem>
    {
        /// <summary>
        /// Converts the argument to an array.
        /// </summary>
        /// <param name="from">
        /// The object from which we are converting.
        /// </param>
        /// <returns>
        /// The array resulting from <paramref name="from"/>.
        /// This may be the same object as <paramref name="from"/>.
        /// </returns>
        TElem[] ToArray(this TFrom from);
    }

    /// <summary>Baseline to-array for enumerables.</summary>
    [Overlappable]
    public instance ToArray_Enumerable<TColl, [AssociatedType]TEnum, [AssociatedType]TElem, implicit Eb, implicit Et>
        : CToArray<TColl, TElem>
        where Eb : CEnumerable<TColl, TEnum>
        where Et : CEnumerator<TEnum, TElem>
    {
        TElem[] ToArray(this TColl from)
        {
            var e = from.RefGetEnumerator();
            var q = new Queue<TElem>();
            while (e.MoveNext())
            {
                q.Enqueue(e.Current());
            }
            return q.ToArray();
        }
    }

    /// <summary>Optimised to-array for enumerables with O(1) count.</summary>
    [Overlappable]
    public instance ToArray_SCEnumerable<TColl, [AssociatedType]TEnum, [AssociatedType]TElem, implicit S, implicit Eb, implicit Et>
        where S : CStaticCountable<TColl>
        where Eb : CEnumerable<TColl, TEnum>
        where Et : CEnumerator<TEnum, TElem>
    {
        TElem[] ToArray(this TColl from)
        {
            var len = S.Count(from); // TODO: why doesn't e.Count work?
            var result = new TElem[len];
            var e = from.RefGetEnumerator();
            for (var i = 0; i < len; i++)
            {
                e.MoveNext();
                result[i] = e.Current();
            }
            return result;
        }
    }
}
