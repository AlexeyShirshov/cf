﻿
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq;

namespace CoreFramework.CFCollections
{
    public class EqualityFunc<T> : IEqualityComparer<T>
    {

        private Func<T, T, bool> _func;
        private Func<T, int> _hash;
        public EqualityFunc(Func<T, T, bool> equals, Func<T, int> hash)
        {
            if (equals == null)
                throw new ArgumentNullException("func");

            if (hash == null)
                throw new ArgumentNullException("hash");

            _func = equals;
            _hash = hash;
        }
        public bool Equals1(T x, T y)
        {
            return _func(x, y);
        }
        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return Equals1(x, y);
        }

        public int GetHashCodeComparer(T obj)
        {
            return _hash(obj);
        }
        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return GetHashCodeComparer(obj);
        }
    }
       
}