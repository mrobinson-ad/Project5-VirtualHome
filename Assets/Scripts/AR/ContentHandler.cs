using UnityEngine;
using Vuforia;

namespace VirtualHome
{
    public class ContentHandler : VuforiaMonoBehaviour
    {
        [SerializeField] private Material transparentPreviewMaterial;
        public Product objectToPlace;
        [SerializeField] private AnchorBehaviour anchorStage;
        private GameObject previewObject;
        private bool isPlacing = false;

        public void PlaceGround(HitTestResult hit)
        {
            if (anchorStage.name != "Base Plane")
            {
                anchorStage = VuforiaBehaviour.Instance.ObserverFactory.CreateAnchorBehaviour("Base Plane", hit);
            }

            if (isPlacing)
            {
                PlaceNewObject(hit, objectToPlace);
            }
        }

        private void PlaceNewObject(HitTestResult hit, Product product)
        {
            Instantiate(product.prefab, hit.Position, hit.Rotation, anchorStage.transform);
            isPlacing = false;
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
        }

        public void ChangeObject(Product objectToSet)
        {
            objectToPlace = objectToSet;
            CreatePreviewObject();
        }

        public void CancelPlacing()
        {
            previewObject.SetActive(false);
            isPlacing = false;
        }

        private void CreatePreviewObject()
        {
            if (previewObject != null)
            {
                Destroy(previewObject);
            }

            if (objectToPlace != null)
            {
                previewObject = Instantiate(objectToPlace.prefab);
                SetTransparentMaterial(previewObject);
                previewObject.SetActive(true);
                isPlacing = true;
            }
        }

        private void SetTransparentMaterial(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {

                renderer.material = transparentPreviewMaterial;
            }
        }

        public void UpdatePreviewPosition(HitTestResult hit)
        {
            if (isPlacing && previewObject != null)
            {
                previewObject.transform.position = hit.Position;
                previewObject.transform.rotation = hit.Rotation;
            }
        }
    }
}