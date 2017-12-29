// ----------------------------------------------------------------------------
//  Copyright © 2017 Schell Games, LLC. All Rights Reserved. 
// 
//  Author: Ryan Hipple
//  Date:   07/26/2017
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.Editor
{
    // serialized objects on rows, field names on columns?
    // object passed in for rows, functions to call on those objects for columns

    public class SerializedObjectGUITable : GUITable<SerializedObject>
    {
        public void AutoAdd(SerializedObject o)
        {
            AddNameColumn("Name");
            bool display = false;
            SerializedProperty prop = o.GetIterator();
            prop.Next(true);
            do
            {
                if (display)
                    AddColumn(prop.name, prop.name);
                if (prop.name == "m_EditorClassIdentifier")
                    display = true;
            } while (prop.Next(false));

        }

        
        public void AddColumn(string fieldName, string header)
        {
            Columns.Add(new ColumnData {
                Draw = (o, rect) =>
                {
                    RectOffset offset = new RectOffset(4, 4, 4, 4);
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(offset.Remove(rect), o.FindProperty(fieldName), GUIContent.none, true);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        o.ApplyModifiedProperties();
                        EditorUtility.SetDirty(o.targetObject);
                    }
                    return EditorGUI.GetPropertyHeight(o.FindProperty(fieldName), true) + 8;
                },
                Width = CellWidth,
                Header = header });
        }

        public void AddNameColumn(string header)
        {
            Columns.Add(new ColumnData
            {
                Draw = (o, rect) =>
                {
                    EditorGUI.LabelField(rect, o.targetObject.name);
                    return 16.0f;
                },
                Width = CellWidth,
                Header = header
            });
        }
    }

    public class GUITable<T>
    {
        public delegate float DrawColumn(T t, Rect r);

        public class ColumnData
        {
            public DrawColumn Draw;
            //public Action<T, Rect> Draw;
            public float Width;
            public string Header;
        }

        public List<T> Rows = new List<T>();
        public List<float> RowHeights = new List<float>();

        public List<ColumnData> Columns = new List<ColumnData>();

        public float MinWidth = 40.0f;
        public float MinHeight = 20.0f;

        public float CellWidth = 200.0f;
        public float CellHeight = 100.0f;

        public float HeaderHeight = 20.0f;

        public GUITable()
        {}
        

        public void AddRow(T row)
        {
            Rows.Add(row);
            RowHeights.Add(CellHeight);
        }

        public void AddColumn(DrawColumn column, string header)
        {
            Columns.Add(new ColumnData {Draw = column, Width = CellWidth, Header = header});
        }

        private int resizing = -1;

        public Action AddCallback; 

        public Rect Draw(Rect rect)
        {
            if (Event.current.type == EventType.MouseUp)
                resizing = -1;

            float x = 0.0f;
            float y = HeaderHeight;
            for (int c = 0; c < Columns.Count; c++)
            {
                GUI.Box(new Rect(x, 0, Columns[c].Width, y), Columns[c].Header);
                x += Columns[c].Width;

                Rect dragArea = new Rect(x - 2, 0, 4, rect.height);
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.ResizeHorizontal);
                if (Event.current.type == EventType.MouseDown && dragArea.Contains(Event.current.mousePosition))
                {
                    resizing = c;
                }

                if (resizing == c && Event.current.type == EventType.MouseDrag)
                {
                    float target = Event.current.mousePosition.x;
                    Columns[c].Width = Mathf.Max(MinWidth, Columns[c].Width - (x - target));
                    Event.current.Use();
                }
            }

            
            for (int r = 0; r < Rows.Count; r++)
            {
                x = 0.0f;
                float[] heights = new float[Columns.Count];
                for (int c = 0; c < Columns.Count; c++)
                {
                    Rect cellRect = new Rect(x, y, Columns[c].Width, RowHeights[r]);
                    GUI.Box(cellRect, "");
                    if (r%2==0)
                        GUI.Box(cellRect, "");
                    GUI.BeginGroup(cellRect);
                    cellRect.position = Vector2.zero;
                    GUILayout.BeginArea(cellRect);

                    heights[c] = Columns[c].Draw(Rows[r], cellRect);
                    
                    //GUI.Box(cellRect, Columns[c](Rows[r]));
                    
                    GUILayout.EndArea();
                    GUI.EndGroup();
                    x += Columns[c].Width;
                }
                y += RowHeights[r];
                RowHeights[r] = Mathf.Max(MinHeight, Mathf.Max(heights));
            }

            
            Rect addRect = new Rect(0, y, CellWidth, MinHeight);
            if (GUI.Button(addRect, "+"))
            {
                AddCallback();
            }
            y += MinHeight;

            return new Rect(0, 0, x, y);
        }
    }
}