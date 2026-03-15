// AudioManager.cs
// Singleton that manages all SFX playback in the game.
// Other systems call AudioManager.Instance.Play("clip_name") — nothing else.
// Uses a pool of AudioSources to allow overlapping sounds (e.g. number ticks
// playing rapidly while count-up animation runs).

using System.Collections.Generic;
using UnityEngine;
using DiceSpirit.Core;

namespace DiceSpirit.Core
{
    public class AudioManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────
        public static AudioManager Instance { get; private set; }

        // ── Inspector Fields ─────────────────────────────────────────────────
        [Header("Audio Clips")]
        [Tooltip("Assign each clip and give it a key name to call Play() with")]
        [SerializeField] private List<NamedClip> clips;

        [Header("Pool Settings")]
        [Tooltip("Number of AudioSources in the pool — allows overlapping SFX")]
        [SerializeField] private int poolSize = 6;

        [Header("Volume")]
        [Range(0f, 1f)][SerializeField] private float masterVolume = 0.8f;

        // ── Internal ─────────────────────────────────────────────────────────
        private Dictionary<string, AudioClip> _clipMap;
        private List<AudioSource> _sourcePool;
        private int _poolIndex = 0;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildClipMap();
            BuildSourcePool();
        }

        private void OnEnable()
        {
            GameEvents.OnRollRequested += HandleRollRequested;
            GameEvents.OnRollComplete += HandleRollComplete;
            GameEvents.OnSpiritCardActivated += HandleCardActivated;
            GameEvents.OnEquationUpdated += HandleEquationUpdated;
        }

        private void OnDisable()
        {
            GameEvents.OnRollRequested -= HandleRollRequested;
            GameEvents.OnRollComplete -= HandleRollComplete;
            GameEvents.OnSpiritCardActivated -= HandleCardActivated;
            GameEvents.OnEquationUpdated -= HandleEquationUpdated;
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// Play a clip by its registered name key.
        /// Safe to call with an unregistered key — logs warning, no crash.
        public void Play(string clipName, float volumeScale = 1f)
        {
            if (!_clipMap.TryGetValue(clipName, out AudioClip clip))
            {
                Debug.LogWarning($"[AudioManager] Clip not found: '{clipName}'");
                return;
            }

            // Grab next source from pool (round-robin)
            AudioSource source = _sourcePool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % _sourcePool.Count;

            source.clip = clip;
            source.volume = masterVolume * volumeScale;
            source.Play();
        }

        /// Play a clip at a specific pitch (used for number tick variation).
        public void PlayPitched(string clipName, float pitch, float volumeScale = 1f)
        {
            if (!_clipMap.TryGetValue(clipName, out AudioClip clip))
                return;

            AudioSource source = _sourcePool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % _sourcePool.Count;

            source.clip = clip;
            source.pitch = pitch;
            source.volume = masterVolume * volumeScale;
            source.Play();

            // Reset pitch after play so it doesn't affect other clips
            // We use a coroutine-free approach: pitch resets next time this
            // source is grabbed from the pool (in Play() above, pitch isn't set
            // so we reset it here explicitly for safety)
            source.pitch = 1f;
        }

        // ── Event Handlers ───────────────────────────────────────────────────

        private void HandleRollRequested()
        {
            Play("roll_start");
        }

        private void HandleRollComplete(int result)
        {
            Play("roll_settle");
        }

        private void HandleCardActivated(string cardId)
        {
            Play("card_trigger", volumeScale: 1.1f);
        }

        private void HandleEquationUpdated(int points, int multiplier, int total)
        {
            // Small tick sound when numbers update
            // Slight pitch variation makes repeated ticks feel organic
            float pitch = Random.Range(0.9f, 1.1f);
            PlayPitched("number_tick", pitch, volumeScale: 0.6f);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void BuildClipMap()
        {
            _clipMap = new Dictionary<string, AudioClip>(clips.Count);
            foreach (NamedClip nc in clips)
            {
                if (nc.clip == null)
                {
                    Debug.LogWarning($"[AudioManager] Clip entry '{nc.name}' has no clip assigned.");
                    continue;
                }
                _clipMap[nc.name] = nc.clip;
            }
        }

        private void BuildSourcePool()
        {
            _sourcePool = new List<AudioSource>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                // Each source is a child GameObject so the Hierarchy stays clean
                var go = new GameObject($"AudioSource_{i}");
                go.transform.SetParent(transform);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sourcePool.Add(source);
            }
        }
    }

    // ── Data Struct ──────────────────────────────────────────────────────────
    // Kept in the same file since it only serves AudioManager.
    // [System.Serializable] makes it visible in the Inspector.
    [System.Serializable]
    public class NamedClip
    {
        [Tooltip("The key used to play this clip: AudioManager.Instance.Play(\"this_name\")")]
        public string name;
        public AudioClip clip;
    }
}