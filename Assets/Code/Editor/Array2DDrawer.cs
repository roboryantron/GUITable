// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   12/29/2017
// ----------------------------------------------------------------------------

using Microsoft.Win32;
using UnityEditor;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace Assets.Code.Editor
{
    [CustomPropertyDrawer(typeof(Array2D), true)]
    public class Array2DDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //return base.GetPropertyHeight(property, label);

            return 500;
        }

        private static TableDrawer.TableState state;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect row = new Rect(position);

            SerializedProperty width = property.FindPropertyRelative("Width");
            SerializedProperty list = property.FindPropertyRelative("List");
            state = TableDrawer.Draw(state, position, width.intValue, list);
        }
    }

    public static class TableDrawer
    {
        public struct TableState
        {
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
        }

        public const float MIN_WIDTH = 100;
        public const float MIN_HEIGHT = 16;

        public static int GetRowCount(int totalLength, int width)
        {
            return (totalLength / width) + 1;
        }
        
        public static TableState Draw(TableState state, Rect position, int width, SerializedProperty list)
        {
            if (!state.Initialized || width != state.ColumnWidths.Length)
            {
                state.Initialized = true;
                state.ColumnWidths = new float[width];
                for (int i = 0; i < state.ColumnWidths.Length; i++)
                    state.ColumnWidths[i] = MIN_WIDTH; // TODO: defaults
                
                state.RowHeights = new float[GetRowCount(list.arraySize, width)];
                for (int i = 0; i < state.RowHeights.Length; i++)
                    state.RowHeights[i] = MIN_HEIGHT; 
                state.Resizing = -1;
                state.Rect = new Rect(0, 0, 500, state.GetWidth()); // TODO: get height
            }

            state.Rect.min = position.min;
            
            if (Event.current.type == EventType.MouseUp)
                state.Resizing = -1;

            float x = state.Rect.x;
            float y = state.Rect.y;// = 16.0f;//HeaderHeight;

            // Calculate column widths
            Rect header = Rect.zero;
            for (int c = 0; c < width; c++)
            {
                header = new Rect(x, y, state.ColumnWidths[c], MIN_HEIGHT);
                GUI.Box(header, c.ToString());
                x += header.width;

                Rect dragArea = new Rect(x - 2, y, 4, state.Rect.height);
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.ResizeHorizontal);
                if (Event.current.type == EventType.MouseDown && dragArea.Contains(Event.current.mousePosition))
                {
                    state.Resizing = c;
                }

                if (state.Resizing == c && Event.current.type == EventType.MouseDrag)
                {
                    float target = Event.current.mousePosition.x;
                    state.ColumnWidths[c] = Mathf.Max(MIN_WIDTH, state.ColumnWidths[c] - (x - target));
                    Event.current.Use();
                }
            }
            y += header.height;

            int rowCount = GetRowCount(list.arraySize, width);

            // Calculate row heights
            for (int r = 0; r < rowCount; r++)
            {
                float[] cellHeights = new float[width];
                for (int c = 0; c < width; c++)
                {
                    int index = Array2D.GetIndex(c, r, width);

                    if (index < list.arraySize)
                    {
                        SerializedProperty element = list.GetArrayElementAtIndex(index);
                        cellHeights[c] = EditorGUI.GetPropertyHeight(element);
                    }
                }
                state.RowHeights[r] = Mathf.Max(MIN_HEIGHT, Mathf.Max(cellHeights));
            }

            for (int r = 0; r < rowCount; r++)
            {
                x = state.Rect.x;
                for (int c = 0; c < width; c++)
                {
                    Rect cellRect = new Rect(x, y, state.ColumnWidths[c], state.RowHeights[r]);

                    int index = Array2D.GetIndex(c, r, width);

                    if (index < list.arraySize)
                    {
                        EditorGUIUtility.labelWidth = Mathf.Max(60, cellRect.width * 0.6f);
                        SerializedProperty element = list.GetArrayElementAtIndex(index);
                            
                        GUI.Box(cellRect, "");
                        if (r%2==0)
                            GUI.Box(cellRect, "");
                        
                        // If it is a foldout, indent it
                        if (element.hasChildren)
                            cellRect.xMin += 20;
                        
                        EditorGUI.PropertyField(cellRect,
                            element,
                            GUIContent.none, true);
                        EditorGUIUtility.labelWidth = 0.0f;
                    }
                    else
                    {
                        GUI.Box(cellRect, "");
                        if (r%2==0)
                            GUI.Box(cellRect, "");
                    }
                    x += state.ColumnWidths[c];
                }
                y += state.RowHeights[r];
            }

            /*
            Rect addRect = new Rect(0, y, CellWidth, MIN_HEIGHT);
            if (GUI.Button(addRect, "+"))
            {
                //AddCallback();
            }
            */
            y += MIN_HEIGHT;

            return state;
        }
    }
}