using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class RtsStatusLog : MonoBehaviour
    {
        readonly List<string> messages = new List<string>();
        public int maxMessages = 8;
        public bool visible = true;
        public Text outputText;

        string lastMessage = string.Empty;
        int repeatCount;

        void Start()
        {
            BuildIfNeeded();
            gameObject.SetActive(visible);
            RefreshText();
        }

        public void Bind(Text text)
        {
            outputText = text;
            RefreshText();
        }

        public void AddInfo(string message)
        {
            Add("INFO", message);
        }

        public void AddWarning(string message)
        {
            Add("WARN", message);
        }

        public void AddError(string message)
        {
            Add("ERR", message);
        }

        public void AddResult(RtsCommandResult result)
        {
            if (result == null)
                return;

            if (result.Success)
                AddInfo(result.ToString());
            else
                AddWarning(result.ToString());
        }

        public void Clear()
        {
            messages.Clear();
            lastMessage = string.Empty;
            repeatCount = 0;
            RefreshText();
        }

        void Add(string level, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var formatted = "[" + level + "] " + message;
            if (formatted == lastMessage && messages.Count > 0)
            {
                repeatCount++;
                messages[messages.Count - 1] = formatted + " x" + repeatCount;
            }
            else
            {
                lastMessage = formatted;
                repeatCount = 1;
                messages.Add(formatted);
            }

            while (messages.Count > maxMessages)
                messages.RemoveAt(0);

            RefreshText();
        }

        void RefreshText()
        {
            if (outputText == null)
                return;

            outputText.text = messages.Count == 0 ? "Status log ready." : string.Join("\n", messages);
        }

        void BuildIfNeeded()
        {
            if (outputText != null)
                return;

            var rect = gameObject.GetComponent<RectTransform>();
            if (rect == null)
                rect = gameObject.AddComponent<RectTransform>();

            if (gameObject.GetComponent<Image>() == null)
                RtsUiFactory.AddPanel(gameObject, new Color(0.04f, 0.05f, 0.06f, 0.84f));

            outputText = RtsUiFactory.CreateText(transform, "Status Text", "Status log ready.", 12, Color.white, TextAnchor.UpperLeft);
            outputText.rectTransform.offsetMin = new Vector2(8f, 8f);
            outputText.rectTransform.offsetMax = new Vector2(-8f, -8f);
        }
    }
}
