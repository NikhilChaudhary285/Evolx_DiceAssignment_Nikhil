// GameEvents.cs
// Central event bus. All game systems communicate through these static events.
// No script needs a direct reference to another — they just subscribe here.
// This is the Observer pattern: broadcasters and listeners are fully decoupled.

using System;

namespace DiceSpirit.Core
{
    public static class GameEvents
    {
        // Fired by UIRollButton when player presses Roll
        public static event Action OnRollRequested;

        // Fired by DiceRoller when the dice animation finishes
        // int = the final dice face value (1–6)
        public static event Action<int> OnRollComplete;

        // Fired by GameCalculator after Points/Multiplier/Total are updated
        // Args: points, multiplier, total
        public static event Action<int, int, int> OnEquationUpdated;

        // Fired by SpiritCardManager when a card activates
        // string = the card's unique ID (matches SpiritCardData.cardId)
        public static event Action<string> OnSpiritCardActivated;

        // Fired by DiceRoller to lock/unlock the Roll button
        public static event Action<bool> OnRollLockChanged;

        // ── Safe invoke helpers ──────────────────────────────────────────────
        // These check for null before invoking so callers don't need to worry
        // about whether anyone is subscribed yet.

        public static void RaiseRollRequested() => OnRollRequested?.Invoke();
        public static void RaiseRollComplete(int result) => OnRollComplete?.Invoke(result);
        public static void RaiseEquationUpdated(int p, int m, int t)
                                                          => OnEquationUpdated?.Invoke(p, m, t);
        public static void RaiseSpiritCardActivated(string id)
                                                          => OnSpiritCardActivated?.Invoke(id);
        public static void RaiseRollLockChanged(bool isLocked)
                                                          => OnRollLockChanged?.Invoke(isLocked);
    }
}