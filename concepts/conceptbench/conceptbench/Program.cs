﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Numerics;

using System.Runtime.CompilerServices;
namespace conceptbench {


    interface Num<T> {
        T FromInteger(int v);
        T Plus(T a, T b);

        T Mult(T a, T b);
    }

    struct NumInt : Num<int> {
        public int FromInteger(int v) => v;
        public int Mult(int a, int b) => a * b;

        public int Plus(int a, int b) => a + b;
    }


    struct SlowNumInt : Num<int> {
        [MethodImpl(MethodImplOptions.NoInlining)]

        public int FromInteger(int v) => v;

        [MethodImpl(MethodImplOptions.NoInlining)]

        public int Mult(int a, int b) => a * b;

        [MethodImpl(MethodImplOptions.NoInlining)]

        public int Plus(int a, int b) => a + b;
    }



    struct NumDouble : Num<double> {
        public Double FromInteger(int v) => v;
        public Double Mult(Double a, Double b) => a * b;

        public Double Plus(Double a, Double b) => a + b;
    }


    struct NumFloat : Num<float> {
        public float FromInteger(int v) => v;
        public float Mult(float a, float b) => a * b;

        public float Plus(float a, float b) => a + b;
    }


    struct NumLong : Num<long> {
        public long FromInteger(int v) => v;
        public long Mult(long a, long b) => a * b;

        public long Plus(long a, long b) => a + b;
    }

    struct NumVector3 : Num<Vector3> {
        public Vector3 FromInteger(int v) => new Vector3(v, v, v);
        public Vector3 Mult(Vector3 a, Vector3 b) => a * b;

        public Vector3 Plus(Vector3 a, Vector3 b) => a + b;
    }


    struct NumTuple<T1,T2,T3,W1,W2,W3> : Num<Tuple<T1,T2,T3>> where W1:struct, Num<T1>  where W2: struct, Num<T2> where W3 : struct, Num<T3> {
        public Tuple<T1, T2, T3> FromInteger(int v) => new Tuple<T1,T2,T3>(default(W1).FromInteger(v),default(W2).FromInteger(v),default(W3).FromInteger(v));
        public Tuple<T1, T2, T3> Mult(Tuple<T1, T2, T3> a, Tuple<T1, T2, T3> b) =>
            new Tuple<T1, T2, T3>(default(W1).Mult(a.Item1, b.Item1),
                                  default(W2).Mult(a.Item2, b.Item2),
                                  default(W3).Mult(a.Item3, b.Item3));

        public Tuple<T1, T2, T3> Plus(Tuple<T1, T2, T3> a, Tuple<T1, T2, T3> b) =>
             new Tuple<T1, T2, T3>(default(W1).Plus(a.Item1, b.Item1),
                                   default(W2).Plus(a.Item2, b.Item2),
                                   default(W3).Plus(a.Item3, b.Item3));
    }


    struct NumTuple<T, W> : Num<Tuple<T,T,T>> where W : struct, Num<T> {

        
        public Tuple<T,T,T> FromInteger(int v) {
            var W = default(W);

            return
            new Tuple<T, T, T>(W.FromInteger(v), W.FromInteger(v), W.FromInteger(v));
        }
        public Tuple<T, T, T> Mult(Tuple<T, T, T> a, Tuple<T, T, T> b) {
            var W = default(W);
            return
            new Tuple<T, T, T>(W.Mult(a.Item1, b.Item1),
                                  W.Mult(a.Item2, b.Item2),
                                  W.Mult(a.Item3, b.Item3));
        }
        public Tuple<T, T, T> Plus(Tuple<T, T, T> a, Tuple<T, T, T> b) {

            var W = default(W);
            return
            new Tuple<T, T, T>(W.Plus(a.Item1, b.Item1),
                                   W.Plus(a.Item2, b.Item2),
                                   W.Plus(a.Item3, b.Item3));
        }
    }

    struct NumTupleOpt<T, V> : Num<Tuple<T, T, T>> where V : struct, Num<T> {

