using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum AttackType
{
    Step,
    Immediate
}

[System.Serializable]
public enum Element
{
    Air,
    Earth,
    Fire,
    Water,
    Light,
    Dark,
    Physical,
    Spirit,
    Emnis
}

public class BattleCard : MonoBehaviour
{
    [SerializeField]
    private bool m_debug = false;

    [SerializeField]
    private string m_displayName = "BatCrd";

    [SerializeField]
    private AttackType m_attackType = AttackType.Immediate;

    [SerializeField]
    private Element m_element = Element.Physical;

    [Header( "Grid Change" )]

    [SerializeField]
    private CellOwner m_cellOwnerChange = CellOwner.None;

    [SerializeField]
    private CellType m_cellTypeChange = CellType.None;

    [SerializeField]
    [Tooltip( "The amount of time in seconds before we automatically clear the damage (not changes)" )]
    private float m_damageStayTimeSec = 0.0f;

    [Header( "Attack Stats" )]

    [SerializeField]
    private int m_damage = 1;

    [SerializeField]
    [Tooltip( "The additional distance (in cells) where the attack starts (for Immediate, where raycast starts)" )]
    private int m_startDistance = 0;

    [SerializeField]
    [Tooltip("Number of seconds to wait between each step for Step type after the first (0 = 1 frame)" )]
    private float m_stepDelaySec = 0.0f;

    [SerializeField]
    [Tooltip( "Maximum number of steps to take before ending move (0 = infinite)" )]
    private int m_stepMax = 0;

    [SerializeField]
    [Tooltip("Whether spread applies after hit (if false, applies before) - always true in Immediate")]
    private bool m_spreadAfterHit = false;

    [SerializeField]
    private float m_initialDelaySec = 0.0f;

    [Header("Spread")]

    [SerializeField]
    private bool m_spreadE = false;

    [SerializeField]
    private bool m_spreadNE = false;

    [SerializeField]
    private bool m_spreadN = false;

    [SerializeField]
    private bool m_spreadNW = false;

    [SerializeField]
    private bool m_spreadS = false;

    [SerializeField]
    private bool m_spreadSE = false;

    [SerializeField]
    private bool m_spreadSW = false;

    [SerializeField]
    private bool m_spreadW = false;

    [Header( "Graphics" )]

    [SerializeField]
    [Tooltip("Sprite for display in menu when selected. Ideal: 32x32")]
    private Sprite m_detailSprite = null;

    [SerializeField]
    [Tooltip("Sprite drawn on tiles in each Step when used in combat. Ideal: 24x24")]
    private Sprite m_fieldSprite = null;

    [SerializeField]
    [Tooltip("Sprite for display as a selected card in combat. Ideal: 16x16")]
    private Sprite m_iconSprite = null;

    [Header( "Events" )]

    [SerializeField]
    private UnityEvent m_onUseStart = new UnityEvent();

    [SerializeField]
    private UnityEvent m_onUseFinished = new UnityEvent();

    public Sprite DetailSprite { get { return m_detailSprite; } }
    public Sprite FieldSprite {  get { return m_fieldSprite; } }
    public Sprite IconSprite {  get { return m_iconSprite; } }

    public UnityEvent CastStart {  get { return m_onUseStart; } }
    public UnityEvent CastFinished {  get { return m_onUseFinished; } }

    public string DisplayName {  get { return m_displayName; } }

    public string ShortSummary {
        get { return string.Format( "{0,-6} {1}", m_displayName, m_element.ToString()[0] ); }
    }

    public string LongSummary {
        get {
            return string.Format( "{0} {1}dmg throw{2} rng{3} {4}/{5} Sprd{6}", m_attackType == AttackType.Immediate ? "Immd" : "Step",
          m_damage, m_startDistance, m_stepMax, m_cellOwnerChange, m_cellTypeChange, m_spreadAfterHit ? "Atk" : "Mv" );
        }

    }

    public string MultilineSummary {
        get {
            return string.Format( "ID {0}: {1}\n", Id, name )
                + string.Format( "{0} {1} attack, spread {2} hit\n", m_attackType, m_element, 
                    m_spreadAfterHit ? "after" : "before" )
                + string.Format("Change to {0}/{1}\n", m_cellOwnerChange, m_cellTypeChange )
                + string.Format( "Damage: {0}\n", m_damage )
                + string.Format( "Start: {0} / Range: {1}\n", m_startDistance + 1, m_stepMax )
                + string.Format( "Delay: {0}s initial, {1}s/step", m_initialDelaySec, m_stepDelaySec );
        }
    }

