using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEncounterController : MonoBehaviour
{
    [SerializeField] private Transform EncounterTransform;

    [Header("Waves")] [SerializeField] private WaveData[] _waves;

    [Header("Spawn locaties (in deze arena)")] [SerializeField]
    private EnemySpawnPoint[] _spawnPoints;

    [Header("Gate")] [SerializeField] private MonoBehaviour _gateBehaviour; // moet IEncounterGate implementeren
    private IEncounterGate _gate;

    [Header("Events (optioneel, voor UI/audio)")]
    public System.Action<int> OnWaveStarted; // wave index

    public System.Action OnEncounterCompleted;

    private readonly List<HealthComponent> _aliveThisWave = new();
    private readonly List<HealthComponent> _spawnedThisWave = new();
    private int _currentWaveIndex = -1;
    private int _remainingToSpawnThisWave;
    private float _spawnCooldown;
    private bool _encounterActive;
    private bool _encounterDone;

    private void Awake()
    {
        _gate = _gateBehaviour as IEncounterGate;
        if (_gateBehaviour != null && _gate == null)
            Debug.LogError(
                $"[WaveEncounterController] {name}: assigned gate behaviour does not implement IEncounterGate.");
    }

    public void BeginEncounter()
    {
        if (_encounterActive || _encounterDone) return;
        if (_waves == null || _waves.Length == 0)
        {
            Debug.LogWarning($"[WaveEncounterController] {name}: no waves configured.");
            return;
        }

        _encounterActive = true;
        _gate?.Lock();
        StartWave(0);
    }

    private void StartWave(int index)
    {
        _currentWaveIndex = index;
        var wave = _waves[index];

        _aliveThisWave.Clear();
        _spawnedThisWave.Clear();
        _remainingToSpawnThisWave = wave.enemies.Length;
        _spawnCooldown = 0f;

        OnWaveStarted?.Invoke(index);
    }

    private void Update()
    {
        if (!_encounterActive) return;

        var wave = _waves[_currentWaveIndex];

        // Cleanup: prune any entries that got destroyed without OnDeath firing (edge case safety)
        _aliveThisWave.RemoveAll(h => h == null);

        if (_spawnCooldown > 0f)
            _spawnCooldown -= Time.deltaTime;

        // Trickle spawn: vul aan tot maxConcurrent zolang er nog vijanden in de wave-pool zitten
        while (_remainingToSpawnThisWave > 0
               && _aliveThisWave.Count < wave.maxConcurrent
               && _spawnCooldown <= 0f)
        {
            SpawnNextEnemy(wave);
            _spawnCooldown = wave.minSpawnInterval;
        }

        // Wave geklaard: niets meer te spawnen en niemand meer in leven
        if (_remainingToSpawnThisWave <= 0 && _aliveThisWave.Count == 0)
        {
            AdvanceToNextWaveOrFinish();
        }
    }

    private void SpawnNextEnemy(WaveData wave)
    {
        int entryIndex = wave.enemies.Length - _remainingToSpawnThisWave;
        var entry = wave.enemies[entryIndex];
        _remainingToSpawnThisWave--;

        if (entry.enemyPrefab == null)
        {
            Debug.LogError($"[WaveEncounterController] {name}: wave entry {entryIndex} has no prefab assigned.");
            return;
        }

        Transform point = PickSpawnPoint();
        GameObject go = Instantiate(entry.enemyPrefab, point.position, point.rotation);

        var stateMachine = go.GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
            stateMachine.data = entry.enemyData;

        var health = go.GetComponent<HealthComponent>();
        if (health == null)
        {
            Debug.LogError(
                $"[WaveEncounterController] Spawned enemy '{go.name}' has no HealthComponent — can't track death.");
            return;
        }

        _aliveThisWave.Add(health);
        _spawnedThisWave.Add(health);
        health.OnDeath += _ => HandleEnemyDeath(health);
    }

    private void HandleEnemyDeath(HealthComponent health)
    {
        _aliveThisWave.Remove(health);
    }

    private Transform PickSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
            return transform;

        return _spawnPoints[Random.Range(0, _spawnPoints.Length)].transform;
    }

    private void AdvanceToNextWaveOrFinish()
    {
        int next = _currentWaveIndex + 1;
        if (next < _waves.Length)
        {
            StartWave(next);
        }
        else
        {
            CompleteEncounter();
        }
    }

    private void CompleteEncounter()
    {
        _encounterActive = false;
        _encounterDone = true;
        QuestManager.Instance.CompleteObjective("Vecht!");
        if (EncounterTransform != null)
        {
            QuestManager.Instance.SetObjective(new QuestObjective
            {
                id = "reach_boss!",
                description = "Ga naar de locatie van het laatste gevecht!",
                worldMarkerTarget = EncounterTransform
            });
        }

        _gate?.Unlock();
        OnEncounterCompleted?.Invoke();
    }
}