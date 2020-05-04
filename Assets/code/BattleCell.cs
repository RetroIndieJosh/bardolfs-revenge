using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCell : MonoBehaviour {
    [SerializeField]
    private CellOwner m_owner = CellOwner.Enemy;

    [SerializeField]
    private CellType m_type = CellType.Floor;

    [SerializeField]
    private BattleCell m_eastCell = null;

    [SerializeField]
    private BattleCell m_northCell = null;

    [SerializeField]
    private BattleCell m_southCell = null;

    [SerializeField]
    private BattleCell m_westCell = null;

    [SerializeField]
    private bool m_alwaysDrawConnections = false;

    public BattleCell EastCell {  get { return m_eastCell; } }
    public BattleCell NorthCell {  get { return m_northCell; } }
    public BattleCell SouthCell {  get { return m_southCell; } }
    public BattleCell WestCell {  get { return m_westCell; } }

    //[HideInInspector]
    public bool IsOccupied = false;

    private CellOwner m_originalOwner = CellOwner.Enemy;
    private CellType m_originalType = CellType.Floor;
    private CellType m_prevType = CellType.Floor;

    public CellOwner Owner {
        get { return m_owner; }
        set {
            m_owner = value;
            if ( m_owner == CellOwner.Enemy )
                transform.GetChild( 0 ).gameObject.SetLayer( "Player Solid" );
            else if ( m_owner == CellOwner.Player )
                transform.GetChild( 0 ).gameObject.SetLayer( "Enemy Solid" );
            else 
                transform.GetChild( 0 ).gameObject.SetLayer( "Solid" );
            UpdateSprite();
        }
    }

    public CellType CellType {
        set {
            m_type = value;
            UpdateSprite();
        }
    }

    public int Damage = 0;
    public BattleDamager DamageSource {
        private get { return m_damageSource; }
        set {
            m_damageSource = value;
            if( m_damageSource != null ) Damage = m_damageSource.Damage;
            UpdateSprite();
        }
    }
    public CellOwner DamageTarget {
        get {
            if ( m_type == CellType.DamageEnemy ) return CellOwner.Enemy;
            else if ( m_type == CellType.DamagePlayer ) return CellOwner.Player;
            else return CellOwner.None;
        }
    }
    private BattleDamager m_damageSource = null;
    private SpriteRenderer m_spriteRenderer = null;

    public void ClearDamage( bool a_causedDamage = false ) {
        if ( a_causedDamage && DamageSource != null )
            DamageSource.OnDamage();
        Damage = 0;
        DamageSource = null;
        UpdateSprite();
    }

    public BattleCell GetNeighbor(Direction a_direction ) {
        switch ( a_direction ) {
            case Direction.East: return EastCell;
            case Direction.North: return NorthCell;
            case Direction.South: return SouthCell;
            case Direction.West: return WestCell;
            case Direction.Northeast: return NorthCell == null ? null : NorthCell.EastCell;
            case Direction.Northwest: return NorthCell == null ? null : NorthCell.WestCell;
            case Direction.Southeast: return SouthCell == null ? null : SouthCell.EastCell;
            case Direction.Southwest: return SouthCell == null ? null : SouthCell.WestCell;
            default: return this;
        }
    }

    public void RevertTypeAfter( float a_seconds ) {
        StartCoroutine( RevertTypeAfterCoroutine( a_seconds ) );
    }

    private IEnumerator RevertTypeAfterCoroutine( float a_seconds ) {
        yield return new WaitForSeconds( a_seconds );
        RevertType();
    }

    public void RevertOwnerToOriginal() {
        m_owner = m_originalOwner;
        UpdateSprite();
    }

    public void RevertType() {
        m_type = m_prevType;
        UpdateSprite();
    }

    public void RevertTypeToOriginal() {
        m_type = m_originalType;
        UpdateSprite();
    }

    public void UpdateSprite() {
        if ( Application.isEditor ) {
            m_spriteRenderer = GetComponent<SpriteRenderer>();
            BattleGrid.instance = FindObjectOfType<BattleGrid>();
        }

        if( DamageTarget == CellOwner.Enemy ) {
            m_spriteRenderer.sprite = BattleGrid.instance.GetSpriteForCell( m_owner, CellType.DamageEnemy );
        } else if( DamageTarget == CellOwner.Player ) {
            m_spriteRenderer.sprite = BattleGrid.instance.GetSpriteForCell( m_owner, CellType.DamageEnemy );
        } else m_spriteRenderer.sprite = BattleGrid.instance.GetSpriteForCell( m_owner, m_type );
    }

    private void Awake() {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_originalOwner = m_owner;
        m_originalType = m_type;
        m_prevType = m_type;
    }

    private void OnDrawGizmos() {
        if ( m_alwaysDrawConnections ) DrawConnections();
    }

    private void OnDrawGizmosSelected() {
        DrawConnections();
    }

    private void DrawConnections() {
        if ( m_eastCell != null ) {
            Gizmos.color = Color.green;
            Utility.GizmoArrow( transform.position, m_eastCell.transform.position - transform.position );
        }
        if ( m_northCell != null ) {
            Gizmos.color = Color.red;
            Utility.GizmoArrow( transform.position, m_northCell.transform.position - transform.position );
        }
        if ( m_southCell != null ) {
            Gizmos.color = Color.blue;
            Utility.GizmoArrow( transform.position, m_southCell.transform.position - transform.position );
        }
        if ( m_westCell != null ) {
            Gizmos.color = Color.yellow;
            Utility.GizmoArrow( transform.position, m_westCell.transform.position - transform.position );
        }
    }

    private void Start() {
        UpdateSprite();
    }

    // TODO figure out whether this is a corner or wall based on adjacent cells; add tiles to BattleGrid for corner/wall etc. and set accordingly
}
