using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMover : TileMover {
    [Header( "Battle Mover" )]

    [SerializeField]
    private Vector2 m_offset = Vector2.zero;

    [SerializeField]
    private CellOwner m_owner = CellOwner.Enemy;

    private Health m_health = null;

    public override bool CanMove {
        get { return base.CanMove; } 
        set { base.CanMove = value; }
    }

    public override bool IsMoving { get { return base.IsMoving; } }

    public BattleCell CurrentCell {  get { return m_currentCell; } }

    BattleCell m_currentCell = null;

    public override void BeginStopping() {
        base.BeginStopping();
    }

    public override void MoveInDirection( Vector3 a_direction ) {
        MoveInDirection( a_direction.ToDirection() );
    }

    public void MoveInDirection( Direction a_direction ) {
        if ( IsMoving ) return;

        var targetCell = m_currentCell.GetNeighbor( a_direction );
        if ( targetCell == null || targetCell.IsOccupied || targetCell.Owner != m_owner ) return;

        SetCell( targetCell );
        MoveTo( (Vector2)targetCell.transform.position + m_offset );
    }

    public void SetCell( BattleCell a_cell, bool a_setPos = false ) {
        if( m_currentCell != null ) m_currentCell.IsOccupied = false;
        m_currentCell = a_cell;
        m_currentCell.IsOccupied = true;
        if( a_setPos ) {
            var x = m_currentCell.transform.position.x;
            var y = m_currentCell.transform.position.y;
            var z = transform.position.z;
            transform.position = new Vector3( x, y, z );
        }
    }

    protected override void Awake() {
        base.Awake();

        m_health = GetComponent<Health>();
    }

    private void OnDrawGizmos() {
        if ( m_currentCell == null ) return;

        Gizmos.color = Color.white;
        Utility.GizmoArrow( transform.position, m_currentCell.transform.position - transform.position );
    }

    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        base.Update();

        if ( m_currentCell == null || m_currentCell.DamageTarget != m_owner ) return;

        if ( m_owner == CellOwner.Player )
            PlayerManager.instance.PlayerHealth -= m_currentCell.Damage;
        else m_health.ApplyDamage( m_currentCell.Damage );

        m_currentCell.ClearDamage( true );
    }
}
