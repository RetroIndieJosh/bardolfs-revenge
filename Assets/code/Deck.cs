using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {
    [SerializeField]
    private bool m_debug = false;

    [SerializeField]
    private bool m_debugRandomFill = false;

    [SerializeField]
    private BattleCard[] m_cardList = null;

    public BattleCard this[int i] {
        get {
            if ( m_cardList == null ) {
                if ( m_debug ) Debug.Log( "[Deck] Card list is null, can't get cards." );
                return null;
            }
            if ( i < 0 || i >= CardSlotCount ) {
                if ( m_debug ) Debug.LogFormat( "[Deck] Card index {0} invalid", i );
                return null;
            }
            return m_cardList[i];
        }
        set { SetCard( i, value ); }
    }

    public int CardCount {
        get {
            var count = 0;
            foreach( var card in m_cardList) {
                if ( card != null ) ++count;
            }
            return count;
        }
    }

    public int CardSlotCount {  get { return m_cardList.Length; } }

    public string CardListStr {
        get {
            var list = "";
            for( int i = 0; i < CardSlotCount; ++i ) {
                var card = this[i];

                list += string.Format( "{0}. ", i );
                if ( card == null ) list += "ID -- None\n";
                else list += string.Format( "{0} {1}\n", card.Id.ToString("D2"), card.name );
            }
            return list;
        }
    }

    public string CardListStrConsolidated {
        get {
            Dictionary<BattleCard, int> m_uniqueCardDict = new Dictionary<BattleCard, int>();
            foreach( var card in m_cardList) {
                if ( m_uniqueCardDict.ContainsKey( card ) )
                    ++m_uniqueCardDict[card];
                else m_uniqueCardDict.Add( card, 1 );
            }

            var str = "";
            foreach( var pair in m_uniqueCardDict)
                str += string.Format( "{0}x{1}, ", pair.Value, pair.Key.DisplayName );
            str = str.Substring( 0, str.Length - 2 );

            return str;
        }
    }

    public int CountCardsOfType(BattleCard a_card) {
        var count = 0;
        foreach( var card in m_cardList)
            if ( card == a_card ) ++count;
        return count;
    }

    public BattleCard RandomCard {
        get {
            List<BattleCard> consolidatedDeck = new List<BattleCard>();
            foreach ( var card in m_cardList )
                if ( card != null ) consolidatedDeck.Add( card );
            return consolidatedDeck[Random.Range( 0, consolidatedDeck.Count )];

            /*
            if ( CardCount == 0 ) {
                Debug.LogError( "[Deck] No cards in deck, but tried to get random card." );
                return null;
            }

            var loops = 0;
            while ( true ) {
                var card = m_cardList[Random.Range( 0, m_cardList.Length )];
                if ( card != null ) return card;
                ++loops;
                if( loops > 1000) {
                    Debug.LogError( "[Deck] Infinite loop detected searching for random card. Aborting." );
                    return null;
                }
            }
            */
        }
    }

    public bool AddCard( BattleCard a_card) {
        var targetIndex = -1;
        for( int i = 0; i < m_cardList.Length; ++i ) {
            if( m_cardList[i] == null ) {
                targetIndex = i;
                break;
            }
        }

        if ( targetIndex == -1 ) return false;
        SetCard( targetIndex, a_card );
        return true;
    }
    
    public BattleCard SetCard(int i, BattleCard a_card) {
        if ( i < 0 || i >= m_cardList.Length ) return null;

        var prev = m_cardList[i];
        m_cardList[i] = a_card;
        return prev;
    }

    public void Clear() {
        for ( int i = 0; i < CardSlotCount; ++i ) m_cardList[i] = null;
    }

    public void Initialize() {
        if ( m_debugRandomFill ) {
            for ( int i = 0; i < CardSlotCount; ++i )
                this[i] = CardDatabase.instance.RandomCard;
        }
    }

    private void Awake() {
        Initialize();
    }
}
