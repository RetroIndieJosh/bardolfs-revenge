using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BardolfMap : Map {
    [Header("Bardolf Map")]

    [SerializeField]
    private bool m_hasWorldPlayer = true;

    private void Start() {
        OnLoaded.AddListener( delegate( Map m) {
            if ( m_hasWorldPlayer ) PlayerManager.instance.EnableWorldPlayer();
            else PlayerManager.instance.WorldPlayer.SetActive( false );
        } );
    }
}
