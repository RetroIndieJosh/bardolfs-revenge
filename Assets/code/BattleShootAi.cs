using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleShootAi : MonoBehaviour {
    [SerializeField]
    private bool m_debug = false;

    [SerializeField]
    private BattleActor m_actor = null;

    [SerializeField]
    private float m_timeBetweenShotSec = 0.5f;

    [SerializeField]
    [Tooltip("HACK to replace sprite and apply palette without using palette in prefabs")]
    private Sprite m_sprite = null;

    public Sprite Sprite { get { return m_sprite; } }

    private float m_timeSinceLastShotSec = 0.0f;

    private void Awake() {
        m_actor = Utility.RequireComponent( this, m_actor );
    }

    private void Update () {
        m_timeSinceLastShotSec += Time.deltaTime;
        if ( m_timeSinceLastShotSec < m_timeBetweenShotSec ) return;
        Shoot();
	}

    private void Shoot() {
        if ( m_debug ) Debug.LogFormat( "[Battle Shoot AI] {0} shoots", name );
        m_actor.AttackPrimary();
        m_timeSinceLastShotSec = 0.0f;
    }
}
