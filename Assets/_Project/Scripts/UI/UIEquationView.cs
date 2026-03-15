// UIEquationView.cs
// Listens for OnEquationUpdated and animates Points, Multiplier, Total labels.
// Uses NumberJuice for count-up, bounce, and color flash effects.
// Responsible ONLY for display — never modifies game state.

using DiceSpirit.Core;
using DiceSpirit.VFX;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace DiceSpirit.UI
{
    public class UIEquationView : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────────
        [Header("Equation Labels")]
        [SerializeField] private TextMeshProUGUI pointsLabel;
        [SerializeField] private TextMeshProUGUI multiplierLabel;
        [SerializeField] private TextMeshProUGUI totalLabel;

        [Header("Total Rect (for bounce scale)")]
        [Tooltip("Assign the RectTransform of the Total label or its parent container")]
        [SerializeField] private RectTransform totalRect;

        [Header("Animation Settings")]
        [SerializeField] private float countUpDuration = 0.6f;
        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.1f); // Gold
        [SerializeField] private Color normalColor = Color.white;

        // ── Cached previous values for count-up start points ─────────────────
        private int _prevPoints = 0;
        private int _prevMultiplier = 10;
        private int _prevTotal = 0;

        // Track running coroutines so we can stop them if a new roll fires
        // before the previous animation finishes
        private Coroutine _animationCoroutine;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnEquationUpdated += HandleEquationUpdated;
        }

        private void OnDisable()
        {
            GameEvents.OnEquationUpdated -= HandleEquationUpdated;
        }

        private void Start()
        {
            // Show default values immediately on scene load
            pointsLabel.text = "0";
            multiplierLabel.text = "10";
            totalLabel.text = "0";
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void HandleEquationUpdated(int points, int multiplier, int total)
        {
            // If a previous animation is still running, stop it cleanly
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(
                AnimateEquation(points, multiplier, total)
            );
        }

        private IEnumerator AnimateEquation(int points, int multiplier, int total)
        {
            // Run Points and Multiplier count-ups in parallel using separate coroutines.
            // We track completion manually so we know when BOTH are done before
            // playing the Total emphasis sequence.
            bool pointsDone = false;
            bool multiplierDone = false;

            StartCoroutine(NumberJuice.CountTo(
                pointsLabel, _prevPoints, points, countUpDuration,
                onComplete: () => pointsDone = true
            ));

            StartCoroutine(NumberJuice.CountTo(
                multiplierLabel, _prevMultiplier, multiplier, countUpDuration * 0.8f,
                onComplete: () => multiplierDone = true
            ));

            // Inserting After the two StartCoroutine CountTo calls:
            StartCoroutine(TickAudioDuringCountUp(countUpDuration));

            // Also bounce the Points label rect
            if (pointsLabel.rectTransform != null)
                StartCoroutine(NumberJuice.BounceScale(pointsLabel.rectTransform));

            // Wait until both count-ups finish
            yield return new WaitUntil(() => pointsDone && multiplierDone);

            // Small pause — builds anticipation before Total reveals
            yield return new WaitForSeconds(0.08f);

            // Snap Total label to new value with emphasis sequence
            totalLabel.text = total.ToString();

            yield return StartCoroutine(NumberJuice.EmphasisSequence(
                pointsLabel, multiplierLabel, totalLabel,
                totalRect, highlightColor, normalColor
            ));

            // Store values for next animation's start point
            _prevPoints = points;
            _prevMultiplier = multiplier;
            _prevTotal = total;

            _animationCoroutine = null;
        }

        private IEnumerator TickAudioDuringCountUp(float duration)
        {
            // Fire a tick sound every 0.06 seconds during the count-up
            float elapsed = 0f;
            float interval = 0.06f;

            while (elapsed < duration)
            {
                AudioManager.Instance?.Play("number_tick");
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }
    }
}

#region Audio Work Done - Checking/Testing

/* Adding `using DiceSpirit.Core;` at the top of `UIEquationView.cs` if not already there.

The `?.` null-conditional on `AudioManager.Instance` means if audio isn't set up, this line silently skips instead of crashing. Defensive coding.

## Final Play Test Checklist:

Before recording, Checking every step mentally:

1) Roll button disables during animation, re-enables after
2) Random result: Points = dice, Multiplier = 10, Total = Points×10
3) Result 6: Multiplier overrides to 2, Total = 6×2 = 12, CardA glows+particles
4) Result 3: Points becomes 13, Total = 13×10 = 130, CardB glows+particles  
5) Non-trigger result: neither card activates
6) History panel updates every roll, shows last 5, newest at top
7) Number labels count up with bounce animation
8) Total label gets bigger bounce than Points/Multiplier
9) Card resets to normal color when next roll starts
10) Debug panel: Force 3 button correctly forces result = 3
11) Debug panel: Force 6 button correctly forces result = 6
12) Debug panel: Clear button returns to random
13) All 4 sounds play at correct moments
14) Console: zero red errors during any of the above tests */
#endregion Audio Work Done - Checking/Testing