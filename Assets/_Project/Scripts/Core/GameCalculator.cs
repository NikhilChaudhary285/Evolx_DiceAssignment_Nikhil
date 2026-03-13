// GameCalculator.cs
// Owns all game state: Points, Multiplier, Total.
// Listens for roll results, applies them, then broadcasts updated values.
// SpiritCardManager modifies Points/Multiplier BEFORE we call Recompute(),
// so the final equation always reflects any card effects.

using UnityEngine;
using DiceSpirit.Core;

namespace DiceSpirit.Core
{
    public class GameCalculator : MonoBehaviour
    {
        // ?? Constants ????????????????????????????????????????????????????????
        private const int DefaultMultiplier = 10;

        // ?? State ????????????????????????????????????????????????????????????
        // Public getters so SpiritCardManager can read current values,
        // but only GameCalculator can SET them (private setters).
        public int Points { get; private set; }
        public int Multiplier { get; private set; }
        public int Total { get; private set; }

        // ?? Unity Lifecycle ??????????????????????????????????????????????????
        private void OnEnable()
        {
            // Subscribe to the roll complete event.
            // OnEnable/OnDisable is safer than Start/OnDestroy for event subscription
            // because it handles the object being toggled active/inactive in scenes.
            GameEvents.OnRollComplete += HandleRollComplete;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks and ghost callbacks
            // if this object is destroyed while another object fires the event.
            GameEvents.OnRollComplete -= HandleRollComplete;
        }

        // ?? Public API ???????????????????????????????????????????????????????

        /// Called by SpiritCardManager BEFORE Recompute() to override multiplier.
        public void SetMultiplier(int newMultiplier)
        {
            Multiplier = newMultiplier;
        }

        /// Called by SpiritCardManager BEFORE Recompute() to add bonus points.
        public void AddToPoints(int bonus)
        {
            Points += bonus;
        }

        /// Recalculates Total and fires the equation updated event.
        /// Called once after all card effects have been applied.
        public void Recompute()
        {
            Total = Points * Multiplier;
            GameEvents.RaiseEquationUpdated(Points, Multiplier, Total);
            Debug.Log($"[GameCalculator] {Points} x {Multiplier} = {Total}");
        }

        // ?? Private ??????????????????????????????????????????????????????????

        private void HandleRollComplete(int diceResult)
        {
            // Step 1: Reset to defaults for this new roll
            Points = diceResult;
            Multiplier = DefaultMultiplier;
            Total = 0;

            // Step 2: Let SpiritCardManager check and apply effects.
            // We raise a specific event so the manager can modify our state
            // BEFORE we call Recompute(). The manager will call Recompute()
            // after it's done — or we call it here if no cards fire.
            // We use a direct approach: SpiritCardManager calls back into us.
            // This keeps GameCalculator unaware of card logic entirely.
            GameEvents.RaiseRollComplete_PostSet(diceResult, this);
        }
    }
}