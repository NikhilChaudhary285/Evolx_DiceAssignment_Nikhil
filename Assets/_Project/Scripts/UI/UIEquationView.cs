// UIEquationView.cs
// Listens for OnEquationUpdated and animates Points, Multiplier, Total labels.
// Uses NumberJuice for count-up, bounce, and color flash effects.
// Responsible ONLY for display — never modifies game state.

using System.Collections;
using TMPro;
using UnityEngine;
using DiceSpirit.Core;
using DiceSpirit.VFX;

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
    }
}