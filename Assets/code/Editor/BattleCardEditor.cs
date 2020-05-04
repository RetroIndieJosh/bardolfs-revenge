using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( BattleCard ) )]
public class BattleCardScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        DrawDefaultScriptEditor();

        var card = target as BattleCard;
        EditorGUILayout.LabelField( "Summary", EditorStyles.boldLabel );
        GUILayout.Label( card.MultilineSummary, EditorStyles.label );

        EditorGUILayout.LabelField( "Pattern", EditorStyles.boldLabel );
        GUILayout.Label( card.Pattern, EditorStyles.label );
    }
}

#endif // UNITY_EDITOR
