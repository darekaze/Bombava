using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapManager : NetworkBehaviour{
    public static ChunkControl[][] ChunkControls;
    [SyncVar]
    public float WidthX;
    [SyncVar]
    public float HeightY;

    public void GenerateMap(int sizeX, int sizeY) {
        //var random = new System.Random(seed);

        ChunkInfo.Init();
        ChunkControls = new ChunkControl[sizeX][];

        //generate map, change chance with distance from center point
        for (int i = 0; i < sizeX; i++) {
            ChunkControls[i] = new ChunkControl[sizeY];
            for (int j = 0; j < sizeY; j++) {
                Chunk newChunk = ChunkInfo.GetChunk(Random.Range(0, ChunkInfo.Chunks.Count));
                GameObject newChunkGO = GameObject.Instantiate(Resources.Load("Prefabs/Chunks/" + newChunk.ID) as GameObject);
                newChunkGO.transform.SetParent(transform);
                WidthX = sizeX * ChunkInfo.ChunkSize.x;
                HeightY = sizeY * ChunkInfo.ChunkSize.y;
                float posX = -(WidthX / 2) + (ChunkInfo.ChunkSize.x / 2) + ChunkInfo.ChunkSize.x * i;
                float posY = -(HeightY / 2) + (ChunkInfo.ChunkSize.y / 2) + ChunkInfo.ChunkSize.y * j;
                newChunkGO.transform.localPosition = new Vector3(posX, posY, 30f);
                var newChunkControl = newChunkGO.AddComponent<ChunkControl>();
                ChunkControls[i][j] = newChunkControl;

                NetworkServer.Spawn(newChunkGO);

                float distX = i - sizeX / 2f;
                float distY = j - sizeY / 2f;
                float distanceFromCenter = Mathf.Sqrt(distX * distX + distY * distY);

                int numOfObstacles = 0;
                float tempNumber = Random.Range(0, distanceFromCenter + 5);
                if (tempNumber > 3) numOfObstacles = 1;
                if (tempNumber > 5) numOfObstacles = 2;

                //Use script from darwin to generate random items (bombs)
                for (int k = 0; k < numOfObstacles; k++) {
                    //int dice = Random.Range(0, 100);
                    //string itemName = null;
                    //if (dice < 45) {
                    //    itemName = "CollectableLightning";
                    //}
                    //else if (dice < 75) {
                    //    itemName = "CollectableBomb";
                    //}
                    //else {
                    //    itemName = "CollectableShuriken";
                    //}
                    //GenerateItem(itemName);

                    int itemPosX = 0, itemPosY = 0;
                    bool placeable = false;
                    while (!placeable) {
                        int childIndex = Random.Range(0, 36);
                        if (!newChunkGO.transform.GetChild(childIndex).GetComponent<BoxCollider>()) {
                            placeable = true;
                            itemPosX = childIndex % 6;
                            itemPosY = childIndex / 6;
                        }
                    }
                    Vector3 pos = new Vector3(-0.4f + itemPosX * 0.16f, -0.4f + itemPosY * 0.16f, -1);
                    //Lucas was here
                    //Create different collectables based on chance
                    GameObject newCollectable;
                    int dice = Random.Range(0, 100);
                    if (dice < 45) {
                        newCollectable = GameObject.Instantiate(Resources.Load("Prefabs/CollectableLightning") as GameObject);
                    }
                    else if (dice < 75) {
                        newCollectable = GameObject.Instantiate(Resources.Load("Prefabs/CollectableBomb") as GameObject);
                    }
                    else {
                        newCollectable = GameObject.Instantiate(Resources.Load("Prefabs/CollectableShuriken") as GameObject);
                    }
                    newCollectable.transform.position = newChunkGO.transform.position + pos * 5f;
                    NetworkServer.Spawn(newCollectable);
                }
            }
        }

        CmdGenerateItem("CollectableBomb");
    }

    [Command]
    public void CmdGenerateItem(string whatItem) {
        Debug.Log("Generate Item");
        int sizeX = ChunkControls[0].Length;
        int sizeY = ChunkControls.Length;
        int chunkIndex = Random.Range(0, sizeX * sizeY);
        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeY; j++) {
                if (j * sizeX + i != chunkIndex) continue;

                int itemPosX = 0, itemPosY = 0;
                bool placeable = false;
                while (!placeable) {
                    int childIndex = Random.Range(0, 36);
                    if (!ChunkControls[i][j].transform.GetChild(childIndex).GetComponent<BoxCollider>()) {
                        placeable = true;
                        itemPosX = childIndex % 6;
                        itemPosY = childIndex / 6;
                    }
                }
                Vector3 pos = new Vector3(-0.4f + itemPosX * 0.16f, -0.4f + itemPosY * 0.16f, -1);
                //Lucas was here
                //Create different collectables based on chance
                GameObject newCollectable;
                if (isServer) {
                    newCollectable = GameObject.Instantiate(Resources.Load("Prefabs/" + whatItem) as GameObject);
                    newCollectable.transform.position = ChunkControls[i][j].transform.position + pos * 5f;
                    NetworkServer.Spawn(newCollectable);
                    break;
                }
            }
        }
    }

    public void InitPlayersPositions() {
        if (transform.childCount == 0) {
            Debug.LogError("Map not initialized");
            return;
        }
        //var random = new System.Random(seed);
        
        int widthX = ChunkControls.Length;
        int heightY = ChunkControls[0].Length;
        float minX = transform.position.x - (widthX * ChunkInfo.ChunkSize.x / 2);
        float maxX = transform.position.x + (widthX * ChunkInfo.ChunkSize.x / 2);
        float minY = transform.position.y - (heightY * ChunkInfo.ChunkSize.y / 2);
        float maxY = transform.position.y + (heightY * ChunkInfo.ChunkSize.y / 2);

        foreach (Player p in Main.ConnectedPlayers.Values) {
            p.transform.position = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
        }
        //Deal with spawning on obstacles case
    }
}