    public string Pattern {
        get {
            var hit = "X";
            var miss = "O";
            return string.Format( "{0}{1}{2}\n{3}X{4}\n{5}{6}{7}",
                m_spreadNW ? hit : miss, m_spreadN ? hit : miss, m_spreadNE ? hit : miss,
                m_spreadW ? hit : miss, m_spreadE ? hit : miss,
                m_spreadSW ? hit : miss, m_spreadS ? hit : miss, m_spreadSE ? hit : miss );
        }
    }

    public bool DestroyOnUse { private get; set; }
    public bool IsEffectActive {
        get { return m_isEffectActive; }
        private set {
            var m_prevInUse = m_isEffectActive;
            m_isEffectActive = value;
        }
    }

    [HideInInspector]
    public int Id = -1;

    private BattleCell m_currentCell = null;
    private Direction m_direction = Direction.NoDirection;
    private int m_stepsTaken = 0;
    private bool m_isEffectActive = false;

    public void Use( BattleActor a_user, Direction a_direction ) {
        m_direction = a_direction;

        // TODO allow multiple use before resolved?
        if ( IsEffectActive ) {
            if ( m_debug ) Debug.LogFormat( "[Battle Card] {0}'s effect is still active; ignoring new use", name );
            return;
        }
        IsEffectActive = true;

        CastStart.Invoke();

        if ( m_debug ) Debug.LogFormat( "[Battle Card] {0} used by {1} toward {2}", name, a_user, a_direction );

        // immediate = raycast to first enemy hit
        if ( m_attackType == AttackType.Immediate ) {
            AttackImmediate( a_user, a_direction );
        // step = step along in direction cell by cell
        } else if( m_attackType == AttackType.Step) {
            AttackStep( a_user, a_direction );
        }
    }

    private void Awake() {
        DestroyOnUse = false;
        IsEffectActive = false;

        if ( m_attackType != AttackType.Step ) m_spreadAfterHit = true;

        // have to stay for at least four frames (to be super safe)
        m_damageStayTimeSec = Mathf.Max( m_damageStayTimeSec, 4.0f / 60.0f );
    }

    private void AttackCell( BattleCell a_cell, BattleActor a_user, bool a_stayForever = false ) {
        if ( a_cell == null ) return;

        if ( m_cellOwnerChange != CellOwner.None && !a_cell.IsOccupied ) 
            a_cell.Owner = m_cellOwnerChange;
        if ( m_cellTypeChange != CellType.None )
            a_cell.CellType = m_cellTypeChange;

        if ( a_user.Owner == CellOwner.Enemy ) a_cell.CellType = CellType.DamagePlayer;
        else a_cell.CellType = CellType.DamageEnemy;
        a_cell.Damage = m_damage;
        if ( !a_stayForever ) a_cell.RevertTypeAfter( m_damageStayTimeSec );

        if ( m_fieldSprite == null ) return;

        PlaceAttackSprite( a_cell.transform.position, m_damageStayTimeSec + 0.1f );
    }

    private void AttackImmediate( BattleActor a_user, Direction a_direction ) {
        StartCoroutine( AttackImmediateCoroutine( a_user, a_direction) );
    }

    // TODO this needs a better name - "immediate" means target is found by raycast
    private IEnumerator AttackImmediateCoroutine( BattleActor a_user, Direction a_direction ) {
        if( m_debug) 
            Debug.LogFormat( "[Battle Card] Fire {0} at {1} target in {2} seconds", name, a_direction, 
                m_initialDelaySec );
        if ( m_initialDelaySec > Mathf.Epsilon ) yield return new WaitForSeconds( m_initialDelaySec );

        CastFinished.Invoke();

        var attackDelaySecPerUnit = a_user.InstantAttackDelayFramesPerUnit / 60.0f;

        var targetLayer = 1 << LayerMask.NameToLayer( a_user.Owner == CellOwner.Enemy ? "Player" : "Enemy" );
        var origin = GetStartCell( a_user.CurrentCell ).transform.position;
        var hit = Physics2D.CircleCast( origin, 0.5f, a_direction.ToVector2(), 1000.0f,
            targetLayer );
        if ( hit ) {
            var distance = Vector2.Distance( a_user.transform.position, hit.collider.transform.position );
            var attackDelaySec = attackDelaySecPerUnit * distance;
            Debug.LogFormat( "[Battle Card] {0} attack delay: {1}s", name, attackDelaySec );
            yield return new WaitForSeconds( attackDelaySec );

            if ( m_debug ) Debug.LogFormat( "Hit {0}", hit.collider );
            var hitActor = hit.collider.GetComponent<BattleActor>();
            if ( hitActor == null ) {
                // TODO we hit something other than an actor, for now just a miss
                // TODO don't forget to use attack delay!
                IsEffectActive = false;
                yield break;
            }

            m_currentCell = hitActor.CurrentCell;
            AttackCurrent( a_user );
        } else {
            // find furthest cell in given direction
            var curCell = a_user.CurrentCell;
            while ( curCell.GetNeighbor( a_direction) != null )
                curCell = curCell.GetNeighbor( a_direction );

            var targetPos = (Vector2)curCell.transform.position + a_direction.ToVector2();

            var distance = Vector2.Distance( a_user.transform.position, targetPos );
            var attackDelaySec = attackDelaySecPerUnit * distance;
            yield return new WaitForSeconds( attackDelaySec );


            // render attack beyond furthest cell
            PlaceAttackSprite( targetPos, m_damageStayTimeSec );
        }

        IsEffectActive = false;
    }

