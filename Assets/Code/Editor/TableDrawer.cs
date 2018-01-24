// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   12/31/2017
// ----------------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Code.Editor
{
    public static class TableDrawer
    {
        public static readonly RectOffset Padding = new RectOffset(2, 2, 2, 2);
        
        public const float MIN_WIDTH = 100;
        public const float MIN_HEIGHT = 20;
        public const float BOTTOM_PADDING = 10;

        public static TableDrawerState GetState(SerializedProperty property)
        {
            int control = (property.serializedObject.targetObject.name + "_" + property.propertyPath).GetHashCode();
            return (TableDrawerState)GUIUtility.GetStateObject(typeof(TableDrawerState), control);
        }

        public static int GetRowCount(int totalLength, int width)
        {
            return (totalLength / width) + 1;
        }

        // TODO move to more general script
        public static int GetChildCount(SerializedProperty prop)
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

        private static void SetupState(TableDrawerState state, Rect position, int width, SerializedProperty list)
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
                
                list.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(list.serializedObject.targetObject);
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

        private static void HandleResize(TableDrawerState state, Rect position, int width, SerializedProperty list)
        {
            if (Event.current.type == EventType.MouseUp &&
                state.Resizing != -1)
            {
                state.Resizing = -1;
                EditorUtility.SetDirty(list.serializedObject.targetObject);
            }
            
            float x = position.x;
            for (int c = 0; c < width; c++)
            {
                x += state.ColumnWidths[c];
                
                // Make the active resize area wide to prevent flickering as the cursor jumps in and out of the area.
                float dragAreaWidth = state.Resizing == c ? 50 : 6;
                Rect dragArea = new Rect(x - dragAreaWidth/2, position.y, dragAreaWidth, position.height - BOTTOM_PADDING);
                //GUI.Box(dragArea, "");
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.ResizeHorizontal);
                if (Event.current.type == EventType.MouseDown && dragArea.Contains(Event.current.mousePosition))
                {
                    state.Resizing = c;
                    EditorUtility.SetDirty(list.serializedObject.targetObject);
                }

                if (state.Resizing == c)
                {
                    Color color = GUI.color;
                    GUI.color = new Color(0.3f, 0.57f, 0.71f);
                    GUI.Box(new Rect(x - 2, position.y, 4, position.height - BOTTOM_PADDING), "");
                    GUI.color = color;
                }

                if (state.Resizing == c && Event.current.type == EventType.MouseDrag)
                {
                    float target = Event.current.mousePosition.x;
                    state.ColumnWidths[c] = Mathf.Max(MIN_WIDTH, state.ColumnWidths[c] - (x - target));
                    Event.current.Use();
                }
            }
        }

        private static Rect DrawHeader(TableDrawerState state, Vector2 cursor, int width)
        {
            Rect header = new Rect(cursor, new Vector2(0, MIN_HEIGHT));
            for (int c = 0; c < width; c++)
            {
                header.width = state.ColumnWidths[c];
                GUI.Box(header, c.ToString());
                header.x += header.width;
            }
            header.width = header.xMax - cursor.x;
            return header;
        }

        private static void DrawContextMenu(SerializedProperty list, int index)
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

        public static bool IsContextClick(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && 
                   Event.current.button == 1 &&
                   rect.Contains(Event.current.mousePosition);
        }

        public static void DrawCell(Rect contentRect, SerializedProperty element)
        {
            EditorGUIUtility.labelWidth = Mathf.Min(85, Mathf.Max(40, contentRect.width * 0.5f));
            // If the property is a container for a single object, jump into that object
            int childCount = GetChildCount(element);
            if (childCount == 1)
                element.NextVisible(true);
           
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

        public static void DrawAddCell(Rect cellRect, SerializedProperty list, TableDrawerState state)
        {
            Rect contentRect = new Rect(cellRect);
            contentRect.height = MIN_HEIGHT - 6;
            contentRect.width = MIN_HEIGHT - 3;
            contentRect.center = cellRect.center;
            if (GUI.Button(contentRect, "+", EditorStyles.miniButton))
            {
                list.InsertArrayElementAtIndex(list.arraySize);
                list.serializedObject.ApplyModifiedProperties();
                
                //state.CalculateRowHeights(width, rowCount, list);
                //state.se
                state.GetHeight(list);
                EditorUtility.SetDirty(list.serializedObject.targetObject);
            }
        }
        
        public static void Draw(TableDrawerState state, Rect position, int width, SerializedProperty list)
        {
            int listSize = list.arraySize;
            int rowCount = GetRowCount(listSize, width);
            float height = state.GetHeight(list);
            
            Vector2 cursor = state.Rect.position;
            
            SetupState(state, position, width, list);
            
            HandleResize(state, position, width, list);
            
            state.CalculateRowHeights(width, rowCount, list);
            
            Rect header = DrawHeader(state, cursor, width);
            cursor.y = header.yMax;

            // Iterate through each cell via row and column
            for (int r = 0; r < rowCount; r++)
            {
                cursor.x = state.Rect.x;
                for (int c = 0; c < width; c++)
                {
                    int index = Array2D.GetIndex(c, r, width);
                    
                    Rect cellRect = new Rect(cursor, new Vector2(state.ColumnWidths[c], state.RowHeights[r]));
                    Rect contentRect = Padding.Remove(cellRect);

                    if (IsContextClick(cellRect))
                        DrawContextMenu(list, index);

                    GUI.Box(cellRect, "");
                    if (r%2==0)
                        GUI.Box(cellRect, "");
                    
                    if (index < listSize)
                    {
                        SerializedProperty element = list.GetArrayElementAtIndex(index);
                        DrawCell(contentRect, element);
                    }
                    else
                    {
                        if (listSize == index)
                        {
                            DrawAddCell(cellRect, list, state);
                        }
                    }
                    cursor.x += state.ColumnWidths[c];
                }
                cursor.y += state.RowHeights[r];
            }

            if (Math.Abs(state.GetHeight(list) - height) > 0.1f)
            {
                EditorUtility.SetDirty(list.serializedObject.targetObject);
                EditorApplication.delayCall += () => EditorApplication.delayCall += () => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}