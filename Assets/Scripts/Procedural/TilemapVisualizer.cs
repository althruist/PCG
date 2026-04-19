using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class TilemapVisualizer : MonoBehaviour
{
    public enum TileType
    {
        Floor,
        Wall
    }

    public enum BiomeType
    {
        None,
        Obsidian,
        Slate,
        Ivy
    }

    private class TileInfo
    {
        public Tilemap tilemap;
        public List<TileBase> tileList;
    }

    [Serializable]
    public class TileEntry
    {
        public TileType tileType;
        public BiomeType biome;
        public Tilemap tilemap;
        public List<TileBase> tiles;
    }

    [Header("Floor Tiles per Biome")]
    [SerializeField] private List<TileEntry> tileEntries;

    private Dictionary<(TileType, BiomeType), TileInfo> tileDictionary;

    private void OnEnable()
    {
        BuildDictionary();
    }

    private void OnValidate()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        tileDictionary = new Dictionary<(TileType, BiomeType), TileInfo>();

        foreach (var entry in tileEntries)
        {
            tileDictionary[(entry.tileType, entry.biome)] = new TileInfo
            {
                tilemap = entry.tilemap,
                tileList = entry.tiles
            };
        }
    }

    public void GenerateTiles(TileType tileType, BiomeType biome, IEnumerable<Vector2Int> positions)
    {
        if (!tileDictionary.TryGetValue((tileType, biome), out var info)) { return; }

        foreach (var pos in positions)
        {
            GenerateTile(info.tilemap, info.tileList, pos);
        }
    }

    private void GenerateTile(Tilemap tilemap, List<TileBase> tileList, Vector2Int pos)
    {
        var tilePos = tilemap.WorldToCell((Vector3Int)pos);
        tilemap.SetTile(tilePos, tileList[Random.Range(0, tileList.Count)]);
    }

    public void Clear()
    {
        HashSet<Tilemap> cleared = new HashSet<Tilemap>();

        foreach (var entry in tileDictionary.Values)
        {
            if (cleared.Add(entry.tilemap))
            {
                entry.tilemap.ClearAllTiles();
            }
        }
    }
};