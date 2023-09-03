using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Builder))]
public class BuilderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Builder builder = (Builder)target;
        if(GUILayout.Button("Build Piramid"))
        {
            builder.BuildPiramidMap();
        }
    }

}
