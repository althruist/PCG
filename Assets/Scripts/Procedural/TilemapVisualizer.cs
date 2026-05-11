using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class TilemapVisualizer : MonoBehaviour
{
    public enum TileType
    {
        Floor,
        WallTop,
        WallBottom,
        WallLeft,
        WallRight,
        WallTopLeft,
        WallTopRight,
        WallBottomLeft,
        WallBottomRight,
        WallSingleHole,
        WallInnerTopLeft,
        WallInnerTopRight,
        WallInnerBottomLeft,
        WallInnerBottomRight,
        WallFull,
        Spawn,
        End,
        Water,
        Lava,
        Decoration,
        Trap
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

    [Header("Tile Lists")]
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

    // creates dictionary of tiles from the inspector
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

    // paints a tile of the requested type/biome at every position
    public void GenerateTiles(TileType tileType, BiomeType biome, IEnumerable<Vector2Int> positions)
    {
        if (!TryGetTileInfo(tileType, biome, out var info)) { return; }

        foreach (var pos in positions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            GenerateTile(info.tilemap, info.tileList, pos);
        }
    }

    // finds the best tile entry
    private bool TryGetTileInfo(TileType tileType, BiomeType biome, out TileInfo info)
    {
        if (tileDictionary.TryGetValue((tileType, biome), out info) && IsUsable(info))
        {
            return true;
        }

        return false;
    }

    // checks that a tile entry has a target tilemap and at least one tile variant
    private bool IsUsable(TileInfo info)
    {
        return info.tilemap != null
            && info.tileList != null
            && info.tileList.Count > 0;
    }

    // converts a grid position into a tilemap cell and paints a random tile variant
    private void GenerateTile(Tilemap tilemap, List<TileBase> tileList, Vector2Int pos)
    {
        var tilePos = tilemap.WorldToCell((Vector3Int)pos);
        tilemap.SetTile(tilePos, tileList[Random.Range(0, tileList.Count)]);
    }

    // clears each tilemap referenced by the configured tile entries once
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
