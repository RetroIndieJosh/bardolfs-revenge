using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeckMenu : MonoBehaviour
{
    [SerializeField]
    private bool m_showCardListNumber = false;

    [SerializeField]
    private Deck m_linkedDeck = null;

    [SerializeField]
    private Deck m_swapDeck = null;

    [SerializeField]
    private Deck m_baseDeck = null;

    [SerializeField]
    private int m_baseIndex = -1;

    [SerializeField]
    private TextMeshPro m_descriptionTextMesh = null;

    [SerializeField]
    private SpriteRenderer m_graphicDisplay = null;

    [SerializeField]
    private DeckMenu m_baseMenu = null;

    [SerializeField]
    private DeckMenu m_swapMenu = null;

    [SerializeField]
    private TextMeshPro m_swapText = null;

    [SerializeField]
    [Tooltip("If 0, fills all available options; otherwise stops at given number")]
    private int m_cardMax = 0;

    public Deck LinkedDeck {
        set {
            m_linkedDeck = value;
            UpdateOptions();
        }
    }

    public TextMeshPro GetOptionTextMesh(int i) {
        return m_menuCursor.OptionList[i].GetComponent<TextMeshPro>();
    }
    
    public Deck SwapDeck {
        set {
            m_swapDeck = value;
            UpdateOptions();
        }
    }

    private int m_selectedIndex = 0;

    private MenuCursor m_menuCursor = null;
    private int m_startIndex = 0;

    public void ClearSwap() {
        SwapDeck = null;
    }

    public void LinkPlayerDeck( bool a_swap ) {
        if ( a_swap ) SwapDeck = PlayerManager.instance.MainDeck;
        else LinkedDeck = PlayerManager.instance.MainDeck;
    }

    public void LinkPlayerLibrary( bool a_swap ) {
        if ( a_swap ) SwapDeck = PlayerManager.instance.Library;
        else LinkedDeck = PlayerManager.instance.Library;
    }

    public void ScrollDown() {
        if ( m_startIndex + m_menuCursor.OptionList.Count >= m_linkedDeck.CardCount ) return;
        m_startIndex++;
        UpdateOptions();
    }

    public void ScrollUp() {
        if ( m_startIndex <= 0 ) return;
        m_startIndex--;
        UpdateOptions();
    }

    private void Awake() {
        m_menuCursor = Utility.RequireComponent( this, m_menuCursor );
    }

    private void Start() {
        UpdateOptions();
    }

    public void ResetOptionTexts() {
        var optionCount = m_cardMax == 0 ? m_menuCursor.OptionList.Count : m_cardMax;
        for ( int i = 0; i < optionCount; ++i ) {
            var cardIndex = i + m_startIndex;
            var card = m_linkedDeck[cardIndex];
            var option = m_menuCursor.OptionList[i];

            var textMesh = option.GetComponent<TextMeshPro>();
            if ( m_showCardListNumber )
                textMesh.text = string.Format( "{0}.", ( cardIndex + 1 ).ToString( "D2" ) );
            else textMesh.text = "";

            if ( card == null ) textMesh.text += "(none)";
            else textMesh.text += string.Format( "{0}", card.ShortSummary );
        }
    }

    private void UpdateOptions() {
        var optionCount = m_cardMax == 0 ? m_menuCursor.OptionList.Count : m_cardMax;
        for ( int i = 0; i < optionCount; ++i ) {
            var cardIndex = i + m_startIndex;
            var card = m_linkedDeck[cardIndex];
            var option = m_menuCursor.OptionList[i];

            option.OnHighlighted.RemoveAllListeners();
            option.OnHighlighted.AddListener( delegate {
                if ( card == null ) return;

                m_selectedIndex = cardIndex;

                if ( m_descriptionTextMesh != null )
                    m_descriptionTextMesh.text = card.LongSummary;
                if ( m_graphicDisplay != null ) {
                    m_graphicDisplay.sprite = card.DetailSprite;
                    var pal = m_graphicDisplay.GetComponent<Palette>();
                    if ( pal != null ) pal.UpdateColors();
                }
            } );

            option.OnSelected.RemoveAllListeners();

            // if we're the base: launch the swap menu on selection
            if( m_swapDeck != null && m_swapMenu != null ) {
                option.OnSelected.AddListener( delegate {
                    if ( card == null ) return;

                    m_swapMenu.transform.parent.gameObject.SetActive( true );
                    m_swapMenu.m_baseMenu = this;
                    m_swapMenu.m_linkedDeck = m_swapDeck;
                    m_swapMenu.m_baseDeck = m_linkedDeck;
                    m_swapMenu.m_baseIndex = cardIndex;

                    m_swapText.text = "Swap " + ( card == null ? "(none)" : card.DisplayName );

                    m_swapMenu.UpdateOptions();
                } );
            }

            // if we're the swap: return to the base menu on selection (after swapping)
            if( m_baseMenu != null ) {
                option.OnSelected.AddListener( delegate {
                    if ( card == null ) return;

                    var si = cardIndex;
                    var baseCard = m_linkedDeck[si];
                    var swapCard = m_baseDeck.SetCard( m_baseIndex, baseCard );
                    m_linkedDeck.SetCard( si, swapCard );

                    Debug.LogFormat( "Swap {0} for {1}", baseCard, swapCard );

                    MenuManager.instance.ActiveMenu = m_baseMenu.GetComponent<MenuCursor>();
                    //m_baseMenu.transform.parent.gameObject.SetActive( true );
                    m_baseMenu.UpdateOptions();
                } );
            }
        }

        ResetOptionTexts();
    }
}
