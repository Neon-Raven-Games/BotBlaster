#if UNITY_EDITOR
using NRTools.GpuSkinning;
using UnityEditor;
using UnityEngine;

namespace NRTools.CustomAnimator
{
    public class TransitionControlWindow : EditorWindow
    {
        [MenuItem("Development/Transition Preview")]
        public static void ShowWindow()
        {
            GetWindow<TransitionControlWindow>("Transition");
        }

        AnimationTransitionData _transitionData;
        private AnimationData _fromAnimation;
        private AnimationData _toAnimation;
        private bool _drawing;

        public void OnDestroy()
        {
            _drawing = false;
            AnimationController.OnTransitionSelected -= OnTransitionSelected;
        }

        internal void OnEnable()
        {
            AnimationController.OnTransitionSelected += OnTransitionSelected;
            _drawing = true;
        }

        private void OnTransitionSelected(AnimationTransitionData data, string from)
        {
            _transitionData = data;
            if (data == null)
            {
                Debug.LogError("Error: Transition data is null");
                return;
            }

            _fromAnimation = AnimationManager.GetAnimationData(AnimationController.currentAnimator, from);
            _toAnimation = AnimationManager.GetAnimationData(AnimationController.currentAnimator , data.toAnimation);
            _drawing = true;
            Repaint();
        }

        private void OnGUI()
        {
            DrawScrubBar();
        }

        internal void DrawScrubBar()
        {
            if (!_drawing || _transitionData == null) return;
            GUILayout.BeginHorizontal();

            var scrubRect = GUILayoutUtility.GetRect(_fromAnimation.frameCount / 24f, 20);
            EditorGUI.DrawRect(scrubRect, new Color(0.2f, 0.2f, 0.2f));

            var secondRect = GUILayoutUtility.GetRect(_toAnimation.frameCount / 24f, 20);
            EditorGUI.DrawRect(secondRect, new Color(0.5f, 0.5f, 0.5f));

            GUILayout.EndHorizontal();

            float totalAnimationDuration = _fromAnimation.frameCount / 24f;

            float targetProgressNormalized = .5f / totalAnimationDuration;
            float blendDurationNormalized = _transitionData.blendDuration / totalAnimationDuration;

            float targetProgress = Mathf.Lerp(0, scrubRect.width, targetProgressNormalized);
            float width = Mathf.Lerp(0, scrubRect.width, blendDurationNormalized);

            var progressRect = new Rect(scrubRect.x + targetProgress, scrubRect.y, width, scrubRect.height);
            EditorGUI.DrawRect(progressRect, new Color(0.54f, 0.65f, 1f));
        }
    }
}
#endif