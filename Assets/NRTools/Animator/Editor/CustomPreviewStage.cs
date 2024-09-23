using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NRTools.CustomAnimator
{
    public class CustomPreviewStage : PreviewSceneStage
    {
        private Camera previewCamera;
        private GameObject previewObject;
        private RenderTexture previewTexture;

        protected override GUIContent CreateHeaderContent()
        {
            var headerIcon = EditorGUIUtility.IconContent("d_UnityEditor.AnimationWindow"); // Example Unity icon
            return new GUIContent("Animation Preview", headerIcon.image, "Preview and blend animations here");
        }

        // Optionally override this method to handle the scene culling mask for the preview camera
        public ulong GetCombinedSceneCullingMaskForCamera()
        {
            return base.GetCombinedSceneCullingMaskForCamera();
        }

        public Camera GetPreviewCamera()
        {
            return previewCamera;
        }

        // Called when the custom stage is opened
        protected override bool OnOpenStage()
        {
            // Create a camera for the preview
            var previewCameraObject = new GameObject("Preview Camera");
            previewCamera = previewCameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.gray;
            previewCamera.cullingMask = 1 << LayerMask.NameToLayer("Preview");
            previewCamera.targetTexture = previewTexture;

            // Move camera into this preview scene
            SceneManager.MoveGameObjectToScene(previewCameraObject, scene);

            // Optionally, add lights to the preview scene
            var lightGameObject = new GameObject("Preview Light");
            var light = lightGameObject.AddComponent<Light>();
            light.type = LightType.Directional;
            SceneManager.MoveGameObjectToScene(lightGameObject, scene);

            // Add a 3D object to preview (e.g., a Cube)
            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.layer = LayerMask.NameToLayer("Preview");
            SceneManager.MoveGameObjectToScene(previewObject, scene);

            // Position the camera relative to the object
            previewCamera.transform.position = previewObject.transform.position + new Vector3(0, 1, -5);
            previewCamera.transform.LookAt(previewObject.transform.position);

            scene = EditorSceneManager.NewPreviewScene();
            return true;
        }

        // Called when the custom stage is closed
        protected override void OnCloseStage()
        {
            // Clean up objects when the stage is closed
            if (previewCamera != null)
            {
                DestroyImmediate(previewCamera.gameObject);
            }

            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
            }
        }
    }
}