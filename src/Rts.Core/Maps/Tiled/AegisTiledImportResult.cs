using System.Collections.Generic;

namespace ProjectAegisRTS.Maps.Tiled
{
    public sealed class AegisTiledImportResult
    {
        public bool Success { get; private set; }
        public AegisMapDocument Document { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; }

        AegisTiledImportResult(bool success, AegisMapDocument document, IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            Success = success;
            Document = document;
            Errors = errors ?? new string[0];
            Warnings = warnings ?? new string[0];
        }

        public static AegisTiledImportResult Ok(AegisMapDocument document, IReadOnlyList<string> warnings)
        {
            return new AegisTiledImportResult(true, document, new string[0], warnings);
        }

        public static AegisTiledImportResult Fail(IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            return new AegisTiledImportResult(false, null, errors, warnings);
        }
    }
}
