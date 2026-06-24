using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Encounters/Wave Data")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public struct WaveEntry
    {
        public EnemyData enemyData;
        public GameObject enemyPrefab;
    }

    [Header("Samenstelling")]
    public WaveEntry[] enemies;            // totale lijst vijanden in deze wave

    [Header("Trickle spawning")]
    [Tooltip("Max aantal vijanden tegelijk levend/actief in deze wave.")]
    public int maxConcurrent = 3;

    [Tooltip("Minimale tijd tussen twee spawns, ook als er ruimte is (voorkomt spawn-spam).")]
    public float minSpawnInterval = 0.5f;
}