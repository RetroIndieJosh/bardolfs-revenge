using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor( typeof( BattleCell ) )]
public class BattleCellScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        if ( GUILayout.Button( "Update Sprite" ) ) {
            foreach ( BattleCell cell in targets ) {
                cell.UpdateSprite();
            }
        }

        DrawDefaultScriptEditor();
    }
}

#endif // UNITY_EDITOR
