// SpiritCardManager.cs
// Listens for the post-set roll event, checks each SpiritCardData condition,
// applies matching effects to GameCalculator, then triggers Recompute().
// Also fires the visual activation event so SpiritCardView knows to play VFX.

using System.Collections.Generic;
using UnityEngine;
using DiceSpirit.Core;

namespace DiceSpirit.SpiritCards
{
    public class SpiritCardManager : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────────
        [Header("Spirit Card Data Assets")]
        [Tooltip("Drag your SpiritCardData ScriptableObject assets here")]
        [SerializeField] private List<SpiritCardData> spiritCards;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnRollComplete_PostSet += HandlePostSetRoll;
        }

        private void OnDisable()
        {
            GameEvents.OnRollComplete_PostSet -= HandlePostSetRoll;
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void HandlePostSetRoll(int diceResult, GameCalculator calculator)
        {
            bool anyCardActivated = false;

            foreach (SpiritCardData card in spiritCards)
            {
                if (!card.ShouldActivate(diceResult))
                    continue;

                // This card's condition was met — apply its effect
                ApplyEffect(card, calculator);

                // Tell the View layer to play VFX for this card
                GameEvents.RaiseSpiritCardActivated(card.cardId);

                anyCardActivated = true;
                Debug.Log($"[SpiritCardManager] Card '{card.cardName}' activated " +
                          $"for dice result {diceResult}");
            }

            // Always call Recompute after checking all cards.
            // Whether cards fired or not, the equation must update.
            calculator.Recompute();

            if (!anyCardActivated)
                Debug.Log($"[SpiritCardManager] No cards triggered for result {diceResult}");
        }

        private void ApplyEffect(SpiritCardData card, GameCalculator calculator)
        {
            switch (card.effectType)
            {
                case CardEffectType.MultiplyOverride:
                    // Override the default multiplier (e.g. 10 → 2)
                    calculator.SetMultiplier(card.effectValue);
                    break;

                case CardEffectType.AddToPoints:
                    // Add bonus points before multiplication (e.g. 3 + 10 = 13)
                    calculator.AddToPoints(card.effectValue);
                    break;

                default:
                    Debug.LogWarning($"[SpiritCardManager] Unhandled effect type: " +
                                     $"{card.effectType} on card '{card.cardName}'");
                    break;
            }
        }
    }
}