using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public static class ChunkInfo {
    public static List<Chunk> Chunks;
    public static Vector2 ChunkSize;

    //Add all the chunk info here
    public static void Init() {
        Chunks = new List<Chunk>();
        ChunkSize = new Vector2(4.8f, 4.8f);

        Chunks.Add(new Chunk(0, 5));
        Chunks.Add(new Chunk(1, 5));
        Chunks.Add(new Chunk(2, 5));
        Chunks.Add(new Chunk(3, 5));
        Chunks.Add(new Chunk(4, 5));
        Chunks.Add(new Chunk(5, 5));
        Chunks.Add(new Chunk(6, 5));
        Chunks.Add(new Chunk(7, 5));
        Chunks.Add(new Chunk(8, 5));
        Chunks.Add(new Chunk(9, 5));
        Chunks.Add(new Chunk(10, 5));
    }

    public static Chunk GetChunk(int id) {
        foreach (var c in Chunks) {
            if (c.ID == id) return c;
        }
        return null;
    }

}
