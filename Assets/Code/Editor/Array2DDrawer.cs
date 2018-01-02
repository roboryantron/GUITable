// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   12/29/2017
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace Assets.Code.Editor
{
    [CustomPropertyDrawer(typeof(Array2D), true)]
    public class Array2DDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty width = property.FindPropertyRelative("Width");
            SerializedProperty list = property.FindPropertyRelative("List");
            
            TableDrawerState state = TableDrawer.GetState(property);
            state.Width = width.intValue;
            state.Object = property.serializedObject;
            return state.GetHeight(list);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty width = property.FindPropertyRelative("Width");
            SerializedProperty list = property.FindPropertyRelative("List");
            
            TableDrawerState state = TableDrawer.GetState(property);
         
            TableDrawer.Draw(state, position, width.intValue, list);
        }
    }
}