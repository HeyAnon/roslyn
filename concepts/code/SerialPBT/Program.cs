﻿using System;
using System.Concepts;
using System.Concepts.Showable;
using System.Linq;

/// <summary>
/// A small serial property-based testing library, mostly based on the paper
/// 'SmallCheck and LazySmallCheck: automatic exhaustive testing for small
/// values' by Runciman et al.
///
/// This example is based entirely on the paper with some tweaks and gap
/// filling.  It uses no code from any existing property-based testing
/// library, nor is it inspired by any.
/// </summary>
namespace SerialPBT
{

    static class Extensions
    {
        public static Imp<TL,TR> Implies<TL, TR, [AssociatedType] RL, [AssociatedType] RR, implicit TestableL, implicit TestableR>(this TL filter, TR property)
            where TestableL : CTestable<TL, RL>
            where TestableR : CTestable<TR, RR> =>
            new Imp<TL, TR> { filter=filter, property = property };
    }

    public static class PBTHelpers
    {
        public static Imp<TL, TR> Implies<TL, TR, [AssociatedType] RL, [AssociatedType] RR, implicit TestableL, implicit TestableR>(TL filter, TR property)
            where TestableL : CTestable<TL, RL>
            where TestableR : CTestable<TR, RR> =>
            new Imp<TL, TR> { filter = filter, property = property };

        public static void Check<T, [AssociatedType] R, implicit TestableT, implicit ShowableR>(T test, int depth, bool throwOnFailure = true)
            where TestableT : CTestable<T, R>
            where ShowableR : CShowable<R>
        {
            var name = TestableT.Name(test);
            Console.WriteLine(name);
            Console.WriteLine(new string('=', name.Length));
            Console.WriteLine();
            var result = TestableT.Test(test, depth);
            Console.WriteLine(Helpers.String(result));
            if (throwOnFailure && result.Failed)
            {
                throw new TestFailedException();
            }
        }
    }


    class Program
    {


        static bool Prop_IsSingleDigit(int num) => -10 < num && num < 10;

        static Exists<int, bool> Prop_IsSingleDigit_Exists =>
            new Exists<int, bool>(Prop_IsSingleDigit);
        // Exists((int num) => Prop_IsSingleDigit(num);

        static Imp<bool, Func<bool>> Prop_IsSingleDigit_Imp(int x) =>
            (x == 0).Implies((Func<bool>)(() => Prop_IsSingleDigit(x)));

        // There is more than one way to do it...

        static Imp<bool, Lazy<bool>> Prop_IsSingleDigit_ImpLazy(int x) =>
            (x == 0).Implies(new Lazy<bool>(() => Prop_IsSingleDigit(x)));

        static Func<int, Imp<bool, Lazy<bool>>> Prop_IsSingleDigit_Filtered =>
            ModifierHelpers.Filter((int y) => y == 0, Prop_IsSingleDigit);


        /// <summary>
        /// Broken function that checks whether one array is a prefix of
        /// another.
        /// </summary>
        /// <param name="xs">The smaller array.</param>
        /// <param name="ys">The larger array.</param>
        /// <returns>
        /// In theory, true if and only if <paramref name="xs"/> is a
        /// prefix of <paramref name="ys"/>.
        /// In practice, there are deliberate bugs.
        /// </returns>
        static bool IsPrefixFaulty(int[] xs, int[] ys)
        {
            for (var i = 0; ; i++)
            {
                if (xs.Length <= i)
                {
                    return true;
                }
                if (ys.Length <= i)
                {
                    return false;
                }
                if (xs[i] == ys[i])
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Checks whether one array is a prefix of another.
        /// </summary>
        /// <param name="xs">The smaller array.</param>
        /// <param name="ys">The larger array.</param>
        /// <returns>
        /// True if and only if <paramref name="xs"/> is a prefix of
        /// <paramref name="ys"/>.
        /// </returns>
        static bool IsPrefixValid(int[] xs, int[] ys)
        {
            if (ys.Length < xs.Length)
            {
                return false;
            }
            return Enumerable.SequenceEqual(xs, Enumerable.Take(ys, xs.Length));
        }

        class IsPrefixTests<A>
        {
            Func<A[], A[], bool> isPrefix_Impl;

            public IsPrefixTests(Func<A[], A[], bool> impl)
            {
                isPrefix_Impl = impl;
            }
            public bool Prop_IsPrefix(A[] xs, A[] ys)
            {
                var zs = new A[xs.Length + ys.Length];
                Array.Copy(xs, zs, xs.Length);
                Array.Copy(ys, 0, zs, xs.Length, ys.Length);
                return isPrefix_Impl(xs, zs);
            }

            public Imp<bool, Exists<A[], bool>> Prop_IsPrefix_Sound(A[] xs, A[] ys) =>
                new Imp<bool, Exists<A[], bool>>
                {
                    filter = isPrefix_Impl(xs, ys),
                    property =
                    new Exists<A[], bool>((rest) => Enumerable.SequenceEqual(xs.Concat(rest), ys))
                };
        }

        static class ShowableHelpers
        {

            /// <summary>
            /// Outputs a showable item to the console with a newline.
            /// </summary>
            /// <typeparam name="A">
            /// Type of showable items.
            /// </typeparam>
            /// <typeparam name="ShowableA">
            /// The CShowable instance for the item.
            /// </typeparam>
            /// <param name="a">
            /// The item to show.
            /// </param>
            public static void WriteLine<A, implicit ShowableA>(A a)
                where ShowableA : CShowable<A>
            {
                Console.WriteLine(System.Concepts.Showable.Helpers.String(a));
            }
        }

            static void Main(string[] args)
        {
            // TODO: the concept inferrer can't handle implicit conversions
            // so we have to add the type annotation for Check when we pass a
            // method as a Func.
            
            // We can test simple predicates...
            PBTHelpers.Check(Prop_IsSingleDigit, 9);
            PBTHelpers.Check(Prop_IsSingleDigit, 11);

            // ...existentials, and implications.
            PBTHelpers.Check(Prop_IsSingleDigit_Exists, 11);
            PBTHelpers.Check(Prop_IsSingleDigit_Imp, 11);
            PBTHelpers.Check(Prop_IsSingleDigit_Filtered, 11);

            // We can wrap tests up in generic classes.
            // For example, our IsPrefix test suite can be used both on an
            // invalid implementation...
            var faulty = new IsPrefixTests<int>(IsPrefixFaulty);
            PBTHelpers.Check(faulty.Prop_IsPrefix, 4);
            PBTHelpers.Check(faulty.Prop_IsPrefix_Sound, 4);

            // ...and a valid one.
            var valid = new IsPrefixTests<int>(IsPrefixValid);
            PBTHelpers.Check(valid.Prop_IsPrefix, 4);
            // We can also name properties.
            var named = new Named<Func<int[], int[], Imp<bool, Exists<int[], bool>>>>("valid IsPrefix is sound", valid.Prop_IsPrefix_Sound);
            PBTHelpers.Check(named, 4);
        }
    }
}
