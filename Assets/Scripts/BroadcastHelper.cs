using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BroadcastHelper : NetworkDiscovery{

    public static Main Main;

    void Start() {
        var Main = GetComponent<Main>();
        Initialize();
        StartCoroutine(ListenToBroadcast());
    }

    public void StartServerBroadcast() {
        //broadcastData = Network.player.ipAddress;
        StartAsServer();
    }

    public void StartClientBroadcast() {
        StartAsClient();
    }

    IEnumerator ListenToBroadcast() {
        bool isLooping = true;
        while (isLooping) {
            yield return new WaitForSeconds(0.5f);
            if (broadcastsReceived.Count > 0) {
                isLooping = false;
                foreach (var nbr in broadcastsReceived) {
                    Main.NetworkManager.networkAddress = nbr.Key.Replace("::ffff:","");
                    Debug.Log(Main.NetworkManager.networkAddress);
                    Main.NetworkManager.StartClient();
                    break;
                }
            }
        }
    }

    //public override void OnReceivedBroadcast(string fromAddress, string data) {
        
    //    Main.NetworkManager.networkAddress = fromAddress;
    //    Debug.Log("Message received. Start client");
    //    Main.NetworkManager.StartClient();
        
    //}
}

