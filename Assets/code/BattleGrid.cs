using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CellOwner
{
    Enemy,
    Player,
    None
}

[System.Serializable]
public enum CellType
{
    Floor,
    Hole,
    Solid,
    Air,
    Earth,
    Fire,
    Water,
    Light,
    Dark,
    Emnis,
    DamageEnemy,
    DamagePlayer,
    None
}

public class BattleGrid : MonoBehaviour
{
    static public BattleGrid instance = null;

    [Header( "Enemy Cell Sprites" )]

    [SerializeField]
    Sprite m_enemyFloorSprite = null;

    [SerializeField]
    Sprite m_enemyHoleSprite = null;

    [Header( "Player Cell Sprites" )]

    [SerializeField]
    Sprite m_playerFloorSprite = null;

    [SerializeField]
    Sprite m_playerHoleSprite = null;

    [Header( "Damage Sprites" )]

    [SerializeField]
    Sprite m_damageEnemySprite = null;

    [SerializeField]
    Sprite m_damagePlayerSprite = null;

    public Sprite GetSpriteForCell( CellOwner a_owner, CellType a_type ) {
        switch ( a_type ) {
            case CellType.Floor:
                switch ( a_owner ) {
                    case CellOwner.Enemy: return m_enemyFloorSprite;
                    case CellOwner.Player: return m_playerFloorSprite;
                    default: return null;
                }
            case CellType.Hole:
                switch ( a_owner ) {
                    case CellOwner.Enemy: return m_enemyHoleSprite;
                    case CellOwner.Player: return m_playerHoleSprite;
                    default: return null;
                }
            case CellType.DamageEnemy:
                return m_damageEnemySprite;
            case CellType.DamagePlayer:
                return m_damagePlayerSprite;
            default: return null;
        }
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogErrorFormat( "[BattleGrid] Only one Battle Grid is allowed; destroying duplicate {0}", name );
            Destroy( gameObject );
            return;
        }
        instance = this;
    }
}
