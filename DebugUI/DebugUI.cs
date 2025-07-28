using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SyncedControls.Example
{
    public class DebugUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _terminal;
        [SerializeField] private ScrollRect _scroll;

        private static readonly List<TextMeshProUGUI> _terminals = new ();
        private static readonly List<ScrollRect> _scrolls = new ();
        private void Awake()
        {
            if (_terminal == null || _scroll == null)
            { 
                DebugUI.LogError($"[{nameof(DebugUI)}] Terminal or ScrollRect is not assigned in the inspector.");
                return;
            }

            _terminals.Add(_terminal);
            _scrolls.Add(_scroll);
        }

        public void SetTerminalVisible(bool setVisible)
        { 
            gameObject.SetActive(setVisible);
        }

        public static void Log(string logText)
        {
            Log("Log", logText);
        }

        public static void Log(string sourceTitle, string logText)
        {
            if (_terminals == null || _terminals.Count == 0 || _scrolls == null || _scrolls.Count == 0 || _terminals.Count != _scrolls.Count)
            {
                Debug.Log($"[{sourceTitle}]: {logText}");
            }
            else
            {
                Debug.Log($"[{sourceTitle}]: {logText}");
                for (int n = 0; n < _terminals.Count; n++)
                {
                    _terminals[n].text += $"<color=#20FF20>[{sourceTitle}]</color>: {logText}\n";
                    if(_scrolls[n].verticalScrollbar != null)
                        _scrolls[n].verticalScrollbar.value = 0;
                }
            }
        }

        public static void LogWarning(string logText)
        {
            Log("Warning", logText);
        }

        public static void LogWarning(string sourceTitle, string logText)
        {
            if (_terminals == null || _terminals.Count == 0 || _scrolls == null || _scrolls.Count == 0 || _terminals.Count != _scrolls.Count)
            {
                Debug.Log($"[{sourceTitle}]: {logText}");
            }
            else
            {
                Debug.Log($"[{sourceTitle}]: {logText}");
                for (int n = 0; n < _terminals.Count; n++)
                {
                    _terminals[n].text += $"<color=#FFFF20>Warning [{sourceTitle}]</color>: {logText}\n";
                    if (_scrolls[n].verticalScrollbar != null)
                        _scrolls[n].verticalScrollbar.value = 0;
                }
            }
        }

        public static void LogError(string logText)
        {
            Log("Error", logText);
        }

        public static void LogError(string sourceTitle, string logText)
        {
            if (_terminals == null || _terminals.Count == 0 || _scrolls == null || _scrolls.Count == 0 || _terminals.Count != _scrolls.Count)
            {
                Debug.Log($"[{sourceTitle}]: {logText}");
            }
            else
            {
                Debug.Log($"[{sourceTitle}]: {logText}");
                for (int n = 0; n < _terminals.Count; n++)
                {
                    _terminals[n].text += $"<color=#FF2020>Error [{sourceTitle}]</color>: {logText}\n";
                    if (_scrolls[n].verticalScrollbar != null)
                        _scrolls[n].verticalScrollbar.value = 0;
                }
            }
        }

    }
}