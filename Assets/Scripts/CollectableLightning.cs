using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CollectableLightning : NetworkBehaviour , Collectable{
    [SyncVar]
    bool isCollected = false;

    public void Collect(Player player) {
        if (isCollected) return;
        isCollected = true;
        player.StartCoroutine(player.SpeedBonusRoutine(1.8f, 4));
        NetworkServer.Destroy(this.gameObject);
        var map = GameObject.FindObjectOfType<MapManager>();
        map.CmdGenerateItem("CollectableLightning");
    }
}
