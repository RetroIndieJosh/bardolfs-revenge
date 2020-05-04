using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPlayer : MonoBehaviour {
    [SerializeField]
    [Tooltip("0 = full heal")]
    private int m_amount = 0;

    public void Heal() {
        if ( m_amount == 0 ) PlayerManager.instance.ResetHealth();
        else PlayerManager.instance.PlayerHealth += m_amount;
    }
}
