// UIRollHistory.cs
// Displays the last 5 dice roll results in a scrolling history panel.
// Listens for OnRollComplete and prepends the new result to the list.
// Uses a fixed-size queue so it always shows exactly the last N results.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DiceSpirit.Core;

namespace DiceSpirit.UI
{
    public class UIRollHistory : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────────
        [Header("References")]
        [Tooltip("Assign 5 TMP labels in order (index 0 = most recent)")]
        [SerializeField] private List<TextMeshProUGUI> historyLabels;

        [Header("Settings")]
        [SerializeField] private int maxHistory = 5;

        [Header("Colors")]
        [SerializeField] private Color resultColor6 = new Color(1f, 0.75f, 0.1f); // Gold for 6
        [SerializeField] private Color resultColor3 = new Color(0.4f, 0.9f, 0.4f); // Green for 3
        [SerializeField] private Color normalColor = Color.white;

        // ── State ────────────────────────────────────────────────────────────
        // Queue automatically handles the "last N" sliding window
        private readonly Queue<int> _history = new Queue<int>();

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnRollComplete += HandleRollComplete;
        }

        private void OnDisable()
        {
            GameEvents.OnRollComplete -= HandleRollComplete;
        }

        private void Start()
        {
            ClearDisplay();
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void HandleRollComplete(int result)
        {
            // Add result to front of history
            _history.Enqueue(result);

            // Keep only the last maxHistory results
            while (_history.Count > maxHistory)
                _history.Dequeue();

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            // Convert queue to array so we can index it
            // Most recent result = last item in queue = index 0 in display
            int[] results = new int[_history.Count];
            _history.CopyTo(results, 0);

            for (int i = 0; i < historyLabels.Count; i++)
            {
                // Display newest first: reverse the array index
                int resultIndex = results.Length - 1 - i;

                if (resultIndex < 0)
                {
                    // No result yet for this slot
                    historyLabels[i].text = "—";
                    historyLabels[i].color = normalColor;
                    continue;
                }

                int value = results[resultIndex];
                historyLabels[i].text = value.ToString();
                historyLabels[i].color = GetColorForResult(value);
            }
        }

        private void ClearDisplay()
        {
            foreach (var label in historyLabels)
            {
                label.text = "—";
                label.color = normalColor;
            }
        }

        private Color GetColorForResult(int result)
        {
            // Color-code special results so history is scannable at a glance
            return result switch
            {
                6 => resultColor6,
                3 => resultColor3,
                _ => normalColor
            };
        }
    }
}