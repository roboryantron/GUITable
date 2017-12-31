// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   12/29/2017
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using UnityEditor;
using UnityEngine;

namespace Assets.Code.Editor
{
    [CustomPropertyDrawer(typeof(Array2D), true)]
    public class Array2DDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TableState state = TableDrawer.GetState(property);
            return state.GetHeight();
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty width = property.FindPropertyRelative("Width");
            SerializedProperty list = property.FindPropertyRelative("List");
            
            TableState state = TableDrawer.GetState(property);
         
            TableDrawer.Draw(state, position, width.intValue, list);
        }
    }
    public class TableState
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

        public float GetHeight()
        {
            float result = TableDrawer.MIN_HEIGHT;// header
            if (RowHeights == null)
                return result;
            for (int i = 0; i < RowHeights.Length; i++)
                result += RowHeights[i];
            return result + TableDrawer.BOTTOM_PADDING;
        }
    }
    
    public static class TableDrawer
    {
        public static readonly RectOffset Padding = new RectOffset(2, 2, 2, 2);
        
        public const float MIN_WIDTH = 100;
        public const float MIN_HEIGHT = 20;
        public const float BOTTOM_PADDING = 10;

        public static TableState GetState(SerializedProperty property)
        {
            int control = (property.serializedObject.targetObject.name + "_" + property.propertyPath).GetHashCode();
            return (TableState)GUIUtility.GetStateObject(typeof(TableState), control);
        }

        public static int GetRowCount(int totalLength, int width)
        {
            return (totalLength / width) + 1;
        }

        // TODO move to more general script
        private static int GetChildCount(SerializedProperty prop)
        {
            int rootDepth = prop.depth;
            SerializedProperty copy = prop.Copy();
            
            bool more = copy.NextVisible(true);
            if (!more) return 0;
            if (copy.depth <= rootDepth)
                return 0;
            int count = 1;
            while (copy.NextVisible(false))
            {
                if (copy.depth == rootDepth + 1)
                    count++;
                else
                    break;
            }
            return count;
        }

        private static void SetupState(TableState state, Rect position, int width, SerializedProperty list)
        {
            int listSize = list.arraySize;
            int rowCount = GetRowCount(listSize, width);
            
            if (!state.Initialized || width != state.ColumnWidths.Length)
            {
                state.Initialized = true;
                state.ColumnWidths = new float[width];
                for (int i = 0; i < state.ColumnWidths.Length; i++)
                    state.ColumnWidths[i] = MIN_WIDTH; // TODO: defaults
                
                state.RowHeights = new float[rowCount];
                for (int i = 0; i < state.RowHeights.Length; i++)
                    state.RowHeights[i] = MIN_HEIGHT; 
                state.Resizing = -1;
                state.Rect = new Rect(0, 0, 500, state.GetWidth()); // TODO: get height
            }

            while (state.RowHeights.Length < rowCount)
                ArrayUtility.Add(ref state.RowHeights, MIN_HEIGHT);
            
            while (state.RowHeights.Length > rowCount)
                ArrayUtility.RemoveAt(ref state.RowHeights, state.RowHeights.Length-1);
            
            while (state.ColumnWidths.Length < width)
                ArrayUtility.Add(ref state.ColumnWidths, MIN_WIDTH);
            
            while (state.ColumnWidths.Length > width)
                ArrayUtility.RemoveAt(ref state.ColumnWidths, state.ColumnWidths.Length-1);

            state.Rect.min = position.min;
        }
        
        public static void Draw(TableState state, Rect position, int width, SerializedProperty list)
        {
            int listSize = list.arraySize;
            int rowCount = GetRowCount(listSize, width);
            float height = state.GetHeight();
            
            SetupState(state, position, width, list);

            if (Event.current.type == EventType.MouseUp &&
                state.Resizing != -1)
            {
                state.Resizing = -1;
                EditorUtility.SetDirty(list.serializedObject.targetObject);
            }

            // Setup position cursors
            Vector2 cursor = state.Rect.position;

            // Calculate column widths
            Rect header = Rect.zero;
            for (int c = 0; c < width; c++)
            {
                header = new Rect(cursor, new Vector2(state.ColumnWidths[c], MIN_HEIGHT));
                GUI.Box(header, c.ToString());
                cursor.x += header.width;

                // Make the active resize area wide to prevent flickering as the cursor jumps in and out of the area.
                float dragAreaWidth = state.Resizing == c ? 50 : 6;
                Rect dragArea = new Rect(cursor.x - dragAreaWidth/2, cursor.y, dragAreaWidth, position.height - BOTTOM_PADDING);
                //GUI.Box(dragArea, "");
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.ResizeHorizontal);
                if (Event.current.type == EventType.MouseDown && dragArea.Contains(Event.current.mousePosition))
                {
                    state.Resizing = c;
                }

                if (state.Resizing == c && Event.current.type == EventType.MouseDrag)
                {
                    float target = Event.current.mousePosition.x;
                    state.ColumnWidths[c] = Mathf.Max(MIN_WIDTH, state.ColumnWidths[c] - (cursor.x - target));
                    Event.current.Use();
                }
            }
            cursor.y += header.height;

            

            // Calculate row heights
            for (int r = 0; r < rowCount; r++)
            {
                float[] cellHeights = new float[width];
                for (int c = 0; c < width; c++)
                {
                    int index = Array2D.GetIndex(c, r, width);

                    if (index < listSize)
                    {
                        SerializedProperty element = list.GetArrayElementAtIndex(index);
                        // If the property is a container for a single object, jump into that object
                        int childCount = GetChildCount(element);
                        if (childCount == 1)
                            element.NextVisible(true);
                        cellHeights[c] = EditorGUI.GetPropertyHeight(element) + Padding.top + Padding.bottom;
                    }
                }
                state.RowHeights[r] = Mathf.Max(MIN_HEIGHT, Mathf.Max(cellHeights));
            }

            for (int r = 0; r < rowCount; r++)
            {
                cursor.x = state.Rect.x;
                for (int c = 0; c < width; c++)
                {
                    int index = Array2D.GetIndex(c, r, width);
                    
                    Rect cellRect = new Rect(cursor, new Vector2(state.ColumnWidths[c], state.RowHeights[r]));

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1 &&
                        cellRect.Contains(Event.current.mousePosition))
                    {
                        int indexToModify = index;
                        GenericMenu m = new GenericMenu();
                        m.AddItem(new GUIContent("Delete"), false, ()=>
                        {
                            list.DeleteArrayElementAtIndex(indexToModify);
                            list.serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(list.serializedObject.targetObject);
                        });
                        
                        m.AddItem(new GUIContent("Duplicate"), false, ()=>
                        {
                            list.InsertArrayElementAtIndex(indexToModify);
                            list.serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(list.serializedObject.targetObject);
                        });
                        
                        m.ShowAsContext();
                        
                    }
                    
                    Rect contentRect = Padding.Remove(cellRect);

                    

                    if (index < listSize)
                    {
                        EditorGUIUtility.labelWidth = Mathf.Min(85, Mathf.Max(40, cellRect.width * 0.5f));
                        SerializedProperty element = list.GetArrayElementAtIndex(index);
                        // If the property is a container for a single object, jump into that object
                        int childCount = GetChildCount(element);
                        if (childCount == 1)
                            element.NextVisible(true);
                        
                        //GUI.Box(cellRect, "" + childCount);
                        GUI.Box(cellRect, "");
                        if (r%2==0)
                            GUI.Box(cellRect, "");
 
                        GUIContent label = GUIContent.none;
                        
                        if (element.hasVisibleChildren)
                        {
                            contentRect.xMin += 10;
                            label = new GUIContent(element.displayName);
                        }
                        EditorGUI.PropertyField(contentRect,
                            element,
                            label, true);
                        EditorGUIUtility.labelWidth = 0.0f;
                    }
                    else
                    {
                        GUI.Box(cellRect, "");
                        if (r%2==0)
                            GUI.Box(cellRect, "");

                        if (listSize == index)
                        {
                            contentRect.height = MIN_HEIGHT - 6;
                            contentRect.width = MIN_HEIGHT - 3;
                            contentRect.center = cellRect.center;
                            if (GUI.Button(contentRect, "+", EditorStyles.miniButton))
                            {
                                list.InsertArrayElementAtIndex(listSize);
                                list.serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(list.serializedObject.targetObject);
                            }
                        }
                    }
                    cursor.x += state.ColumnWidths[c];
                }
                cursor.y += state.RowHeights[r];
            }

            if (Math.Abs(state.GetHeight() - height) > 0.1f)
            {
                EditorUtility.SetDirty(list.serializedObject.targetObject);
            }
        }
    }
}