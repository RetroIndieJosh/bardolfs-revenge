using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEncounterArea : MonoBehaviour {
    [SerializeField]
    private bool m_debug = false;

    [Header("Steps")]

    [SerializeField]
    private int m_framesPerStep = 10;

    [SerializeField]
    private int m_stepsBetweenRolls = 100;

    [Header("Threat")]

    [SerializeField]
    private int m_battleChanceIncreasePerThreatLevel = 10;

    [SerializeField]
    private int m_threatLevelMax = 5;

    [SerializeField]
    [Tooltip("Percentage chance of encounter at each roll (0 - 100).")]
    private int m_battleChance = 50;

    [Header("Enemies")]

    [SerializeField]
    private List<BattleActor> m_enemyList = new List<BattleActor>();

    [SerializeField]
    private int m_enemyCountMin = 1;

    [SerializeField]
    private int m_enemyCountMax = 3;

    [SerializeField]
    private Song m_battleMusic = null;

    private bool m_playerIsInside = false;
    private int m_stepCount = 0;
    private int m_threatLevel = 0;

    public void StartBattle() {
        if ( m_debug ) Debug.LogFormat( "[Random Encounter Area] Battle in {0}!", name );

        var enemyCount = Random.Range( m_enemyCountMin, m_enemyCountMax + 1 );
        var enemyList = new List<BattleActor>();
        for( int i = 0; i < enemyCount; ++i ) {
            enemyList.Add( m_enemyList[Random.Range( 0, m_enemyList.Count )] );
        }

        BardolfMapManager.instance.StartBattle( enemyList );
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        if ( collision.tag != "Player" ) return;
        m_playerIsInside = true;
        m_stepCount = 0;
        m_threatLevel = 0;
    }

    private void OnTriggerExit2D( Collider2D collision ) {
        if ( collision.tag != "Player" ) return;
        m_playerIsInside = false;
    }

    private void Start() {
        if( m_enemyList.Count == 0 ) {
            Debug.LogErrorFormat( "[Randomer Encounter Area] {0} has no enemies defined! Destroying", name );
            Destroy( gameObject );
        }
    }

    private int m_frameCount = 0;

    private void Update() {

        // only update if player is here and not in battle
        if ( BattleManager.instance != null || !m_playerIsInside ) return;

        if ( PlayerManager.instance.WorldPlayer.GetComponent<Mover>().IsMoving ) {
            ++m_frameCount;
            if ( m_frameCount > m_framesPerStep ) {
                ++m_stepCount;
                m_frameCount = 0;
            }
        }

        if ( m_stepCount < m_stepsBetweenRolls ) return;
        m_stepCount = 0;

        var roll = Random.Range( 0, 100 );
        var curBattleChance = m_battleChance + m_threatLevel * m_battleChanceIncreasePerThreatLevel;

        if ( m_debug ) Debug.LogFormat( "[Random Encounter Area] {2} roll: {0}/{1}", roll, curBattleChance, name );
        if ( roll > m_battleChance ) {
            m_threatLevel = Mathf.Min( m_threatLevel + 1, m_threatLevelMax );
            if ( m_debug ) Debug.LogFormat( "[Random Encounter Area] No battle in {0}; threat is now {1}", name, 
                m_threatLevel );
            return;
        }

        StartBattle();
        m_threatLevel = 0;
    }
}
