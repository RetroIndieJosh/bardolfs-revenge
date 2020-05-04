using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover))]
public class BattleMoveAi : MonoBehaviour {
    [System.Serializable]
    enum MoveMode
    {
        RandomDirection,
        RandomWarp
    }

    [SerializeField]
    bool m_debugBattleMoveAi = false;

    [SerializeField]
    MoveMode m_mode = MoveMode.RandomDirection;

    [SerializeField]
    float m_timeBetweenMoveSec = 0.5f;

    [SerializeField]
    bool m_allowIntercardinal = false;

    private BattleActor m_actor = null;
    float m_timeSinceLastMoveSec = 0.0f;
    TileMover m_mover = null;

    private void Awake() {
        m_actor = Utility.RequireComponent( this, m_actor );
        m_mover = Utility.RequireComponent( this, m_mover );
    }

    // TODO should this be coroutine to multithread and keep reaction faster?
    private void Update () {
        m_timeSinceLastMoveSec += Time.deltaTime;
        if ( m_timeSinceLastMoveSec < m_timeBetweenMoveSec ) return;
        Move();
	}

    private void Move() {
        if ( m_mode == MoveMode.RandomDirection ) {
            Direction dir = m_allowIntercardinal ? Utility.RandomDirection() : Utility.RandomDirectionCardinal();
            if ( m_debugBattleMoveAi ) Debug.LogFormat( "[Battle Move AI] {0} moves {1}", name, dir );

            GetComponent<BattleMover>().MoveInDirection( dir );
        }
        m_timeSinceLastMoveSec = 0.0f;
    }
}
