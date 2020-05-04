using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    static public BattleManager instance = null;

    [SerializeField]
    private BattleActor m_playerBattleActor = null;

    [Header( "Battle HUD" )]

    [SerializeField]
    private Counter m_playerHealthCounter = null;

    [SerializeField]
    private Deck m_playerHand = null;

    [SerializeField]
    private DeckMenu m_playerCardMenu = null;

    [SerializeField]
    private GameObject m_victory = null;

    [SerializeField]
    private SpriteRenderer m_currentCardRenderer = null;

    [SerializeField]
    private List<SpriteRenderer> m_nextCardRenderer = new List<SpriteRenderer>();

    [SerializeField]
    private TextMeshPro m_enterMenuTextMesh = null;

    [SerializeField]
    private TextMeshPro m_enemyInfoTextMesh = null;

    [SerializeField]
    private List<CounterBar> m_enemyHealthBarList = new List<CounterBar>();

    [Header("Battle Cells")]

    [SerializeField]
    private BattleCell m_playerCenterCell = null;

    [SerializeField]
    private BattleCell m_enemyCenterCell = null;

    [SerializeField]
    private Counter m_countdown = null;

    [Header("Battle Settings")]

    [SerializeField]
    private int m_battleNameMaxLength = 5;

    [SerializeField]
    private int m_secondsBetweenMenus = 10;

    [SerializeField]
    private GameObject m_bounds = null;

    [Header("Battle Prefabs")]

    // TODO rename to m_spriteRendererPrefab
    [SerializeField]
    private SpriteRenderer m_enemySpriteRendererPrefab = null;

    public SpriteRenderer SpritePrefab {  get { return m_enemySpriteRendererPrefab; } }
    public int BattleNameMaxLength {  get { return m_battleNameMaxLength; } }

    private bool m_canEnterMenu = true;
    private List<GameObject> m_enemyList = new List<GameObject>();
    private bool m_isInBattle = false;
    private List<BattleCard> m_playerCardsAvailable = new List<BattleCard>();
    private List<int> m_selectedCardIndexes = new List<int>();

    public void EnterBattle() {
        InputManager.instance.IsPaused = false;
        MenuManager.instance.ActiveMenu = null;

        m_playerBattleActor.ClearCards();
        foreach ( var cardIndex in m_selectedCardIndexes ) {
            m_playerBattleActor.AddCard( m_playerHand[cardIndex] );
            m_playerHand[cardIndex] = null;
        }

        // we set this here so we can have it change (e.g. FastGauge chip)
        m_countdown.SetRange( 0, m_secondsBetweenMenus );
        m_countdown.ResetToMaximum();
        m_isInBattle = true;
    }

    public void DeselectMostRecentCard() {
        if ( m_selectedCardIndexes.Count == 0 ) return;

        m_selectedCardIndexes.Remove( m_selectedCardIndexes[m_selectedCardIndexes.Count - 1] );
        UpdateCardIndexes();
    }

    // select/deselect
    public void SelectCard( int newCardIndex ) {
        if ( m_playerHand[newCardIndex] == null ) return;

        // can only select matching cards
        if( m_selectedCardIndexes.Count > 0 ) {
            var first = m_playerHand[m_selectedCardIndexes[0]];
            if ( m_playerHand[newCardIndex] != first ) return;
        }

        if ( m_selectedCardIndexes.Contains( newCardIndex ) )
            m_selectedCardIndexes.Remove( newCardIndex );
        else m_selectedCardIndexes.Add( newCardIndex );

        UpdateCardIndexes();
    }

    private void UpdateCardIndexes() {
        m_playerCardMenu.ResetOptionTexts();
        for( int i = 0; i < m_selectedCardIndexes.Count; ++i ) {
            var optionTextMesh = m_playerCardMenu.GetOptionTextMesh( m_selectedCardIndexes[i] );
            optionTextMesh.text = string.Format( "{0}*{1}", i + 1, optionTextMesh.text );
        }
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogErrorFormat( "[Battle Manager] Duplicate in {0}, destroying", name );
            Destroy( gameObject );
            return;
        }
        instance = this;
    }

    private void Start() {
        m_playerCardsAvailable = new List<BattleCard>();
        for ( int i = 0; i < PlayerManager.instance.MainDeck.CardCount; ++i )
            m_playerCardsAvailable.Add( PlayerManager.instance.MainDeck[i] );

        PlayerManager.instance.EnableBattlePlayer( m_playerBattleActor );
        m_playerBattleActor.Mover.SetCell( m_playerCenterCell );

        m_victory.SetActive( false );
        m_currentCardRenderer.transform.parent.gameObject.SetActive( false );
        m_nextCardRenderer[0].transform.parent.gameObject.SetActive( false );

        InputManager.instance.GetPage().AddListenerDown( KeyCode.S, ShowMenu );
        var menuButtonKey = InputManager.GetKeyCodeForGamepadInput( GamepadButton.Back );
        InputManager.instance.GetPage().AddListenerDown( menuButtonKey, ShowMenu );

        foreach( var bar in m_enemyHealthBarList )
            bar.gameObject.SetActive( true );

        m_enemyInfoTextMesh.text = "";
        for ( int i = 0; i < BardolfMapManager.instance.EnemyCount; ++i ) {
            var prefab = BardolfMapManager.instance.GetEnemy( i );
            if ( prefab == null ) {
                Debug.LogError( "[Battle Manager] Got a null enemy" );
                break;
            }

            var enemy = Instantiate( prefab );
            enemy.transform.parent = transform;
            enemy.name = prefab.name + " " + i;

            enemy.GetComponent<TileMover>().Bounds = m_bounds;

            m_enemyList.Add( enemy.gameObject );

            var health = enemy.GetComponent<Health>();
            health.ResetToMaximum();
            health.OnDeathFinal.AddListener( delegate {
                RemoveEnemy( enemy.gameObject );
            } );
            if ( m_enemyHealthBarList.Count > i ) {
                m_enemyHealthBarList[i].gameObject.SetActive( true );
                m_enemyHealthBarList[i].TargetCounter = health;
            }

            // copy over in-game sprite prefab - if it's an actual prefab, we muck up Palette 
            var spriteRenderer = Instantiate( m_enemySpriteRendererPrefab );
            spriteRenderer.transform.parent = enemy.transform;
            spriteRenderer.name = enemy.name + " Sprite";
            spriteRenderer.sprite = enemy.GetComponent<BattleShootAi>().Sprite;
            spriteRenderer.GetComponent<Palette>().UpdateColors();

            var enemyActor = enemy.GetComponent<BattleActor>();
            if ( i == 0 ) enemyActor.Mover.SetCell( m_enemyCenterCell, true );
            else if ( i == 1 ) enemyActor.Mover.SetCell( m_enemyCenterCell.EastCell.NorthCell, true );
            else if ( i == 2 ) enemyActor.Mover.SetCell( m_enemyCenterCell.EastCell.SouthCell, true );
            else if ( i == 3 ) enemyActor.Mover.SetCell( m_enemyCenterCell.WestCell.NorthCell, true );
            else if ( i == 4 ) enemyActor.Mover.SetCell( m_enemyCenterCell.WestCell.SouthCell, true );
            else if ( i == 5 ) enemyActor.Mover.SetCell( m_enemyCenterCell.EastCell, true );
            else if ( i == 6 ) enemyActor.Mover.SetCell( m_enemyCenterCell.SouthCell, true );
            else if ( i == 7 ) enemyActor.Mover.SetCell( m_enemyCenterCell.NorthCell, true );
            else enemyActor.Mover.SetCell( m_enemyCenterCell.WestCell, true );

            m_enemyInfoTextMesh.text += string.Format( "{0, 5} =====\n", enemyActor.BattleName );

            enemy.transform.position += Vector3.up * 0.5f;
        }
    }

    private void Update() {
        if ( m_playerBattleActor == null || m_victory.activeSelf ) return;

        if ( !m_isInBattle ) {
            ShowMenu();
            return;
        }

        UpdateCards();
        //UpdateEnemyInfo();
        UpdateSpellStatus();

        m_playerHealthCounter.Count = PlayerManager.instance.PlayerHealth;
        var maxHealth = PlayerManager.instance.PlayerHealthMax;
        m_playerHealthCounter.SetRange( 0, maxHealth );

        HandlePlayerDeath();

        if ( m_enemyList.Count == 0 ) HandleVictory();
    }

    private void HandleVictory() {
        if( m_playerBattleActor != null ) 
            Destroy( m_playerBattleActor.gameObject );
        m_playerBattleActor = null;

        m_isInBattle = false;
        m_victory.SetActive( true );
        var card = CardDatabase.instance.RandomCard;
        PlayerManager.instance.Library.AddCard( card );
        m_victory.GetComponentInChildren<TextMeshPro>().text = "Victory! You get:\n  " + card.ShortSummary
            + "\n\nRemember: add to your deck.\n\n     [Start]";
    }

    private void UpdateCards() {
        UpdateCardCurrent();
        UpdateCardNext();
    }

    private void UpdateCardCurrent() {
        if ( m_playerBattleActor.CurrentCard == null ) {
            m_currentCardRenderer.sprite = null;
            m_currentCardRenderer.transform.parent.gameObject.SetActive( false );
        } else {
            m_currentCardRenderer.sprite = m_playerBattleActor.CurrentCard.DetailSprite;
            m_currentCardRenderer.transform.parent.gameObject.SetActive( true );
        }
    }

    private void UpdateCardNext() {
        foreach ( var sr in m_nextCardRenderer ) sr.sprite = null;
        if ( m_playerBattleActor.CardCount > 1 ) {
            m_nextCardRenderer[0].transform.parent.gameObject.SetActive( true );

            for ( int i = 1; i < m_playerBattleActor.CardCount; ++i ) {
                if ( i > m_nextCardRenderer.Count ) break;

                if ( m_playerBattleActor[i] == null )
                    m_nextCardRenderer[i - 1].sprite = null;
                else m_nextCardRenderer[i - 1].sprite = m_playerBattleActor[i].IconSprite;
            }
        } else {
            m_nextCardRenderer[0].transform.parent.gameObject.SetActive( false );
        }
    }

    /*
    private void UpdateEnemyInfo() {
        var info = "";
        foreach( var enemy in m_enemyList ) {
            var healthRatio = enemy.GetComponent<Health>().Percent;
            var equalCount = Mathf.CeilToInt( 5 * healthRatio );
            var equals = "";
            for( var i = 0; i < equalCount; ++i )
                equals += "=";

            var actor = enemy.GetComponent<BattleActor>();
            info += string.Format( "{0,5} {1}\n", actor.BattleName, equals );
        }

        m_enemyInfoTextMesh.text = info;
    }
    */

    private void HandlePlayerDeath() {
        if ( PlayerManager.instance.PlayerHealth > 0 ) return;

        m_playerBattleActor.UpdateHealth();
        Destroy( m_playerBattleActor.gameObject );
        m_isInBattle = false;
    }

    private void UpdateSpellStatus() {
        if ( InputManager.instance.IsPaused ) {
            m_enterMenuTextMesh.text = "PAUSED";
            return;
        }

        if ( NoDraw ) {
            m_enterMenuTextMesh.text = string.Format( "No Spells" );
            return;
        }

        m_countdown.Add( -Time.deltaTime );
        if ( m_countdown.RawCount <= Mathf.Epsilon ) {
            m_enterMenuTextMesh.text = "Spells Ready";
            m_canEnterMenu = true;
            return;
        }

        const int TICKS_MAX = 12;
        var ticks = TICKS_MAX - Mathf.FloorToInt( m_countdown.Percent * TICKS_MAX );
        var text = "";
        for ( int i = 0; i < ticks; ++i ) text += "#";
        for ( int i = 0; i < TICKS_MAX + 1 - ticks; ++i ) text += "-";
        m_enterMenuTextMesh.text = text;
    }

    private void RemoveEnemy( GameObject a_enemy ) {
        m_enemyList.Remove( a_enemy );
        Destroy( a_enemy );
    }

    private void DrawHand() {
        for ( int slotIndex = 0; slotIndex < m_playerHand.CardSlotCount; ++slotIndex ) {
            if ( m_playerCardsAvailable.Count == 0 ) break;
            if ( m_playerHand[slotIndex] == null ) {
                var cardIndex = Random.Range( 0, m_playerCardsAvailable.Count );
                m_playerHand[slotIndex] = m_playerCardsAvailable[cardIndex];
                m_playerCardsAvailable.RemoveAt( cardIndex );
            }
        }
    }

    private bool NoDraw { get { return m_playerCardsAvailable.Count == 0 && m_playerHand.CardCount == 0; } }

    private void ShowMenu() {
        if ( m_victory.activeSelf || !m_canEnterMenu || PlayerManager.instance.MainDeck.CardCount == 0 || NoDraw ) {
            m_isInBattle = true;
            return;
        }

        DrawHand();

        m_isInBattle = false;
        m_canEnterMenu = false;
        m_selectedCardIndexes.Clear();

        InputManager.instance.IsPaused = true;

        MenuManager.instance.ActiveMenu = m_playerCardMenu.GetComponent<MenuCursor>();
        m_playerCardMenu.LinkedDeck = m_playerHand;
        PlayerManager.instance.MoveController.ResetState();
    }
}
