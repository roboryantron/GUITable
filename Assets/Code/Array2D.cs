// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   12/29/2017
// ----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Code
{
    public class Array2D : ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Address
        {
            public int X;
            public int Y;

            public Address(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        
        // TODO: could this just be multidimensional with an array of dimension sizes?
        public int Width;

        public int GetIndex(int x, int y)
        {
            return GetIndex(x, y, Width);
        }


        public Address GetAddress(int i)
        { 
            return new Address(i % Width, i / Width);
        }

        public void OnBeforeSerialize()
        {
            Width = Mathf.Max(Width, 1);
        }

        public void OnAfterDeserialize()
        {}

        public static int GetIndex(int x, int y, int width)
        {
            return y * width + x;
        }
    }
    
    public class Array2D<T> : Array2D
    {
        //public List<T> List;
        public List<T> List;

        public T this[int i]
        {
            get { return List[i]; }
            set { List[i] = value; }
        }
        
        public T this[int x, int y]
        {
            get { return List[GetIndex(x, y)]; }
            set { List[GetIndex(x, y)] = value; }
        }
    }

    [Serializable]
    public class ObjectArray2D : Array2D<Object>
    {}

    [Serializable]
    public class StringSet
    {
        public string[] Strings;
    }
    
    [Serializable]
    public class StringListArray2D : Array2D<StringSet>
    {
        // nope
    }
}