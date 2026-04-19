using UnityEngine;

[CreateAssetMenu(fileName = "DungeonSettings", menuName = "Scriptable Objects/DungeonSettings")]
public class DungeonSettings : ScriptableObject
{
    public int repetitions = 10, walkLength = 10, corridorLength = 10, corridorCount = 10;
}
