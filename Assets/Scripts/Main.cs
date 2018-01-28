using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    public static Main Instance;
    public static BroadcastHelper BroadcastHelper;
    public static NetworkManager NetworkManager;
    public static Dictionary<string, Player> ConnectedPlayers;

    public static GameObject Map;

    public string PlayerName;

    public static int MinPlayersToStart = 2;

    GameObject _landing;

	// Use this for initialization
	void Awake () {
        if (Instance != null) {
            GameObject.DestroyImmediate(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        NetworkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        BroadcastHelper = GetComponent<BroadcastHelper>();

        _landing = GameObject.Find("Landing");

        Button hostButton = GameObject.Find("Landing/Host/HostButton").GetComponent<Button>();
        Button clientButton = GameObject.Find("Landing/Client/ClientButton").GetComponent<Button>();
        //Button startButton = GameObject.Find("ServerPanel/Button").GetComponent<Button>();

        Text respawnTimer = GameObject.Find("Canvas").transform.Find("RespawnTimer").GetComponent<Text>();
        respawnTimer.gameObject.SetActive(false);

        //InputField nameInput = GameObject.Find("Landing/NameInput").GetComponent<InputField>();
        //nameInput.onEndEdit.AddListener((s) => {
        //    PlayerName = s;
        //});

        ConnectedPlayers = new Dictionary<string, Player>();

        hostButton.onClick.AddListener(() => {
            Debug.Log("Host button clicked");
            if (Map != null) NetworkServer.Destroy(Map);
            Map = GameObject.Instantiate(Resources.Load("Prefabs/Map") as GameObject);
            Map.name = "Map";
            var mapManager = Map.GetComponent<MapManager>();
            Map.transform.position = Vector3.zero;
            Map.transform.localScale = Vector3.one;
            Map.transform.rotation = Quaternion.identity;
            
            //Start as host
            NetworkManager.StartHost();

            mapManager.GenerateMap(5, 5);
            NetworkServer.Spawn(Map);
            BroadcastHelper.StartServerBroadcast();
            StartGame();
        });

        clientButton.onClick.AddListener(() => {
            Debug.Log("Client button clicked");
            BroadcastHelper.StartClientBroadcast();
            //Lucas was here
            _landing.SetActive(false);

        });

        //MapManager.GenerateMap(System.DateTime.UtcNow.Second, 10, 10);
    }

    void Start() {
        Debug.Log("Start");
        _landing.SetActive(true);
    }

    void StartGame() {
        _landing.SetActive(false);
        StartRound();
    }

    void StartRound() {
        Map.GetComponent<MapManager>().InitPlayersPositions();
    }

    void EndRound() {
        
    }

    public static void AddPlayer(Player newPlayer) {
        ConnectedPlayers.Add(newPlayer.GetComponent<NetworkIdentity>().netId.ToString(), newPlayer);
        Debug.Log("Added Player");
        //if (ConnectedPlayers.Count == MinPlayersToStart) {
        //    int randomPlayerIndex = Random.Range(0, ConnectedPlayers.Count);
        //    int i = 0;
        //    Debug.Log("Entered IF");
        //    foreach (var kvp in ConnectedPlayers) {
        //        Debug.Log("Entered forloop");
        //        if (i == randomPlayerIndex) {
        //            kvp.Value.CmdSetBombPlayer();
        //            break;
        //        }
        //        i++;
        //    }
        //}
    }
}
