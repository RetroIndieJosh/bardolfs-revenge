using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( CardDatabase ) )]
public class CardDatabaseScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        DrawDefaultScriptEditor();

        var db = target as CardDatabase;
        if ( GUILayout.Button( "Update IDs" ) )
            db.UpdateIds();
    }
}

#endif // UNITY_EDITOR
