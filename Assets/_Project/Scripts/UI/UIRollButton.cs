// UIRollButton.cs
// Handles the Roll button. When clicked, fires OnRollRequested into the event bus.
// Listens for OnRollLockChanged to enable/disable itself during animation.
// This script knows NOTHING about dice logic — it only fires and listens to events.

using DiceSpirit.Core;
using System.Numerics;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Audio.GeneratorInstance;

namespace DiceSpirit.UI
{
    public class UIRollButton : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────────
        [SerializeField] private Button rollButton;
        [SerializeField] private float rollDebounce = 0.2f; // Extra padding time

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            // Fallback: try to find Button on this GameObject if not assigned
            if (rollButton == null)
                rollButton = GetComponent<Button>();

            // Wire the click. Using onClick.AddListener is cleaner than
            // setting it in the Inspector for script-driven buttons.
            rollButton.onClick.AddListener(OnRollButtonClicked);
        }

        private void OnEnable()
        {
            GameEvents.OnRollLockChanged += HandleRollLockChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnRollLockChanged -= HandleRollLockChanged;
        }

        private void OnDestroy()
        {
            // Remove listener to avoid memory leaks
            rollButton.onClick.RemoveListener(OnRollButtonClicked);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void OnRollButtonClicked()
        {
            GameEvents.RaiseRollRequested();
        }

        private void HandleRollLockChanged(bool isLocked)
        {
            if (isLocked)
            {
                // Disable immediately when the roll starts
                rollButton.interactable = false;
            }
            else
            {
                // Start the debounce routine to wait before re-enabling
                StartCoroutine(DebounceEnableRoutine());
            }
        }

        private IEnumerator DebounceEnableRoutine()
        {
            // Wait for the specified debounce time
            yield return new WaitForSeconds(rollDebounce);

            // Re-enable the button
            rollButton.interactable = true;
        }

    }
}

#region ---- Logic Work Done - Checking/Testing ----

/* ## Now we will Check / Test after all of these Steps

**What to look for if we have errors:**
- `namespace not found` -> check our `using` statements at the top of each file
- `type does not exist` -> check the filename matches the class name exactly
- `missing ;` -> C# syntax error, check the line number shown

** Quick test in Play mode(no scene setup yet):**
- We can't fully test yet because we haven't built the scene
- But we can verify: By creating an empty GameObject in the scene, add `GameCalculator` component, add `SpiritCardManager` component — Unity should not throw any errors when we press Play

## What we have now

1) GameEvents      — event bus (all systems talk through this)
2) SpiritCardData  — ScriptableObject data for each card
3) CardA + CardB   — actual card asset files with data filled in
4) GameCalculator  — owns Points/Multiplier/Total, handles math
5) SpiritCardManager — checks card conditions, applies effects
6) DiceRoller      — runs animation coroutine, emits result
7) UIRollButton    — fires roll event, locks itself during roll */
#endregion ---- Logic Work Done - Checking/Testing ----