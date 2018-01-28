using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CollectableBomb : NetworkBehaviour , Collectable{

    [SyncVar]
    bool isCollected = false;

    public void Collect(Player player) {
        if (isCollected) return;
        isCollected = true;
        player.CmdSetBombPlayer();
        player.StartCoroutine(player.SpeedBonusRoutine(1.4f, 3));
        NetworkServer.Destroy(this.gameObject);
        var map = GameObject.FindObjectOfType<MapManager>();
        map.CmdGenerateItem("CollectableBomb");
    }
}
