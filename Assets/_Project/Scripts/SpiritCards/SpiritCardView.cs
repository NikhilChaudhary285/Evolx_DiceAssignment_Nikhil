// SpiritCardView.cs
// Attached to each Spirit Card UI element in the scene.
// Listens for OnSpiritCardActivated — if the ID matches this card, plays VFX.
// Handles: glow pulse, scale punch, color tint, particle burst.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DiceSpirit.Core;
using DiceSpirit.VFX;

namespace DiceSpirit.SpiritCards
{
    public class SpiritCardView : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────────
        [Header("Card Identity")]
        [Tooltip("Must match SpiritCardData.cardId exactly")]
        [SerializeField] private string cardId;

        [Header("References")]
        [SerializeField] private RectTransform cardRect;
        [SerializeField] private Image cardBackground;
        [SerializeField] private TextMeshProUGUI cardNameLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;

        [Tooltip("Optional particle system child on this card for burst VFX")]
        [SerializeField] private ParticleSystem activationParticles;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        [SerializeField] private Color activatedColor = new Color(1f, 0.75f, 0.1f, 1f);
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color activeTextColor = new Color(0.1f, 0.05f, 0f, 1f);

        [SerializeField] private float activationPunchScale = 1.4f;
        [SerializeField] private float glowPulseDuration = 1.2f;

        // ── State ────────────────────────────────────────────────────────────
        private Coroutine _activeCoroutine;
        private bool _isActivated = false;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnSpiritCardActivated += HandleCardActivated;
            // Also listen for new rolls to reset the card state
            GameEvents.OnRollRequested += HandleRollRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnSpiritCardActivated -= HandleCardActivated;
            GameEvents.OnRollRequested -= HandleRollRequested;
        }

        private void Start()
        {
            SetVisualState(activated: false, instant: true);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void HandleCardActivated(string activatedCardId)
        {
            // Only respond if the ID matches THIS card
            if (activatedCardId != cardId) return;

            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);

            _activeCoroutine = StartCoroutine(ActivationSequence());
        }

        private void HandleRollRequested()
        {
            // Reset card to normal state when a new roll begins
            if (_isActivated)
            {
                if (_activeCoroutine != null)
                    StopCoroutine(_activeCoroutine);

                SetVisualState(activated: false, instant: false);
            }
        }

        private IEnumerator ActivationSequence()
        {
            _isActivated = true;

            // Step 1: Immediate color snap to activated tint
            SetVisualState(activated: true, instant: true);

            // Step 2: Big scale punch — "card slams into play"
            yield return StartCoroutine(
                NumberJuice.BounceScale(cardRect,
                                        peakScale: activationPunchScale,
                                        punchTime: 0.14f,
                                        settleTime: 0.22f)
            );

            // Step 3: Fire particles if assigned
            if (activationParticles != null)
            {
                activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                activationParticles.Play();
            }

            // Step 4: Glow pulse — card breathes in its activated state
            yield return StartCoroutine(GlowPulse(cycles: 2));

            _activeCoroutine = null;
        }

        private IEnumerator GlowPulse(int cycles)
        {
            // Pulse between activatedColor and a brighter version
            Color brightColor = activatedColor * 1.4f;
            brightColor.a = 1f;

            for (int i = 0; i < cycles; i++)
            {
                // Pulse bright
                float elapsed = 0f;
                float half = glowPulseDuration * 0.5f;

                while (elapsed < half)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / half);
                    cardBackground.color = Color.Lerp(activatedColor, brightColor, t);
                    yield return null;
                }

                // Pulse dim
                elapsed = 0f;
                while (elapsed < half)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / half);
                    cardBackground.color = Color.Lerp(brightColor, activatedColor, t);
                    yield return null;
                }
            }
        }

        private void SetVisualState(bool activated, bool instant)
        {
            Color targetBg = activated ? activatedColor : normalColor;
            Color targetText = activated ? activeTextColor : normalTextColor;

            if (instant)
            {
                cardBackground.color = targetBg;
                if (cardNameLabel != null) cardNameLabel.color = targetText;
                if (descriptionLabel != null) descriptionLabel.color = targetText;
                return;
            }

            // Animated fade back to normal
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeToNormal(targetBg, targetText));
            _isActivated = false;
        }

        private IEnumerator FadeToNormal(Color targetBg, Color targetText)
        {
            Color startBg = cardBackground.color;
            Color startText = cardNameLabel != null ? cardNameLabel.color : targetText;
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                cardBackground.color = Color.Lerp(startBg, targetBg, t);
                if (cardNameLabel != null) cardNameLabel.color = Color.Lerp(startText, targetText, t);
                if (descriptionLabel != null) descriptionLabel.color = Color.Lerp(startText, targetText, t);
                yield return null;
            }

            cardBackground.color = targetBg;
            _activeCoroutine = null;
        }
    }
}