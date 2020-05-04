using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Destroyer : MonoBehaviour {
    private void OnCollisionEnter2D( Collision2D a_collision ) {
        Destroy( a_collision.gameObject );
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        Destroy( collision.gameObject );
    }
}