    private void AttackStep( BattleActor a_user, Direction a_direction ) {
        StartCoroutine( AttackStepCoroutine( a_user, a_direction ) );
    }

    private IEnumerator AttackStepCoroutine( BattleActor a_user, Direction a_direction ) {
        if ( m_debug ) Debug.LogFormat( "[Battle Card] {0} taking first step in {1}s", name, m_initialDelaySec );
        if( m_initialDelaySec > Mathf.Epsilon) yield return new WaitForSeconds( m_initialDelaySec );

        CastFinished.Invoke();

        m_currentCell = GetStartCell( a_user.CurrentCell );
        StartCoroutine( Step( a_user, a_direction ) );
    }

    private BattleCell GetStartCell( BattleCell a_originCell ) {
        var startCell = a_originCell;
        for ( int i = 0; i < m_startDistance; ++i )
            startCell = startCell.GetNeighbor( m_direction );
        return startCell;
    }

    private void PlaceAttackSprite(Vector2 a_position, float a_lifeTime ) {
        var attackSprite = Instantiate( BattleManager.instance.SpritePrefab );
        attackSprite.transform.position = a_position;
        attackSprite.sprite = m_fieldSprite;
        Destroy( attackSprite.gameObject, a_lifeTime );
    }

    private void AttackCurrent( BattleActor a_user, bool a_stayForever = false ) {
        AttackCell( m_currentCell, a_user, a_stayForever );
        if ( m_spreadAfterHit ) Spread( a_user );
    }

    private void Spread( BattleActor a_user ) {
        if ( m_spreadE ) AttackCell( m_currentCell.EastCell, a_user );
        if ( m_spreadW ) AttackCell( m_currentCell.WestCell, a_user );

        if ( m_currentCell.NorthCell != null ) {
            if ( m_spreadN ) AttackCell( m_currentCell.NorthCell, a_user );
            if ( m_spreadNE ) AttackCell( m_currentCell.NorthCell.EastCell, a_user );
            if ( m_spreadNW ) AttackCell( m_currentCell.NorthCell.WestCell, a_user );
        }

        if ( m_currentCell.SouthCell != null ) {
            if ( m_spreadS ) AttackCell( m_currentCell.SouthCell, a_user );
            if ( m_spreadSE ) AttackCell( m_currentCell.SouthCell.EastCell, a_user );
            if ( m_spreadSW ) AttackCell( m_currentCell.SouthCell.WestCell, a_user );
        }
    }

    private IEnumerator Step( BattleActor a_user, Direction a_direction) {
        m_currentCell = m_currentCell.GetNeighbor( a_direction );

        // limit range to step max
        if ( m_stepMax > 0 ) {
            ++m_stepsTaken;
            if ( m_stepsTaken > m_stepMax ) {
                m_stepsTaken = 0;
                m_currentCell = null;
            }
        }

        if ( m_currentCell == null ) {
            IsEffectActive = false;
            yield break;
        }

        if( m_debug )
            Debug.LogFormat( "[Battle Card] {0} used by {1} steps {2} to {3}", name, a_user, a_direction, 
                m_currentCell );
        AttackCell( m_currentCell, a_user );
        if ( !m_spreadAfterHit ) Spread( a_user );
        yield return new WaitForSeconds( m_stepDelaySec );

        // recurse
        StartCoroutine( Step( a_user, a_direction ) );
    }
}
