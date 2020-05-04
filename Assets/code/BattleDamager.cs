using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDamager : MonoBehaviour
{
    [SerializeField]
    private bool m_debug = false;

    [SerializeField]
    private CellOwner m_damageTarget = CellOwner.Enemy;

    [SerializeField]
    private int m_damage = 1;

    [SerializeField]
    private CellType m_panelChange = CellType.None;

    public int Damage { get { return m_damage; } }
    public CellOwner DamageTarget {  get { return m_damageTarget; } }

    BattleCell m_currentCell = null;

    public void OnDamage() {
        // TODO option to keep on damage for repeat hits
        Destroy( gameObject );
    }

    private void OnTriggerEnter2D( Collider2D a_collider ) {
        var cell = a_collider.GetComponent<BattleCell>();
        if ( cell == null ) return;

        if ( m_debug ) Debug.Log( "Prev cell: " + m_currentCell );

        if ( m_currentCell != null ) m_currentCell.ClearDamage();
        m_currentCell = cell;

        if( m_panelChange != CellType.None)
            m_currentCell.CellType = m_panelChange;

        if ( m_debug ) Debug.Log( " New cell: " + m_currentCell );

        m_currentCell.DamageSource = this;
    }

    private void OnDestroy() {
        if ( m_currentCell != null ) m_currentCell.ClearDamage();
    }
}