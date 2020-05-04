using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( BattleActor ) )]
public class BattleActorScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        DrawDefaultScriptEditor();

        var actor = target as BattleActor;
        EditorGUILayout.LabelField( "Primary Weapon", EditorStyles.boldLabel );
        if ( actor.PrimaryWeaponPrefab == null ) {
            EditorGUILayout.LabelField( "NONE" );
        } else {
            GUILayout.Label( actor.PrimaryWeaponPrefab.MultilineSummary, EditorStyles.label );
            EditorGUILayout.LabelField( "Pattern", EditorStyles.boldLabel );
            GUILayout.Label( actor.PrimaryWeaponPrefab.Pattern, EditorStyles.label );
        }
    }
}

#endif // UNITY_EDITOR
