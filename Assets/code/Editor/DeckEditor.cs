using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( Deck ) )]
public class DeckScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        var deck = target as Deck;

        if ( GUILayout.Button( "Init Cards" ) ) {
            if ( Application.isEditor ) CardDatabase.instance = FindObjectOfType<CardDatabase>();
            deck.Initialize();
        }

        DrawDefaultScriptEditor();

        EditorGUILayout.LabelField( "Cards (" + deck.CardCount + ")", EditorStyles.boldLabel );
        GUILayout.Label( deck.CardListStr, EditorStyles.label );
    }
}

#endif // UNITY_EDITOR
