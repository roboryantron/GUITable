// ----------------------------------------------------------------------------
//  Copyright © 2017 Schell Games, LLC. All Rights Reserved. 
// 
//  Author: Ryan Hipple
//  Date:   07/27/2017
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Assets.Code
{
    [CreateAssetMenu]
    public class TestData : ScriptableObject
    {
        public string Name = "";
        public int X;
        public string[] Strings = new string[0];
    }
}