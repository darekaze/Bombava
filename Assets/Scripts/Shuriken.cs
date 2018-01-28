using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Shuriken : NetworkBehaviour {
    [SyncVar]
    public Vector3 direction;

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Player>() != null) {
            other.GetComponent<Player>().Die();
        }
    }

    void Start() {
            GetComponent<Rigidbody>().AddForce(200f * direction);
        if (isServer) {
        }
        else {

        }
    }
}
