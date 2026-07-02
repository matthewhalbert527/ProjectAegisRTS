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
        RectTransform buttonRoot;

        public bool showDeveloperCommands;

        public void Initialize(RtsSimulationDriver simulationDriver, DesktopUiCommandRouter commandRouter)
        {
            driver = simulationDriver;
            router = commandRouter;
            BuildIfNeeded();
        }

        void Update()
        {
            var pause = FindAnyObjectByType<PauseMenuController>();
            if (pause != null && pause.BlocksGameplayInput())
                return;

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

            RemoveLayoutGroups(gameObject);

            modeText = GetOrCreateText("Mode Text", "Command Mode: Normal", 12, Color.white, TextAnchor.MiddleLeft);
            modeText.rectTransform.anchorMin = new Vector2(0f, 1f);
            modeText.rectTransform.anchorMax = new Vector2(1f, 1f);
            modeText.rectTransform.offsetMin = new Vector2(8f, -24f);
            modeText.rectTransform.offsetMax = new Vector2(-8f, -4f);

            buttonRoot = GetOrCreateButtonRoot();
            AddButton("Stop", () => router.StopSelected());
            AddButton("Move", () => router.SetMoveMode());
            AddButton("Attack", () => router.SetAttackPlaceholderMode());
            AddButton("Guard", () => router.Placeholder("Guard"));
            AddButton("Patrol", () => router.Placeholder("Patrol"));
            AddButton("Deploy", () => router.Placeholder("Deploy"));
            AddButton("Repair", () => router.Placeholder("Repair"));
            AddButton("Sell", () => router.Placeholder("Sell"));
            AddButton("Power", () => router.TogglePowerSelected());

            RemoveLegacyButton("Pause");
            RemoveLegacyButton("Step");
            RemoveLegacyButton("Low Power");
            if (showDeveloperCommands)
            {
                AddButton("Step", () => router.StepTick());
                AddButton("Low Power", () => router.TriggerLowPowerDemo());
            }
        }

        void AddButton(string text, UnityEngine.Events.UnityAction action)
        {
            if (buttonRoot == null)
                buttonRoot = GetOrCreateButtonRoot();

            var child = buttonRoot.Find(text);
            var button = child != null ? child.GetComponent<Button>() : null;
            if (button == null)
                button = RtsUiFactory.CreateButton(buttonRoot, text, text);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(112f, 30f);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        RectTransform GetOrCreateButtonRoot()
        {
            var child = transform.Find("Command Buttons");
            var rootObject = child != null ? child.gameObject : new GameObject("Command Buttons");
            if (child == null)
                rootObject.transform.SetParent(transform, false);

            var rect = rootObject.GetComponent<RectTransform>();
            if (rect == null)
                rect = rootObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(8f, 8f);
            rect.offsetMax = new Vector2(-8f, -30f);

            var grid = EnsureSingleGridLayout(rootObject);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(112f, 30f);
            grid.spacing = new Vector2(6f, 5f);
            return rect;
        }

        void RemoveLegacyButton(string text)
        {
            var child = transform.Find(text);
            if (child == null)
                return;
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        static void RemoveLayoutGroups(GameObject target)
        {
            var groups = target.GetComponents<LayoutGroup>();
            for (var i = 0; i < groups.Length; i++)
            {
                if (Application.isPlaying)
                    Destroy(groups[i]);
                else
                    DestroyImmediate(groups[i]);
            }
        }

        static GridLayoutGroup EnsureSingleGridLayout(GameObject target)
        {
            var groups = target.GetComponents<LayoutGroup>();
            GridLayoutGroup grid = null;
            for (var i = 0; i < groups.Length; i++)
            {
                var candidate = groups[i] as GridLayoutGroup;
                if (candidate != null && grid == null)
                {
                    grid = candidate;
                    continue;
                }

                if (Application.isPlaying)
                    Destroy(groups[i]);
                else
                    DestroyImmediate(groups[i]);
            }

            return grid != null ? grid : target.AddComponent<GridLayoutGroup>();
        }

        Text GetOrCreateText(string objectName, string text, int fontSize, Color color, TextAnchor anchor)
        {
            var child = transform.Find(objectName);
            var existing = child != null ? child.GetComponent<Text>() : null;
            return existing != null ? existing : RtsUiFactory.CreateText(transform, objectName, text, fontSize, color, anchor);
        }
    }
}
