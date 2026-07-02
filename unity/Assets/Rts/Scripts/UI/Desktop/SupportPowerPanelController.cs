using System.Collections.Generic;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.UI.Desktop
{
    public sealed class SupportPowerPanelController : MonoBehaviour
    {
        sealed class Slot
        {
            public string PowerId;
            public Button Button;
            public Text Label;
            public Image Background;
        }

        readonly List<Slot> slots = new List<Slot>();
        RtsSimulationDriver driver;
        DesktopUiCommandRouter router;
        string lastKey = string.Empty;

        public void Initialize(RtsSimulationDriver simulationDriver, DesktopUiCommandRouter commandRouter)
        {
            driver = simulationDriver;
            router = commandRouter;
            BuildIfNeeded();
            RebuildSlots();
        }

        void Update()
        {
            Refresh();
        }

        void BuildIfNeeded()
        {
            RtsUiFactory.Stretch(gameObject, Vector2.zero, Vector2.zero);
            RtsUiFactory.AddPanel(gameObject, new Color(0.08f, 0.095f, 0.12f, 0.88f));

            var layout = GetComponent<HorizontalLayoutGroup>();
            if (layout == null && GetComponent<LayoutGroup>() == null)
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = 5f;
                layout.padding = new RectOffset(6, 6, 5, 5);
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = true;
                layout.childControlWidth = false;
                layout.childControlHeight = true;
            }
        }

        void RebuildSlots()
        {
            if (driver == null || driver.Rules == null)
                return;

            var key = driver.Rules.SupportPowerDefinitions.Count.ToString();
            if (key == lastKey && slots.Count > 0)
                return;

            lastKey = key;
            for (var i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            slots.Clear();

            for (var i = 0; i < driver.Rules.SupportPowerDefinitions.Count; i++)
            {
                var definition = driver.Rules.SupportPowerDefinitions[i];
                var button = RtsUiFactory.CreateButton(transform, "Support " + definition.PowerId, ShortName(definition.PowerId));
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(68f, 30f);
                var label = button.GetComponentInChildren<Text>();
                label.fontSize = 9;
                label.alignment = TextAnchor.MiddleCenter;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;

                var captured = definition.PowerId;
                button.onClick.AddListener(() =>
                {
                    if (router != null)
                        router.ActivateSupportPowerAtHoveredCell(captured);
                });

                slots.Add(new Slot
                {
                    PowerId = definition.PowerId,
                    Button = button,
                    Label = label,
                    Background = button.GetComponent<Image>()
                });
            }
        }

        void Refresh()
        {
            if (driver == null)
                return;

            if (slots.Count == 0)
                RebuildSlots();

            var player = driver.GetLocalPlayerSnapshot();
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var snapshot = FindSupportPower(player, slot.PowerId);
                if (snapshot == null)
                {
                    slot.Button.interactable = false;
                    slot.Label.text = ShortName(slot.PowerId) + "\nN/A";
                    continue;
                }

                slot.Button.interactable = snapshot.IsReady;
                slot.Label.text = ShortName(snapshot.PowerId) + "\n" + StatusText(snapshot);
                if (slot.Background != null)
                    slot.Background.color = StatusColor(snapshot);
            }
        }

        static SupportPowerSnapshot FindSupportPower(PlayerSnapshot player, string powerId)
        {
            if (player == null)
                return null;

            for (var i = 0; i < player.SupportPowers.Count; i++)
                if (player.SupportPowers[i].PowerId == powerId)
                    return player.SupportPowers[i];

            return null;
        }

        static string StatusText(SupportPowerSnapshot snapshot)
        {
            if (!snapshot.IsUnlocked)
                return "Req " + ShortType(snapshot.MissingPrerequisiteTypeId);
            if (snapshot.CooldownRemainingTicks > 0)
                return "CD " + snapshot.CooldownRemainingTicks;
            return "Ready";
        }

        static Color StatusColor(SupportPowerSnapshot snapshot)
        {
            if (snapshot.IsReady)
                return new Color(0.18f, 0.34f, 0.24f, 0.96f);
            if (!snapshot.IsUnlocked)
                return new Color(0.12f, 0.13f, 0.15f, 0.86f);
            return new Color(0.24f, 0.22f, 0.16f, 0.94f);
        }

        static string ShortName(string powerId)
        {
            if (powerId == "reveal_scan")
                return "Scan";
            if (powerId == "emergency_repair_pulse")
                return "Repair";
            if (powerId == "precision_strike")
                return "Strike";
            if (powerId == "production_boost")
                return "Boost";
            if (powerId == "power_surge")
                return "Surge";
            return ShortType(powerId);
        }

        static string ShortType(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                return "-";
            if (typeId == "comm_center")
                return "Comm";
            if (typeId == "repair_bay")
                return "Repair";
            if (typeId == "tech_center")
                return "Tech";
            if (typeId == "advanced_power_plant")
                return "Adv Pwr";
            return typeId.Replace("_", " ");
        }
    }
}
