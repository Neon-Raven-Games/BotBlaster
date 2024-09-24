using System.Collections.Generic;
using NRTools.CustomAnimator;
using NRTools.GpuSkinning;
using UnityEngine;
using UnityEditor;


/*
 *  todo, this is transition timeline, we need to make a new component for it lol
 *             float startTimeProgress = transition.blendStartTime / AnimationPreviewWindow.GetDuration();
            float startX = Mathf.Lerp(timelineRect.x, timelineRect.xMax, startTimeProgress);
            float endTimeProgress = (transition.blendStartTime + transition.blendDuration) /
                                    AnimationPreviewWindow.GetDuration();
            float endX = Mathf.Lerp(timelineRect.x, timelineRect.xMax, endTimeProgress);
            Rect startRect = new Rect(startX - 5, timelineRect.y, endX - startX + 10, 10);
            EditorGUI.DrawRect(startRect, new Color(0.5f, 1f, 1f));
 */

public class TimelineEditorWindow : EditorWindow
{
    [MenuItem("Development/Timeline Editor")]
    public static void ShowWindow()
    {
        GetWindow<TimelineEditorWindow>("Timeline Editor");
    }

    private AnimationPreviewWindow AnimationPreviewWindow;
    private float smoothProgress;

    private void OnEnable()
    {
        AnimationPreviewWindow = GetWindow<AnimationPreviewWindow>();
    }

    private void OnGUI()
    {
        if (eventTimes.Count == 0)
        {
            eventTimes.Add(1.2f);
            eventTimes.Add(1.8f);
            eventTimes.Add(4.2f);
        }

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        NREditorStyle.InitializeStyles();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawTimeline();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        DrawScrubBar();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        DrawEventArea();
        EditorGUILayout.EndHorizontal();
        HandleRepaint();
        EditorGUILayout.EndVertical();
    }

    private void HandleRepaint()
    {
        if (Event.current.type == EventType.Repaint)
            Repaint();
    }

    private void DrawTimeline()
    {
        Rect timelineRect = GUILayoutUtility.GetRect(200, 100,
            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(timelineRect, new Color(0.15f, 0.15f, 0.15f));

        int markerCount = 10;

        for (int i = 0; i <= markerCount; i++)
        {
            float x = Mathf.Lerp(timelineRect.x, timelineRect.xMax, i / (float) markerCount);
            EditorGUI.DrawRect(new Rect(x, timelineRect.y, 1, timelineRect.height), Color.gray);
            EditorGUI.LabelField(new Rect(x + 3f, timelineRect.y - 1, 50, 20),
                (i * AnimationPreviewWindow.GetDuration() / markerCount).ToString("F1"));
        }

        foreach (var time in eventTimes)
        {
            float eventProgress = time / AnimationPreviewWindow.GetDuration();
            float x = Mathf.Lerp(timelineRect.x, timelineRect.xMax, eventProgress);
            Rect markerRect = new Rect(x - 5, timelineRect.y, 10, 10);
            EditorGUI.DrawRect(markerRect, new Color(0.5f, 1f, 1f));
        }

        float targetProgress = AnimationPreviewWindow.currentTime / AnimationPreviewWindow.GetDuration();
        smoothProgress = Mathf.Lerp(smoothProgress, targetProgress, Time.deltaTime * 10f);
        smoothProgress = Mathf.Clamp01(smoothProgress);
        EditorGUI.DrawRect(
            new Rect(timelineRect.x + timelineRect.width * smoothProgress, timelineRect.y, 2, timelineRect.height),
            Color.white);
        HandleScrubbing(timelineRect);
    }

    private List<float> eventTimes = new();

    private void DrawEventArea()
    {
        Rect eventRect = GUILayoutUtility.GetRect(200, 30, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(eventRect, new Color(0.25f, 0.25f, 0.25f));

        // Draw existing events
        foreach (var time in eventTimes)
        {
            float eventProgress = time / AnimationPreviewWindow.GetDuration();
            float x = Mathf.Lerp(eventRect.x, eventRect.xMax, eventProgress);
            Rect markerRect = new Rect(x - 5, eventRect.y, 10, eventRect.height);
            EditorGUI.DrawRect(markerRect, new Color(1f, 0.5f, 0f));
        }

        HandleEventAdding(eventRect);
    }

    private void HandleEventAdding(Rect eventRect)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 1 && eventRect.Contains(e.mousePosition))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Event"), false, () => AddEventAtPosition(e.mousePosition.x, eventRect));
            menu.ShowAsContext();
            e.Use();
        }
    }

    private void AddEventAtPosition(float mouseX, Rect eventRect)
    {
        float eventProgress = (mouseX - eventRect.x) / eventRect.width;
        float eventTime = eventProgress * AnimationPreviewWindow.GetDuration();
        eventTimes.Add(eventTime);
    }

    private void DrawScrubBar()
    {
        Rect scrubRect = GUILayoutUtility.GetRect(200, 30, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(scrubRect, new Color(0.2f, 0.2f, 0.2f));

        float targetProgress = AnimationPreviewWindow.currentTime / AnimationPreviewWindow.GetDuration();
        smoothProgress = Mathf.Lerp(smoothProgress, targetProgress, Time.deltaTime * 10f);
        smoothProgress = Mathf.Clamp01(smoothProgress);

        Rect progressRect = new Rect(scrubRect.x, scrubRect.y, scrubRect.width * smoothProgress, scrubRect.height);
        EditorGUI.DrawRect(progressRect, new Color(0.5f, 0.5f, 0.5f));

        float handleX = scrubRect.x + scrubRect.width * smoothProgress - 5;
        Rect handleRect = new Rect(handleX, scrubRect.y, 10, scrubRect.height);
        EditorGUI.DrawRect(handleRect, new Color(0.8f, 0.8f, 0.8f));

        HandleScrubbing(scrubRect);
    }

    private void HandleScrubbing(Rect scrubRect)
    {
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag)
        {
            if (scrubRect.Contains(currentEvent.mousePosition))
            {
                float relativePosition = (currentEvent.mousePosition.x - scrubRect.x) / scrubRect.width;
                AnimationPreviewWindow.currentTime = Mathf.Clamp(relativePosition *
                                                                 AnimationPreviewWindow.GetDuration(), 0,
                    AnimationPreviewWindow.GetDuration());
                currentEvent.Use();
            }
        }
    }
}