        V W;
        public Tuple<T, T, T> FromInteger(int v) {
           

            return
            new Tuple<T, T, T>(W.FromInteger(v), W.FromInteger(v), W.FromInteger(v));
        }
        public Tuple<T, T, T> Mult(Tuple<T, T, T> a, Tuple<T, T, T> b) {
            
            return
            new Tuple<T, T, T>(W.Mult(a.Item1, b.Item1),
                                  W.Mult(a.Item2, b.Item2),
                                  W.Mult(a.Item3, b.Item3));
        }
        public Tuple<T, T, T> Plus(Tuple<T, T, T> a, Tuple<T, T, T> b) {

            
            return
            new Tuple<T, T, T>(W.Plus(a.Item1, b.Item1),
                                   W.Plus(a.Item2, b.Item2),
                                   W.Plus(a.Item3, b.Item3));
        }
    }

    public struct Vec3 {
        readonly float v1;
        readonly float v2;
        readonly float v3;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3(float v1, float v2, float v3) {
            this.v1 = v1; this.v2 = v2; this.v3 = v3;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public Vec3(int v) {
            float f = v;
            this.v1 = f; this.v2 = f; this.v3 = f;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.v1 + b.v1, a.v2 + b.v2, a.v3 + b.v3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(Vec3 a, Vec3 b) => new Vec3(a.v1 * b.v1, a.v2 * b.v2, a.v3 * b.v3);

    }



    struct NumVec3 : Num<Vec3> {
        public Vec3 FromInteger(int v) => new Vec3(v, v, v);
        public Vec3 Mult(Vec3 a, Vec3 b) => a * b;

        public Vec3 Plus(Vec3 a, Vec3 b) => a + b;
    }



    public class ClassVec3 {
        readonly float v1;
        readonly float v2;
        readonly float v3;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ClassVec3(float v1, float v2, float v3) {
            this.v1 = v1; this.v2 = v2; this.v3 = v3;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ClassVec3(int v) {
            float f = v;
            this.v1 = f; this.v2 = f; this.v3 = f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClassVec3 operator +(ClassVec3 a, ClassVec3 b) => new ClassVec3(a.v1 + b.v1, a.v2 + b.v2, a.v3 + b.v3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClassVec3 operator *(ClassVec3 a, ClassVec3 b) => new ClassVec3(a.v1 * b.v1, a.v2 * b.v2, a.v3 * b.v3);
    }

    public struct NumClassVec3 : Num<ClassVec3> {
        public ClassVec3 FromInteger(int v) => new ClassVec3(v, v, v);
        public ClassVec3 Mult(ClassVec3 a, ClassVec3 b) => a * b;

        public ClassVec3 Plus(ClassVec3 a, ClassVec3 b) => a + b;
    }


    abstract class AbstractNum<T> {
        public abstract T FromInteger(int v);
        public abstract T Mult(T a, T b);
        public abstract T Plus(T a, T b);

    }

    class NumIntSubClass : AbstractNum<int> {
        public override int FromInteger(int v) => v;
        public override int Mult(int a, int b) => a * b;
        public override int Plus(int a, int b) => a + b;
    }

    class NumIntImplemention : Num<int> {
        public int FromInteger(int v) => v;
        public int Mult(int a, int b) => a * b;
        public int Plus(int a, int b) => a + b;
    }


    public class Benchmarks {

        static int n = 1000000;


        //[Benchmark]
        public int Int() {
            int y = 0;
            int c = 666;
            for (int i = 0; i < n; i++) {
                int x = i;
                y = ((x * x) + x) + c;
            }
            return y;
        }

        //[Benchmark]
        public double Double() {
            double y = 0;
            double c = 666;
            for (int i = 0; i < n; i++) {
                double x = i;
                y = ((x * x) + x) + c;
            }
            return y;
        }


        //[Benchmark]
        public float Float() {
            float y = 0;
            float c = 666;
            for (int i = 0; i < n; i++) {
                var x = i;
                y = ((x * x) + x) + c ;
            }
            return y;
        }
        //[Benchmark]
        public long Long() {
            long y = 0;
            long c = 666;
            for (int i = 0; i < n; i++) {
                long x = i;
                y = ((x * x) + x) + c;
            }
            return y;
        }

        //[Benchmark]
        public Vector3 Vector3() {
            var y = new Vector3(0, 0, 0);
            var c = new Vector3(666, 666, 666);
            for (int i = 0; i < n; i++) {
                var x = new Vector3(i, i, i);
                y = ((x * x) + x) + c;
            }
            return y;
        }

        [Benchmark]
        public Vec3 Vec3() {
            var y = new Vec3(0, 0, 0);
            var c = new Vec3(666); 
            for (int i = 0; i < n; i++) {
                var x = new Vec3(i, i, i);
                y = ((x * x) + x) + c;
            }
            return y;
        }


        [Benchmark]
        public ClassVec3 ClassVec3() {
            var y = new ClassVec3(0, 0, 0);
            var c = new ClassVec3(666);
            for (int i = 0; i < n; i++) {
                var x = new ClassVec3(i, i, i);
                y = ((x * x) + x) + c ;
            }
            return y;
        }



        static Tuple<float, float, float> Add(Tuple<float, float, float> a, Tuple<float, float, float> b) =>
            new Tuple<float, float, float>(a.Item1 + b.Item1,
                                           a.Item2 + b.Item2,
                                           a.Item3 + b.Item3);

        static Tuple<float, float, float> Mult(Tuple<float, float, float> a, Tuple<float, float, float> b) =>
            new Tuple<float, float, float>(a.Item1 * b.Item1,
                                           a.Item2 * b.Item2,
                                           a.Item3 * b.Item3);
        [Benchmark]
        public Tuple<float,float,float> Tuple() {
            var y = new Tuple<float,float,float>(0, 0, 0);
            var c = new Tuple<float,float,float>(666,666,666);
            for (int i = 0; i < n; i++) {
                var x = new Tuple<float,float,float>(i, i, i);
                y = Add(Add(Mult(x,x),x),c);
            }
            return y;
        }



        //[Benchmark]
        public int ConceptDirect() {
            var y = default(NumInt).FromInteger(0);
            var c = default(NumInt).FromInteger(666);
            for (int i = 0; i < n; i++) {
                var x = default(NumInt).FromInteger(i);
                y = default(NumInt).Plus(default(NumInt).Plus(default(NumInt).Mult(x, x), x),c);
            }
            return y;
        }
        //[Benchmark]
        public int ConceptDirectOpt() {
            var NumInt = default(NumInt);
            var y = NumInt.FromInteger(0);
            var c = NumInt.FromInteger(666);
            for (int i = 0; i < n; i++) {
                var x = NumInt.FromInteger(i);
                y = NumInt.Plus(NumInt.Plus(NumInt.Mult(x, x), x),c);
            }
            return y;
        }

        //[Benchmark]
        public int ConceptInstanceInt() {
            return ConceptGeneric<int, NumInt>();
        }


        //[Benchmark]
        public int ConceptOptInstanceInt() {
            return ConceptGenericOpt<int, NumInt>();
        }

        //[Benchmark]
        public int ConceptInstanceIntSlow() {
            return ConceptGeneric<int, SlowNumInt>();
        }


        //[Benchmark]
        public int ConceptOptInstanceIntSlow() {
            return ConceptGenericOpt<int,SlowNumInt>();
        }


        //[Benchmark]
        public double ConceptInstanceDouble() {
            return ConceptGeneric<double, NumDouble>();
        }


        //[Benchmark]
        public double ConceptOptInstanceDouble() {
            return ConceptGenericOpt<double, NumDouble>();
        }


        //[Benchmark]
        public float ConceptInstanceFloat() {
            return ConceptGeneric<float, NumFloat>();
        }


        //[Benchmark]
        public float ConceptOptInstanceFloat() {
            return ConceptGenericOpt<float, NumFloat>();
        }



        //[Benchmark]
        public long ConceptInstanceLong() {
            return ConceptGeneric<long, NumLong>();
        }


        //[Benchmark]
        public long ConceptOptInstanceLong() {
            return ConceptGenericOpt<long, NumLong>();
        }



        //[Benchmark]
        public Vector3 ConceptInstanceVector3() {
            return ConceptGeneric<Vector3, NumVector3>();
        }

        //[Benchmark]
        public Vector3 ConceptOptInstanceVector3() {
            return ConceptGenericOpt<Vector3, NumVector3>();
        }

        [Benchmark]
        public Vec3 ConceptInstanceVec3() {
            return ConceptGeneric<Vec3, NumVec3>();
        }

        [Benchmark]
        public Vec3 ConceptOptInstanceVect3() {
            return ConceptGenericOpt<Vec3, NumVec3>();
        }


        [Benchmark]
        public ClassVec3 ConceptInstanceClassVec3() {
            return ConceptGeneric<ClassVec3, NumClassVec3>();
        }

        [Benchmark]
        public ClassVec3 ConceptOptInstanceClassVec3() {
            return ConceptGenericOpt<ClassVec3, NumClassVec3>();
        }



        [Benchmark]
        public Tuple<float,float,float> ConceptInstanceTuple() {
            return ConceptGeneric<Tuple<float,float,float>, NumTuple<float,float,float,NumFloat,NumFloat,NumFloat>>();
        }

        [Benchmark]
        public Tuple<float, float, float> ConceptOptInstanceTuple() {
            return ConceptGenericOpt<Tuple<float, float, float>, NumTuple<float, float, float, NumFloat, NumFloat, NumFloat>>();
        }

        [Benchmark]
        public Tuple<float, float, float> ConceptOptInstanceTupleOpt() {
            return ConceptGenericOpt<Tuple<float, float, float>, NumTuple<float, NumFloat>>();
        }

        [Benchmark]
        public Tuple<float, float, float> ConceptOptInstanceTupleOptOpt() {
            return ConceptGenericOpt<Tuple<float, float, float>, NumTupleOpt<float, NumFloat>>();
        }

        //[Benchmark]
        public int AbstractClass() {
            return AbstractClass<int>(new NumIntSubClass());
        }
        T AbstractClass<T>(AbstractNum<T> NI) {
            var y = NI.FromInteger(0);
            var c = NI.FromInteger(666);
            for (int i = 0; i < n; i++) {
                var x = NI.FromInteger(i);
                y = NI.Plus(NI.Plus(NI.Mult(x, x), x),c);
            }
            return y;
        }


        //[Benchmark]
        public int Interface() {
            return Interface<int>(new NumIntImplemention());
        }
        T Interface<T>(Num<T> NI) {
            var y = NI.FromInteger(0);
            var c = NI.FromInteger(666);
            for (int i = 0; i < n; i++) {
                var x = NI.FromInteger(i);
                y = NI.Plus(NI.Plus(NI.Mult(x, x), x),c);
            }
            return y;
        }



        T ConceptGeneric<T, NumT>() where NumT : struct, Num<T> {
            var y = default(NumT).FromInteger(0);
            for (int i = 0; i < n; i++) {
                var x = default(NumT).FromInteger(i);
                y = default(NumT).Plus(default(NumT).Plus(default(NumT).Mult(x, x), x),
                                        default(NumT).FromInteger(666));
            }
            return y;
        }


        T ConceptGenericOpt<T, NumT>() where NumT : struct, Num<T> {
            NumT NI = default(NumT);
            T y = NI.FromInteger(0);
            T c = NI.FromInteger(666);
            for (int i = 0; i < n; i++) {
                T x = NI.FromInteger(i);
                y = NI.Plus(NI.Plus(NI.Mult(x,x),x), c);
            }
            return y;
        }


        //[Benchmark]
        public int Delegates() {
            return Delegates<int>(i => i, (a, b) => a + b, (a, b) => a * b);
        }
        T Delegates<T>(Func<int, T> FromInteger, Func<T, T, T> Plus, Func<T, T, T> Mult) {
            T y = FromInteger(0);
            T c = FromInteger(666);
            for (int i = 0; i < n; i++) {
                T x = FromInteger(i);
                y = Plus(Plus(Mult(x, x), x),c);
            }
            return y;
        }



    }


    class Program {

        static void Main() {
            System.Console.WriteLine(System.Numerics.Vector.IsHardwareAccelerated);
            var summary = BenchmarkRunner.Run<Benchmarks>();

        }


    }






}
