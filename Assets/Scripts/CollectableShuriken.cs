using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CollectableShuriken : NetworkBehaviour , Collectable{

    public GameObject ShurikenPrefab;
    [SyncVar]
    bool isCollected = false;

    public void Collect(Player player) {
        if (isCollected) return;
        isCollected = true;
        if (!isServer) return;
            //Spawn Shurikens in 4 directions
        for (int i = 0; i < 4; i++) {
            var direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * 90 * i), Mathf.Sin(Mathf.Deg2Rad * 90 * i));
            GameObject newShuriken = GameObject.Instantiate(ShurikenPrefab);
            Rigidbody rb = newShuriken.GetComponent<Rigidbody>();
            newShuriken.transform.position = player.transform.position + direction;
            newShuriken.GetComponent<Shuriken>().direction = direction;
             NetworkServer.Spawn(newShuriken);   
        }

        NetworkServer.Destroy(this.gameObject);

        var map = GameObject.FindObjectOfType<MapManager>();
        map.CmdGenerateItem("CollectableShuriken");
    }
}
