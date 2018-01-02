// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   12/31/2017
// ----------------------------------------------------------------------------

using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

namespace Assets.Code.Editor
{
    public class TableDrawerState
    {
        public int Width;
        public SerializedObject Object;
        public bool Initialized;
        public float[] ColumnWidths;
        public float[] RowHeights;
        public Rect Rect;
        public int Resizing;

        public float GetWidth()
        {
            float result = 0;
            for (int i = 0; i < ColumnWidths.Length; i++)
                result += ColumnWidths[i];
            return result;
        }

        public float GetHeight(SerializedProperty list)
        {
            if (Object == null)
                Object = list.serializedObject;
            
            float result = TableDrawer.MIN_HEIGHT;// header
            if (RowHeights == null)
            {
                int listSize = list.arraySize;
                int rowCount = TableDrawer.GetRowCount(listSize, Width);
                RowHeights = new float[rowCount];
                CalculateRowHeights(Width, rowCount, list);
                return result;
            }

            for (int i = 0; i < RowHeights.Length; i++)
                result += RowHeights[i];
            
            if (!Initialized)
            {
                list.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(list.serializedObject.targetObject);
            }
            
            return result + TableDrawer.BOTTOM_PADDING;
        }
        
        public void CalculateRowHeights(int width, int rowCount, SerializedProperty list)
        {
            if (Object == null)
                Object = list.serializedObject;
            
            for (int r = 0; r < rowCount; r++)
            {
                float[] cellHeights = new float[width];
                for (int c = 0; c < width; c++)
                {
                    int index = Array2D.GetIndex(c, r, width);

                    if (index < list.arraySize)
                    {
                        SerializedProperty element = list.GetArrayElementAtIndex(index);
                        // If the property is a container for a single object, jump into that object
                        int childCount = TableDrawer.GetChildCount(element);
                        if (childCount == 1)
                            element.NextVisible(true);
                        cellHeights[c] = EditorGUI.GetPropertyHeight(element) + TableDrawer.Padding.top + TableDrawer.Padding.bottom;
                    }
                }
                RowHeights[r] = Mathf.Max(TableDrawer.MIN_HEIGHT, Mathf.Max(cellHeights));
            }
        }
    }
}