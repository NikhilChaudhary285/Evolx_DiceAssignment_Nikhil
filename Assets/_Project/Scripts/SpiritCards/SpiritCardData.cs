// SpiritCardData.cs
// ScriptableObject that defines a single Spirit Card's rules.
// To create a new card: right-click in Project → Create → DiceSpirit → Spirit Card Data
// No code changes needed to add new cards — just create a new asset file.

using UnityEngine;

namespace DiceSpirit.SpiritCards
{
    // This enum defines what type of effect a card can apply.
    // Adding a new effect type = add an entry here + handle it in SpiritCardManager.
    public enum CardEffectType
    {
        MultiplyOverride,   // Replaces the default multiplier with a new value
        AddToPoints         // Adds a flat value to Points
    }

    [CreateAssetMenu(menuName = "DiceSpirit/Spirit Card Data", fileName = "New SpiritCardData")]
    public class SpiritCardData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique ID used to match this card to its View in the scene")]
        public string cardId;

        [Tooltip("Display name shown on the card UI")]
        public string cardName;

        [TextArea(2, 4)]
        [Tooltip("Flavour text shown on the card")]
        public string description;

        [Header("Trigger Condition")]
        [Tooltip("The dice result (1–6) that activates this card")]
        [Range(1, 6)]
        public int triggerOnDiceValue;

        [Header("Effect")]
        public CardEffectType effectType;

        [Tooltip("The value used by the effect (e.g. 2 for MultiplyOverride, 10 for AddToPoints)")]
        public int effectValue;

        // ── Logic ────────────────────────────────────────────────────────────

        /// Returns true if this card should activate for the given dice result.
        public bool ShouldActivate(int diceResult)
        {
            return diceResult == triggerOnDiceValue;
        }
    }
}