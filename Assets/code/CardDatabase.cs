using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour {
    static public CardDatabase instance = null;

    [SerializeField]
    private BattleCard[] m_cardList = new BattleCard[100];

    public BattleCard this[int i] {
        get {
            if ( i < 0 || i >= m_cardList.Length ) return null;
            return m_cardList[i];
        }
    }

    public BattleCard RandomCard { get { return this[Random.Range( 0, m_cardList.Length )]; } }

    public void UpdateIds() {
        for( int i = 0; i < m_cardList.Length; ++i )
            this[i].Id = i;
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogErrorFormat( "[CardDatabase] Duplicate singleton in {0} - destroying", name );
            Destroy( gameObject );
            return;
        }

        instance = this;

        UpdateIds();
    }
}
