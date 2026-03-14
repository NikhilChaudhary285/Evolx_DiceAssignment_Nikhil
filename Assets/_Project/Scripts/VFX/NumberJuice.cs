// NumberJuice.cs
// Reusable coroutine utility for animating number transitions.
// Provides: count-up/count-down, bounce scale punch, color flash.
// Static helpers so any MonoBehaviour can call them without a component reference.
// Usage: StartCoroutine(NumberJuice.CountUp(text, from, to, duration, onComplete));

using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DiceSpirit.VFX
{
    public static class NumberJuice
    {
        // ── Count-Up / Count-Down ────────────────────────────────────────────

        /// Animates a TMP text from startValue to endValue over duration seconds.
        /// Uses an ease-out curve so it starts fast and slows near the end.
        /// onComplete is called when the animation finishes (optional).
        public static IEnumerator CountTo(TextMeshProUGUI label,
                                          int startValue,
                                          int endValue,
                                          float duration,
                                          Action onComplete = null)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Normalized time 0→1
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease-out cubic: fast start, slow finish
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                int displayValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, eased));
                label.text = displayValue.ToString();

                yield return null;
            }

            // Snap to exact final value (removes floating point drift)
            label.text = endValue.ToString();
            onComplete?.Invoke();
        }

        // ── Bounce Scale Punch ───────────────────────────────────────────────

        /// Punches the scale of a RectTransform: grows to peakScale, then
        /// springs back to original. Gives satisfying "pop" on number change.
        public static IEnumerator BounceScale(RectTransform rect,
                                              float peakScale = 1.35f,
                                              float punchTime = 0.12f,
                                              float settleTime = 0.18f)
        {
            Vector3 originalScale = rect.localScale;
            Vector3 peakScaleVec = originalScale * peakScale;

            // Phase 1: grow to peak
            float elapsed = 0f;
            while (elapsed < punchTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / punchTime);
                rect.localScale = Vector3.Lerp(originalScale, peakScaleVec, t);
                yield return null;
            }

            // Phase 2: spring back with slight overshoot feel
            elapsed = 0f;
            while (elapsed < settleTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / settleTime);
                // Ease-out expo for snappy spring return
                float eased = 1f - Mathf.Pow(1f - t, 4f);
                rect.localScale = Vector3.Lerp(peakScaleVec, originalScale, eased);
                yield return null;
            }

            rect.localScale = originalScale;
        }

        // ── Color Flash ──────────────────────────────────────────────────────

        /// Flashes a label to flashColor then fades back to originalColor.
        /// Use for the "= Total" highlight moment.
        public static IEnumerator ColorFlash(TextMeshProUGUI label,
                                             Color flashColor,
                                             Color originalColor,
                                             float flashDuration = 0.15f,
                                             float fadeDuration = 0.35f)
        {
            // Snap to flash color
            label.color = flashColor;
            yield return new WaitForSeconds(flashDuration);

            // Fade back to original
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                label.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }

            label.color = originalColor;
        }

        // ── Equation Emphasis ────────────────────────────────────────────────

        /// Sequence: flash Points → flash Multiplier → flash Total with bigger punch.
        /// Gives the "building up to the result" feel the spec asks for.
        public static IEnumerator EmphasisSequence(TextMeshProUGUI pointsLabel,
                                                   TextMeshProUGUI multiplierLabel,
                                                   TextMeshProUGUI totalLabel,
                                                   RectTransform totalRect,
                                                   Color highlightColor,
                                                   Color normalColor)
        {
            // Brief flash on Points
            yield return ColorFlash(pointsLabel, highlightColor, normalColor,
                                    flashDuration: 0.1f, fadeDuration: 0.2f);
            yield return new WaitForSeconds(0.05f);

            // Brief flash on Multiplier
            yield return ColorFlash(multiplierLabel, highlightColor, normalColor,
                                    flashDuration: 0.1f, fadeDuration: 0.2f);
            yield return new WaitForSeconds(0.08f);

            // Big punch on Total — this is the payoff moment
            yield return BounceScale(totalRect, peakScale: 1.5f,
                                     punchTime: 0.15f, settleTime: 0.25f);
        }
    }
}