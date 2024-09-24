using System;
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

        private void OnTransitionSelected(AnimationTransitionData data)
        {
            _transitionData = data;
            if (data == null)
            {
                Debug.LogError("Error: Transition data is null");
                return;
            }

            _fromAnimation = AnimationManager.GetAnimationData("Tank", data.fromAnimation);
            _toAnimation = AnimationManager.GetAnimationData("Tank", data.toAnimation);
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

            float targetProgressNormalized = _transitionData.blendStartTime / totalAnimationDuration;
            float blendDurationNormalized = _transitionData.blendDuration / totalAnimationDuration;

            float targetProgress = Mathf.Lerp(0, scrubRect.width, targetProgressNormalized);
            float width = Mathf.Lerp(0, scrubRect.width, blendDurationNormalized);

            var progressRect = new Rect(scrubRect.x + targetProgress, scrubRect.y * _transitionData.blendWeight, width, scrubRect.height);
            EditorGUI.DrawRect(progressRect, new Color(0.54f, 0.65f, 1f));
        }

            // HandleScrubbing(scrubRect);
        //
        // private void HandleScrubbing(Rect scrubRect)
        // {
        //     var currentEvent = Event.current;
        //
        //     if (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag)
        //     {
        //         if (scrubRect.Contains(currentEvent.mousePosition))
        //         {
        //             float relativePosition = (currentEvent.mousePosition.x - scrubRect.x) / scrubRect.width;
        //             currentTime =
        //                 Mathf.Clamp(relativePosition * animationDuration, 0, animationDuration);
        //
        //             isPlaying = false;
        //
        //             currentEvent.Use();
        //             onUpdateFrame?.Invoke(currentTime);
        //         }
        //     }
        // } 
    }
}