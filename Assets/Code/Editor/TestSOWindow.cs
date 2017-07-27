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
    public class TestSOWindow : EditorWindow
    {
        [MenuItem("Test/TestSOWindow")]
        private static void Open()
        {
            GetWindow<TestSOWindow>();
        }

        private SerializedObjectGUITable table;

        private void OnEnable()
        {
            table = new SerializedObjectGUITable();
            string[] guids = AssetDatabase.FindAssets("t:TestData");
            for(int i = 0; i < guids.Length; i++)
                table.AddRow(new SerializedObject(AssetDatabase.LoadAssetAtPath<TestData>(AssetDatabase.GUIDToAssetPath(guids[i]))));

            /*
            table.AddNameColumn("Name");
            table.AddColumn("Name", "Name");
            table.AddColumn("X", "X");
            table.AddColumn("Strings", "Strings");
            table.AddColumn(PrintStuff, "print");*/
            table.AutoAdd(new SerializedObject(AssetDatabase.LoadAssetAtPath<TestData>(AssetDatabase.GUIDToAssetPath(guids[0]))));
        }

        private float PrintStuff(SerializedObject data, Rect cell)
        {
            if (GUI.Button(cell, "Print"))
            {
                Debug.Log(data.FindProperty("Name").stringValue);
            }
            return 0.0f;
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