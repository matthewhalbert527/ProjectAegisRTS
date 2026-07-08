using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    [Serializable]
    public sealed class MvpVisualQaReport
    {
        public string actorTypeId;
        public string displayName;
        public MvpVisualQaStatus overallStatus = MvpVisualQaStatus.NotRun;
        public ProductionVisualTier visualTier = ProductionVisualTier.FirstPassProxy;
        public string artistImportStatus = "Unknown";
        public string artistModelPath;
        public int meshObjectCount;
        public int materialCount;
        public int socketCount;
        public int requiredSocketCount;
        public Vector3 localBoundsCenter;
        public Vector3 localBoundsSize;
        public List<MvpVisualQaRule> rules = new List<MvpVisualQaRule>();

        public int PassCount { get; private set; }
        public int WarningCount { get; private set; }
        public int FailCount { get; private set; }

        public bool Passed
        {
            get { return overallStatus == MvpVisualQaStatus.Pass || overallStatus == MvpVisualQaStatus.Warning; }
        }

        public void AddRule(string category, MvpVisualQaStatus status, string message, string detail)
        {
            rules.Add(new MvpVisualQaRule(category, status, message, detail));
            RecalculateStatus();
        }

        public void RecalculateStatus()
        {
            PassCount = 0;
            WarningCount = 0;
            FailCount = 0;

            var hasRunRule = false;
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule == null)
                    continue;

                if (rule.status == MvpVisualQaStatus.Pass)
                {
                    PassCount++;
                    hasRunRule = true;
                }
                else if (rule.status == MvpVisualQaStatus.Warning)
                {
                    WarningCount++;
                    hasRunRule = true;
                }
                else if (rule.status == MvpVisualQaStatus.Fail)
                {
                    FailCount++;
                    hasRunRule = true;
                }
            }

            if (FailCount > 0)
                overallStatus = MvpVisualQaStatus.Fail;
            else if (WarningCount > 0)
                overallStatus = MvpVisualQaStatus.Warning;
            else
                overallStatus = hasRunRule ? MvpVisualQaStatus.Pass : MvpVisualQaStatus.NotRun;
        }
    }
}
