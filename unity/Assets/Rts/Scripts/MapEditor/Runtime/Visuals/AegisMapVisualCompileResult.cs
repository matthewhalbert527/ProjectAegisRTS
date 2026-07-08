using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    public sealed class AegisMapVisualCompileResult
    {
        public GameObject Root;
        public readonly List<AegisVisualLayerSummary> Layers = new List<AegisVisualLayerSummary>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Errors = new List<string>();

        public AegisVisualLayerSummary AddLayer(string layerName)
        {
            var summary = new AegisVisualLayerSummary(layerName);
            Layers.Add(summary);
            return summary;
        }

        public void AddWarning(string warning)
        {
            if (!string.IsNullOrEmpty(warning))
                Warnings.Add(warning);
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
                Errors.Add(error);
        }

        public string ToSummaryText()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Layers.Count; i++)
                builder.AppendLine(Layers[i].ToDisplayString());

            for (var i = 0; i < Warnings.Count; i++)
                builder.AppendLine("Warning: " + Warnings[i]);

            for (var i = 0; i < Errors.Count; i++)
                builder.AppendLine("Error: " + Errors[i]);

            return builder.ToString().TrimEnd();
        }

        public int TotalScatterCount
        {
            get
            {
                var total = 0;
                for (var i = 0; i < Layers.Count; i++)
                    total += Layers[i].ScatterCount;
                return total;
            }
        }
    }
}
