using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class CommandBarController : MonoBehaviour
    {
        RtsSimulationDriver driver;
        DesktopUiCommandRouter router;
        Text modeText;

        public void Initialize(RtsSimulationDriver simulationDriver, DesktopUiCommandRouter commandRouter)
        {
            driver = simulationDriver;
            router = commandRouter;
            BuildIfNeeded();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S) && router != null)
                router.StopSelected();
            if (Input.GetKeyDown(KeyCode.M) && router != null)
                router.SetMoveMode();
            if (Input.GetKeyDown(KeyCode.A) && router != null)
                router.SetAttackPlaceholderMode();
            if (modeText != null && router != null)
                modeText.text = "Command Mode: " + router.CurrentMode;
        }

        void BuildIfNeeded()
        {
            if (modeText != null)
                return;

            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.05f, 0.06f, 0.08f, 0.88f));

            var layout = GetComponent<HorizontalLayoutGroup>();
            if (layout == null && GetComponent<LayoutGroup>() == null)
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.spacing = 6f;
                layout.childForceExpandWidth = false;
            }

            AddButton("Stop", () => router.StopSelected());
            AddButton("Move", () => router.SetMoveMode());
            AddButton("Attack", () => router.SetAttackPlaceholderMode());
            AddButton("Guard", () => router.Placeholder("Guard"));
            AddButton("Patrol", () => router.Placeholder("Patrol"));
            AddButton("Deploy", () => router.Placeholder("Deploy"));
            AddButton("Repair", () => router.Placeholder("Repair"));
            AddButton("Sell", () => router.Placeholder("Sell"));
            AddButton("Power", () => router.TogglePowerSelected());
            AddButton("Pause", () => router.TogglePause());
            AddButton("Step", () => router.StepTick());
            AddButton("Low Power", () => router.TriggerLowPowerDemo());

            modeText = GetOrCreateText("Mode Text", "Command Mode: Normal", 12, Color.white, TextAnchor.MiddleLeft);
            modeText.rectTransform.sizeDelta = new Vector2(190f, 32f);
        }

        void AddButton(string text, UnityEngine.Events.UnityAction action)
        {
            var child = transform.Find(text);
            var button = child != null ? child.GetComponent<Button>() : null;
            if (button == null)
                button = RtsUiFactory.CreateButton(transform, text, text);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(84f, 34f);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        Text GetOrCreateText(string objectName, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var child = transform.Find(objectName);
            var existing = child != null ? child.GetComponent<Text>() : null;
            return existing != null ? existing : RtsUiFactory.CreateText(transform, objectName, text, fontSize, color, anchor);
        }
    }
}
