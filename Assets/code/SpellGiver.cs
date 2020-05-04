using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellGiver : MonoBehaviour {
    [SerializeField]
    Deck m_cardList = null;

    [SerializeField]
    [Tooltip("If false, add to Library instead.")]
    private bool m_addToDeck = false;

    public void Give( bool a_showDialogue ) {
        var dialogue = new GameObject();
        var page = dialogue.AddComponent<DialoguePage>();
        page.Text = "You get " + m_cardList.CardListStrConsolidated + "! ";

        var targetDeck = m_addToDeck ? PlayerManager.instance.MainDeck : PlayerManager.instance.Library;

        //var addedAll = true;
        for ( int i = 0; i < m_cardList.CardCount; ++i ) {
            if ( !targetDeck.AddCard( m_cardList[i] ) ) {
                //page.Text += " Could only add " + i + " cards; discarding extra.";
                //addedAll = false;
                break;
            }
        }

        //if ( addedAll ) {
            //page.Text += m_cardList.CardCount + " cards ";
            if ( m_addToDeck ) page.Text += "Added to your Deck.";
            else page.Text += "Added to your Library.";
        //}

        if( !m_addToDeck) page.Text += "Remember to add them to your deck!";

        page.Show();
    }
}
