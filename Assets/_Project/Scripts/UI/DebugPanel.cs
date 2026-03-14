// DebugPanel.cs
// Debug-only panel for forcing dice results during testing.
// Communicates with DiceRoller via a shared DebugRollSettings object.
// In a real build we would wrap this with #if UNITY_EDITOR or a debug flag.

using DiceSpirit.Dice;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

namespace DiceSpirit.UI
{
    public class DebugPanel : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private DiceRoller diceRoller;

        [Header("Force Buttons")]
        [SerializeField] private Button forceResult3Button;
        [SerializeField] private Button forceResult6Button;
        [SerializeField] private Button clearForceButton;

        [Header("Status Label")]
        [SerializeField] private TextMeshProUGUI statusLabel;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            if (forceResult3Button != null)
                forceResult3Button.onClick.AddListener(() => SetForce(3));

            if (forceResult6Button != null)
                forceResult6Button.onClick.AddListener(() => SetForce(6));

            if (clearForceButton != null)
                clearForceButton.onClick.AddListener(() => SetForce(0));
        }

        private void OnDestroy()
        {
            forceResult3Button?.onClick.RemoveAllListeners();
            forceResult6Button?.onClick.RemoveAllListeners();
            clearForceButton?.onClick.RemoveAllListeners();
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void SetForce(int value)
        {
            // Use the public method on DiceRoller to set the debug value
            diceRoller.SetDebugForceResult(value);

            statusLabel.text = value == 0
                ? "Force: OFF (random)"
                : $"Force: NEXT ROLL = {value}";
        }
    }
}

#region ---- UI | VFX | UIROLLHISTORY | DEBUGPANEL Work Done - Checking/Testing ----

/* Check / Test after all of these Steps

**Namespace errors?** Every script uses a namespace. If we see `type does not exist in namespace`, check:
- `DiceSpirit.Core` -> `GameEvents.cs`, `GameCalculator.cs`
- `DiceSpirit.Dice` -> `DiceRoller.cs`
- `DiceSpirit.SpiritCards` -> `SpiritCardData.cs`, `SpiritCardManager.cs`, `SpiritCardView.cs`
- `DiceSpirit.UI` -> `UIEquationView.cs`, `UIRollButton.cs`, `UIRollHistory.cs`, `DebugPanel.cs`
- `DiceSpirit.VFX` -> `NumberJuice.cs`

## What we have now

1) NumberJuice       — count-up, bounce scale, color flash, emphasis sequence
2) UIEquationView    — animates Points × Multiplier = Total with full juice
3) SpiritCardView    — glow, punch, particle burst, reset on new roll
4) UIRollHistory     — last 5 results with color-coded special values
5) DebugPanel        — force roll = 3 or 6 for testing */
#endregion ---- UI | VFX | UIROLLHISTORY | DEBUGPANEL Work Done - Checking/Testing ----