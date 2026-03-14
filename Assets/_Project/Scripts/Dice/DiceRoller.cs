// DiceRoller.cs
// Handles the dice roll sequence:
//   1. Lock the Roll button (so player can't spam)
//   2. Pick a random result (or forced debug value)
//   3. Play the roll animation (coroutine with rotation)
//   4. Settle on the correct face
//   5. Emit OnRollComplete with the final integer
//   6. Unlock the Roll button

using System.Collections;
using UnityEngine;
using DiceSpirit.Core;

namespace DiceSpirit.Dice
{
    public class DiceRoller : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────────
        [Header("References")]
        [Tooltip("The dice mesh Transform that will be animated (can be this GameObject)")]
        [SerializeField] private Transform diceTransform;

        [Header("Roll Settings")]
        [Tooltip("How long the dice spins before settling (seconds)")]
        [SerializeField] private float rollDuration = 1.2f;

        [Tooltip("Speed of random spinning during roll")]
        [SerializeField] private float spinSpeed = 720f;

        [Header("Debug")]
        [Tooltip("Set to 1–6 to force next roll result. 0 = random.")]
        [Range(0, 6)]
        [SerializeField] private int debugForceResult = 0;

        // ── State ────────────────────────────────────────────────────────────
        private bool _isRolling = false;

        // Each face value maps to a specific local Euler rotation
        // so the dice mesh shows the correct number face-up.
        // Adjust these values to match your actual dice mesh orientation.
        private readonly Quaternion[] _faceRotations = new Quaternion[7]
        {
            Quaternion.identity,                          // index 0 unused
            Quaternion.Euler(  0,   0,   0),             // face 1
            Quaternion.Euler(180,   0,   0),             // face 2
            Quaternion.Euler( 90,   0,   0),             // face 3
            Quaternion.Euler(-90,   0,   0),             // face 4
            Quaternion.Euler(  0,   0,  90),             // face 5
            Quaternion.Euler(  0,   0, -90),             // face 6
        };

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnRollRequested += HandleRollRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnRollRequested -= HandleRollRequested;
        }

        private void Start()
        {
            // Ensure diceTransform falls back to this object if not assigned
            if (diceTransform == null)
                diceTransform = transform;
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void HandleRollRequested()
        {
            // Guard: ignore if already rolling
            if (_isRolling) return;

            StartCoroutine(RollCoroutine());
        }

        private IEnumerator RollCoroutine()
        {
            _isRolling = true;

            // Lock UI so player can't roll again mid-animation
            GameEvents.RaiseRollLockChanged(isLocked: true);

            // Determine result now (before animation, so we can settle correctly)
            int result = GetRollResult();

            // ── Phase 1: Spin randomly for rollDuration ──────────────────────
            float elapsed = 0f;
            while (elapsed < rollDuration)
            {
                // Spin on all axes for chaotic tumbling feel
                float step = spinSpeed * Time.deltaTime;
                diceTransform.Rotate(step * 1.0f, step * 0.7f, step * 0.5f,
                                     Space.Self);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // ── Phase 2: Snap-settle to correct face ────────────────────────
            // Smooth rotate to final face over 0.25 seconds
            Quaternion targetRotation = _faceRotations[result];
            float settleTime = 0.25f;
            float settleElapsed = 0f;
            Quaternion startRotation = diceTransform.localRotation;

            while (settleElapsed < settleTime)
            {
                settleElapsed += Time.deltaTime;
                float t = settleElapsed / settleTime;
                // Ease-out curve: fast start, smooth landing
                t = 1f - (1f - t) * (1f - t);
                diceTransform.localRotation = Quaternion.Lerp(startRotation,
                                                              targetRotation, t);
                yield return null;
            }

            // Snap exactly to target (removes any floating-point drift)
            diceTransform.localRotation = targetRotation;

            // ── Phase 3: Broadcast result ────────────────────────────────────
            GameEvents.RaiseRollComplete(result);
            Debug.Log($"[DiceRoller] Roll complete. Result: {result}");

            // Unlock UI
            GameEvents.RaiseRollLockChanged(isLocked: false);
            _isRolling = false;
        }

        private int GetRollResult()
        {
            // If debugForceResult is set (1–6), use it. Otherwise random.
            if (debugForceResult >= 1 && debugForceResult <= 6)
            {
                Debug.Log($"[DiceRoller] DEBUG: Forcing result = {debugForceResult}");
                return debugForceResult;
            }

            return Random.Range(1, 7); // Unity's Random.Range int is min-inclusive, max-exclusive
        }

        /// Called by DebugPanel to force the next roll result (0 = random).
        public void SetDebugForceResult(int value)
        {
            debugForceResult = Mathf.Clamp(value, 0, 6);
        }
    }
}