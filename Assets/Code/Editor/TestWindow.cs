// ----------------------------------------------------------------------------
//  Copyright © 2017 Schell Games, LLC. All Rights Reserved. 
// 
//  Author: Ryan Hipple
//  Date:   07/26/2017
// ----------------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Code.Editor
{
    public class TestWindow : EditorWindow
    {
        [MenuItem("Test/Window")]
        private static void Open()
        {
            GetWindow<TestWindow>();
        }

        [Serializable]
        public class SampleRowData
        {
            public string Name = "";
            public int X;
        }

        private GUITable<SampleRowData> table;

        private void OnEnable()
        {
            table = new GUITable<SampleRowData>();
            table.AddRow(new SampleRowData {Name = "first skdlfj;lsdkfj ;sldkjf;lskd jf;lksd jf;lksdj ;lfkj sd;lfkj s;dlkfj sd;", X = 1});
            table.AddRow(new SampleRowData { Name = "second", X = 2 });
            table.AddRow(new SampleRowData { Name = "third", X = 3 });

            
            table.AddColumn(DrawFirst, "Name");
            table.AddColumn(DrawSecond, "X");
            table.AddColumn(PrintStuff, "function");

            table.AddCallback = () => table.AddRow(new SampleRowData { Name = "new", X = 0 });
        }

        private float PrintStuff(SampleRowData data, Rect cell)
        {
            if (GUI.Button(cell, "Print"))
            {
                Debug.Log(data.Name);
            }
            return 0.0f;
        }

        private float DrawFirst(SampleRowData data, Rect cell)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.wordWrap = true;
            GUI.Label(cell, data.Name, s);
            return s.CalcHeight(new GUIContent(data.Name), cell.width);
        }

        private float DrawSecond(SampleRowData data, Rect cell)
        {
            GUIContent content = new GUIContent(data.X + "!");
            GUI.Label(cell, content);
            return GUI.skin.label.CalcHeight(content, cell.width);
        }

        private Vector2 scroll;
        private Rect lastTable = new Rect(0,0,200,200);
        private void OnGUI()
        {
            Rect r = new Rect(position);
            r.position = Vector2.zero;
            scroll = GUI.BeginScrollView(r, scroll, lastTable);
            lastTable = table.Draw(r);
            GUI.EndScrollView();
        }
    }
}