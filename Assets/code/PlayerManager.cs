using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PlayerManager : MonoBehaviour {
    static public PlayerManager instance = null;

    [SerializeField]
    private bool m_debug = false;

    [SerializeField]
    private GameObject m_player = null;

    [SerializeField]
    private Deck m_libraryDeck = null;

    [SerializeField]
    private Deck m_mainDeck = null;

    [SerializeField]
    [Tooltip("If true, instead of game over on death, call the OnDeath event.")]
    private bool m_useDeathEvent = false;

    [SerializeField]
    private UnityEvent m_onDeath = new UnityEvent();

    [SerializeField]
    private GameObject m_gameOver = null;

    [SerializeField]
    private Health m_playerHealth;

    public int PlayerHealth {
        get { return m_playerHealth.CountAsInt; }
        set {
            if ( m_isDying ) return;

            // TODO make this a DamagePlayer() instead of recalculating diff here
            // positive for healz
            var damage = m_playerHealth.Count - value;
            m_playerHealth.ApplyDamage( Mathf.Abs( damage ) );

            if ( m_debug ) Debug.LogFormat( "[Player Manager] Health is now {0}", m_playerHealth.CountAsInt );

            if ( m_playerHealth.IsDead )
                StartCoroutine( Die() );
        }
    }

    public int PlayerHealthMax {  get { return (int)m_playerHealth.Maximum; } }

    [SerializeField]
    private TextMeshPro m_healthTextMesh = null;

    [SerializeField]
    private MoveController m_moveController = null;

    public Deck Library {  get { return m_libraryDeck; } }
    public Deck MainDeck {  get { return m_mainDeck; } }
    public GameObject WorldPlayer {  get { return m_player; } }
    public MoveController MoveController { get { return m_moveController; } }

    public SpriteRenderer TargetSpriteRenderer { set { GetComponent<BlinkSprite>().TargetSpriteRenderer = value; } }

    // to remember where we were before battle
    private Vector3 m_worldPos = Vector3.zero;

    public void ClearInputs() {
        foreach( var inputKey in GetComponentsInChildren<InputKey>())
            inputKey.DeregisterKey();
    }

    public void EnableBattlePlayer( BattleActor a_playerActor ) {
        WorldPlayer.SetActive( false );

        TargetSpriteRenderer = a_playerActor.GetComponentInChildren<SpriteRenderer>();
        m_moveController.TargetMover = a_playerActor.GetComponent<Mover>();
        m_moveController.RequireReleaseBetweenMoves = true;
        ClearInputs();
    }

    public void EnableWorldPlayer() {
        if ( m_debug ) Debug.LogFormat( "[Player Manager] Enable world player" );
        if ( WorldPlayer.activeSelf ) return;

        WorldPlayer.SetActive( true );

        m_moveController.TargetMover = WorldPlayer.GetComponent<Mover>();
        m_moveController.RequireReleaseBetweenMoves = false;

        TargetSpriteRenderer = WorldPlayer.GetComponentInChildren<SpriteRenderer>();

        ResetInputs();
    }

    public void ResetHealth() {
        m_playerHealth.ResetToMaximum();
    }

    public void ResetInputs() {
        ClearInputs();
        foreach ( var inputKey in WorldPlayer.GetComponentsInChildren<InputKey>() ) {
            if ( m_debug ) Debug.LogFormat( "[Player Manager] Reset input {0}", inputKey.name );
            inputKey.RegisterKey();
        }
        m_moveController.MapKeys();
    }

    private void Awake() {
        if( instance != null ) {
            Debug.LogErrorFormat( "[Player Manager] Duplicate in {0}, destroying", name );
            Destroy( gameObject );
            return;
        }
        instance = this;

        m_playerHealth = Utility.RequireComponent( this, m_playerHealth );
        m_moveController = Utility.RequireComponent( this, m_moveController );
    }

    private void Start() {
        if( m_gameOver != null ) m_gameOver.SetActive( false );
    }

    private void Update() {
        if ( m_healthTextMesh != null ) m_healthTextMesh.text = "" + PlayerHealth;
    }

    [SerializeField]
    private float m_deathDelaySec = 0.0f;

    private bool m_isDying = false;

    private IEnumerator Die() {
        if ( m_isDying ) yield break;
        m_isDying = true;

        if ( m_debug ) Debug.LogFormat( "[Player Manager] Player is dying in {0} seconds", m_deathDelaySec );

        yield return new WaitForSeconds( m_deathDelaySec );

        if ( m_useDeathEvent ) {

            if ( m_debug ) Debug.LogFormat( "[Player Manager] Executing death event" );

            // to stop us immediately dying again
            PlayerHealth = 1;

            m_onDeath.Invoke();
            m_useDeathEvent = false;
            m_onDeath.RemoveAllListeners();
            m_isDying = false;

            yield break;
        }

        Debug.Log( "GAME OVER" );

        if ( m_gameOver != null ) {
            m_gameOver.SetActive( true );
            m_gameOver.transform.position = Camera.main.transform.position;
            m_gameOver.transform.position += Vector3.forward * 10.0f;
        }

        InputManager.instance.IsPaused = true;
        InputManager.instance.PausePage.Clear();
        InputManager.instance.PausePage.AddListenerDown( KeyCode.Escape, delegate { Application.Quit(); } );
    }
}
