using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleStarter : MonoBehaviour {
    [SerializeField]
    private List<BattleActor> m_enemyList = new List<BattleActor>();

    [SerializeField]
    private bool m_isBoss = false;

    public void StartBattle() {
        BardolfMapManager.instance.StartBattle( m_enemyList, m_isBoss );
    }
}
