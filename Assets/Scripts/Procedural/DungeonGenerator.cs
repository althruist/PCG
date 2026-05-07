using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPos = Vector2Int.zero;
    [SerializeField]
    private int seed = 0;
    [SerializeField]
    private bool useRandomSeed = false;
    [SerializeField, ReadOnly]
    private int lastUsedSeed = 0;

    public int Seed
    {
        get => seed;
        set => seed = value;
    }

    public int LastUsedSeed => lastUsedSeed;

    [Button("Generate Dungeon", ButtonSizes.Gigantic), GUIColor(0.5f, 0.5f, 1f)]
    public void GenerateDungeon()
    {
        var previousRandomState = Random.state;

        try
        {
            lastUsedSeed = useRandomSeed ? Environment.TickCount : seed;
            Random.InitState(lastUsedSeed);

            tilemapVisualizer.Clear();
            RunDungeonGenerator();
        }
        finally
        {
            Random.state = previousRandomState;
        }
    }

    protected abstract void RunDungeonGenerator();
}
