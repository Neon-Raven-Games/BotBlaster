﻿using System;
using UnityEditor;
using UnityEngine;

// todo for streamlining the editor
// play bar and animation preview window do not need to know about the time
// lets make the timer function on a manager class that we can use for runtime animations
// as well as editor animations
// this will allow us to have a single point of control for the time

// the animation duration and the animation time will be the centralized point for the scrub bar and timeline
// this will allow us to have a single point of control for the time

// the main window will construct the graph, timeline and preview together, and runtime will just construct the graph
// all the timeline and preview functionality should be accessible via the graph.
// our graph needs to be able to run during runtime, so that needs to be validated on the component level

// our mesh animator

namespace NRTools.CustomAnimator
{
    public class PreviewPlayBar
    {
        private bool isPlaying = false;

        private float currentTime
        {
            get => AnimationPreviewWindow.currentTime;
            set => AnimationPreviewWindow.currentTime = value;
        }

        private float animationDuration =>
            animationPreviewWindow.GetDuration(); // Example total duration of an animation

        private float playSpeed = 1f;

        public Action<float> onUpdateFrame;
        private double lastEditorTime;

        private AnimationPreviewWindow animationPreviewWindow;
        private float smoothProgress = 0f;

        public PreviewPlayBar(AnimationPreviewWindow animationPreviewWindow)
        {
            this.animationPreviewWindow = animationPreviewWindow;
            EditorApplication.update += OnEditorUpdate;
            lastEditorTime = EditorApplication.timeSinceStartup;
        }
        public void SetPlaying(bool playing)
        {
            lastEditorTime = EditorApplication.timeSinceStartup;
            isPlaying = playing;
        }

        public void Disable()
        {
            onUpdateFrame = null;
            EditorApplication.update -= OnEditorUpdate;
        }

        public void Enable()
        {
            EditorApplication.update += OnEditorUpdate;
            lastEditorTime = EditorApplication.timeSinceStartup; 
        }

        public void Draw()
        {
            try
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true),
                    GUILayout.Height(30));
            }
            catch
            {
                return;
            }

            if (GUILayout.Button(isPlaying
                    ? EditorGUIUtility.IconContent("PauseButton")
                    : EditorGUIUtility.IconContent("PlayButton"), EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                isPlaying = !isPlaying;
            }

            DrawScrubBar();

            playSpeed = GUILayout.HorizontalSlider(playSpeed, 0.1f, 2f, GUILayout.ExpandWidth(true));
            GUILayout.Label($"{playSpeed:F2}x", GUILayout.Width(50));

            GUILayout.EndHorizontal();
        }

        private void OnEditorUpdate()
        {
            if (isPlaying)
            {
                UpdateFrame();
                animationPreviewWindow.Repaint();
            }
        }

        private void DrawScrubBar()
        {
            var scrubRect = GUILayoutUtility.GetRect(200, 20, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(scrubRect, new Color(0.2f, 0.2f, 0.2f));

            var targetProgress = currentTime / animationDuration;
            smoothProgress = Mathf.Lerp(smoothProgress, targetProgress, Time.smoothDeltaTime * 10);
            smoothProgress = Mathf.Clamp01(smoothProgress);

            var progressRect = new Rect(scrubRect.x, scrubRect.y, scrubRect.width * smoothProgress, scrubRect.height);
            EditorGUI.DrawRect(progressRect, new Color(0.5f, 0.5f, 0.5f));
            HandleScrubbing(scrubRect);
        }


        private void HandleScrubbing(Rect scrubRect)
        {
            var currentEvent = Event.current;

            if (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag)
            {
                if (scrubRect.Contains(currentEvent.mousePosition))
                {
                    float relativePosition = (currentEvent.mousePosition.x - scrubRect.x) / scrubRect.width;
                    currentTime =
                        Mathf.Clamp(relativePosition * animationDuration, 0, animationDuration);

                    isPlaying = false;

                    currentEvent.Use();
                    onUpdateFrame?.Invoke(currentTime);
                }
            }
        }

        private void UpdateFrame()
        {
            var editorTime = EditorApplication.timeSinceStartup;
            var deltaTime = editorTime - lastEditorTime;
            lastEditorTime = editorTime;
            currentTime = (float) (deltaTime * playSpeed);
            onUpdateFrame?.Invoke(currentTime);
        }
    }
}