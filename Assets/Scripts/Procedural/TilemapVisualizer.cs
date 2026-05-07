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
        End
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
        if (!TryGetTileInfo(tileType, biome, out var info)) { return; }

        foreach (var pos in positions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            GenerateTile(info.tilemap, info.tileList, pos);
        }
    }

    private bool TryGetTileInfo(TileType tileType, BiomeType biome, out TileInfo info)
    {
        if (tileDictionary.TryGetValue((tileType, biome), out info) && IsUsable(info))
        {
            return true;
        }

        if (IsWallTile(tileType))
        {
            return tileDictionary.TryGetValue((TileType.WallTop, BiomeType.None), out info) && IsUsable(info);
        }

        return false;
    }

    private bool IsUsable(TileInfo info)
    {
        return info.tilemap != null
            && info.tileList != null
            && info.tileList.Count > 0;
    }

    private bool IsWallTile(TileType tileType)
    {
        return tileType == TileType.WallTop
            || tileType == TileType.WallBottom
            || tileType == TileType.WallLeft
            || tileType == TileType.WallRight
            || tileType == TileType.WallTopLeft
            || tileType == TileType.WallTopRight
            || tileType == TileType.WallBottomLeft
            || tileType == TileType.WallBottomRight
            || tileType == TileType.WallInnerTopLeft
            || tileType == TileType.WallInnerTopRight
            || tileType == TileType.WallInnerBottomLeft
            || tileType == TileType.WallInnerBottomRight
            || tileType == TileType.WallSingleHole
            || tileType == TileType.WallFull;
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
