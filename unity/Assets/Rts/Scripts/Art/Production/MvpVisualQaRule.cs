using System;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    [Serializable]
    public sealed class MvpVisualQaRule
    {
        public string category;
        public MvpVisualQaStatus status = MvpVisualQaStatus.NotRun;
        public string message;
        public string detail;

        public MvpVisualQaRule()
        {
        }

        public MvpVisualQaRule(string category, MvpVisualQaStatus status, string message, string detail)
        {
            this.category = category;
            this.status = status;
            this.message = message;
            this.detail = detail;
        }
    }
}
