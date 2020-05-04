using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BardolfMapManager : MapManager {
    [SerializeField]
    private List<BattleActor> m_enemyList = new List<BattleActor>();

    [SerializeField]
    private bool m_loadPlayerAtStart = true;

    new public static BardolfMapManager instance = null;

    public int EnemyCount {  get { return m_enemyList.Count; } }

    public BattleActor GetEnemy(int i ) {
        if ( i < 0 || i > m_enemyList.Count ) return null;
        return m_enemyList[i];
    }

    public void StartBattle( List<BattleActor> a_enemyList ) {
        StartBattle( a_enemyList, false );
    }

    public void StartBattle( List<BattleActor> a_enemyList, bool a_isBoss ) {
        if ( a_isBoss ) Debug.Log( "Start boss battle!" );
        else Debug.Log( "Start battle!" );

        m_enemyList.Clear();
        foreach ( var enemy in a_enemyList ) m_enemyList.Add( enemy );

        var song = a_isBoss ? CurrentMap.BossMusic : CurrentMap.BattleMusic;

        PushMap();
        LoadMap( "battle" );

        // do this AFTER load map so we handle unloading current first (remember song pos)
        SongManager.instance.Play( song );
    }

    override protected void Awake() {
        base.Awake();

        instance = MapManager.instance as BardolfMapManager;
    }

    override protected void Start() {
        PlayerManager.instance.WorldPlayer.SetActive( false );
        base.Start();

    }

    private void SetupPlayerForMap() {
    }
}
