using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art
{
    public sealed class ConceptArtCardView : MonoBehaviour
    {
        public ConceptArtReference conceptReference;
        public ActorVisualDefinition visualDefinition;
        public SpriteRenderer spriteRenderer;
        public TextMesh labelMesh;
        public bool visible = true;

        public void Configure(ConceptArtReference concept, ActorVisualDefinition definition, bool show)
        {
            conceptReference = concept;
            visualDefinition = definition;
            visible = show;
            EnsureChildren();
            Apply();
        }

        public void SetVisible(bool show)
        {
            visible = show;
            Apply();
        }

        void EnsureChildren()
        {
            if (spriteRenderer == null)
            {
                var child = transform.Find("Concept Sprite");
                if (child == null)
                {
                    var obj = new GameObject("Concept Sprite");
                    obj.transform.SetParent(transform, false);
                    obj.transform.localPosition = new Vector3(0f, 1.2f, -0.65f);
                    obj.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
                    obj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    spriteRenderer = obj.AddComponent<SpriteRenderer>();
                }
                else
                {
                    spriteRenderer = child.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null)
                        spriteRenderer = child.gameObject.AddComponent<SpriteRenderer>();
                }
            }

            if (labelMesh == null)
            {
                var child = transform.Find("Concept Label");
                if (child == null)
                {
                    var obj = new GameObject("Concept Label");
                    obj.transform.SetParent(transform, false);
                    obj.transform.localPosition = new Vector3(0f, 0.05f, -1.05f);
                    obj.transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
                    labelMesh = obj.AddComponent<TextMesh>();
                }
                else
                {
                    labelMesh = child.GetComponent<TextMesh>();
                    if (labelMesh == null)
                        labelMesh = child.gameObject.AddComponent<TextMesh>();
                }
            }
        }

        void Apply()
        {
            EnsureChildren();
            var sprite = conceptReference == null ? null : conceptReference.unitySprite;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.gameObject.SetActive(visible && sprite != null);
            }

            if (labelMesh != null)
            {
                var nameText = visualDefinition == null ? (conceptReference == null ? "Missing concept" : conceptReference.safeDisplayName) : visualDefinition.safeDisplayName;
                var idText = visualDefinition == null ? (conceptReference == null ? string.Empty : conceptReference.actorTypeId) : visualDefinition.actorTypeId;
                var categoryText = visualDefinition == null ? string.Empty : visualDefinition.category.ToString();
                var statusText = visualDefinition == null ? string.Empty : visualDefinition.productionStatus.ToString();
                var ipText = visualDefinition != null && visualDefinition.ipReviewRequired ? " / IP review" : string.Empty;
                labelMesh.text = nameText + "\n" + idText + "\n" + categoryText + " / " + statusText + ipText;
                labelMesh.fontSize = 34;
                labelMesh.characterSize = 0.045f;
                labelMesh.anchor = TextAnchor.UpperCenter;
                labelMesh.alignment = TextAlignment.Center;
                labelMesh.gameObject.SetActive(visible);
            }
        }
    }
}
