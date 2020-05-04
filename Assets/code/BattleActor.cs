using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleActor : MonoBehaviour
{
    [SerializeField]
    private bool m_debug = false;

    [SerializeField]
    private string m_battleName = "MSNG#";

    [SerializeField]
    private TextMeshPro m_healthTextMesh = null;

    [SerializeField]
    private CellOwner m_owner = CellOwner.Enemy;

    [SerializeField]
    private BattleMover m_mover = null;

    [Header("Aiming")]

    [SerializeField]
    private Transform m_shootOrigin = null;

    [SerializeField]
    private Direction m_shootDirection = Direction.NoDirection;

    [SerializeField]
    private bool m_alwaysDrawShootGizmo = false;

    [Header("Weapons & Spells")]

    [SerializeField]
    private BattleCard m_primaryWeaponPrefab = null;

    [SerializeField]
    private List<BattleCard> m_usableCardList = new List<BattleCard>();

    [SerializeField]
    private int m_instantAttackDelayFramesPerUnit = 100;

    public string BattleName {
        get { return m_battleName.Length > 5 ? m_battleName.Substring( 0, 5 ) : m_battleName; }
    }
    public int InstantAttackDelayFramesPerUnit { get { return m_instantAttackDelayFramesPerUnit; } }

    public BattleCard this[int i] {
        get {
            if ( i < 0 || i >= CardCount ) return null;
            return m_usableCardList[i];
        }
    }

    public int CardCount {  get { return m_usableCardList.Count; } }
    public BattleCard CurrentCard { get { return m_usableCardList.Count > 0 ? this[0] : null; } }
    public BattleCell CurrentCell {  get { return m_mover.CurrentCell; } }
    public BattleMover Mover {  get { return m_mover; } }
    public CellOwner Owner {  get { return m_owner; } }
    public BattleCard PrimaryWeapon { get; private set; }
    public BattleCard PrimaryWeaponPrefab { get { return m_primaryWeaponPrefab; } }

    private BattleCard m_activeCard = null;
    private LayerMask m_targetLayerMask = 0;

    public void AddCard( BattleCard a_cardPrefab ) {
        var card = Instantiate( a_cardPrefab );
        card.transform.parent = transform;

        card.DestroyOnUse = true;
        SetupCardDelegates( card );
        m_usableCardList.Add( card );
    }

    public void AttackPrimary() {
        if ( m_debug ) Debug.LogFormat( "[Battle Actor] {0} using primary attack", name );
        UseCard( PrimaryWeapon );
    }

    public void ClearCards() {
        foreach( var card in m_usableCardList)
            Destroy( card.gameObject );
        m_usableCardList.Clear();
    }

    public void UpdateHealth() {
        if ( m_healthTextMesh != null && m_owner == CellOwner.Player )
            m_healthTextMesh.text = string.Format( "{0}", PlayerManager.instance.PlayerHealth.ToString( "D3" ) );
    }

    public void UseCurrentCard() {
        if ( m_debug ) Debug.LogFormat( "[Battle Actor] {0} using current card", name );
        if ( CurrentCard == null ) {
            if ( m_debug ) Debug.LogFormat( "[Battle Actor] {0} can't use current card - no cards remain", name );
            return;
        }
        UseCard( CurrentCard );
    }

    private void Awake() {
        m_mover = Utility.RequireComponent( this, m_mover );
        if( m_battleName.Length > BattleManager.instance.BattleNameMaxLength ) {
            Debug.LogWarningFormat( "[Battle Actor] Name for {0} ({1}) is too long: {2} > 5", name, m_battleName,
                m_battleName.Length, BattleManager.instance.BattleNameMaxLength );
        }
    }

    private void Start() {
        if( m_primaryWeaponPrefab == null ) {
            Debug.LogWarningFormat( "[Battle Actor] No primary weapon set for {0}, can only use spells", name );
            return;
        }

        PrimaryWeapon = Instantiate( m_primaryWeaponPrefab );
        PrimaryWeapon.transform.parent = transform;
        SetupCardDelegates( PrimaryWeapon, true );
    }

    private void OnDrawGizmos() {
        if ( m_alwaysDrawShootGizmo ) DrawShootGizmo();
    }

    private void OnDrawGizmosSelected() {
        DrawShootGizmo();
    }

    private void Update() {
        UpdateHealth();

        if ( m_activeCard != null && !m_activeCard.IsEffectActive ) m_activeCard = null;
    }

    private void DrawShootGizmo() {
        if ( m_shootOrigin == null ) return;
        Gizmos.color = Color.magenta;
        Utility.GizmoArrow( m_shootOrigin.position, m_shootDirection.ToVector2() );
    }

    private void SetupCardDelegates(BattleCard a_card, bool a_isPrimary = false ) {
        a_card.CastStart.AddListener( delegate {
            GetComponentInChildren<Palette>().Invert();
            GetComponent<Mover>().CanMove = false;
        } );
        a_card.CastFinished.AddListener( delegate {
            GetComponentInChildren<Palette>().Invert();
            if ( !a_isPrimary ) m_usableCardList.RemoveAt( 0 );
            GetComponent<Mover>().CanMove = true;
        } );
    }

    private void UseCard(BattleCard a_card ) {
        if ( m_debug ) Debug.LogFormat( "[Battle Actor] {0} is using card '{1}'", name, a_card.name );
        if ( m_activeCard != null ) {
            if ( m_debug ) Debug.LogFormat( "[Battle Actor] {2} can't use '{0}' because '{1}' is active", a_card.name,
                  m_activeCard.name, name );
            return;
        }
        m_activeCard = a_card;
        m_activeCard.Use( this, m_shootDirection );
    }
}
