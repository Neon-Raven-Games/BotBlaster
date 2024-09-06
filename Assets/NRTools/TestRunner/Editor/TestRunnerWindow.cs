using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NRTools.TestRunner
{
    public class CustomTestRunner : EditorWindow
    {
        private TestRunnerTreeView _treeView;
        private Vector2 _scrollPosition;

        [MenuItem("Neon Raven/Raven Tests")]
        public static void OpenWindow()
        {
            GetWindow<CustomTestRunner>("Raven Tests").Show();
        }

        private void OnEnable()
        {
            var rect = position;
            rect.width = 800;
            rect.height = 600;
            position = rect;
            _treeView = new TestRunnerTreeView(position.width);
            var testResults = TestRunnerTreeView.GetTests();
            _treeView.SetTestResults(testResults);
        }

        private void OnGUI()
        {
            if (_treeView == null)
            {
                _treeView = new(position.width);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.ExpandWidth(true);
            if (GUILayout.Button("Run All Tests", EditorStyles.toolbarButton, GUILayout.Width(position.width * 0.25f)))
            {
                RunTests();
            }

            EditorGUILayout.EndHorizontal();
            var controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            _treeView.OnGUI(controlRect);
        }

        private void RunTests()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.Log("Entering playmode!");

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                // Enter play mode
                EditorApplication.EnterPlaymode();
            }
            else
            {
                Debug.Log("Already in play mode, running tests.");
                ExecuteTests();
            }
        }

        // this method is not being called because our state is being changed to play mode
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Debug.Log($"PlayMode State Changed: {state}");

            // Check if we've entered play mode
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Debug.Log("Entered Play Mode - Running Tests");
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                ExecuteTests();
            }
        }

        private void ExecuteTests()
        {
            _treeView.ClearItems();

            var testClasses = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name == "Assembly-CSharp") // Reference the runtime assembly
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(typeof(Test), false).Length > 0))
                .ToList();

            foreach (var testClass in testClasses)
            {
                object instance = Activator.CreateInstance(testClass);

                // Find Setup methods
                var setupMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(Setup), false).Length > 0);

                // Find Teardown methods
                var teardownMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(Teardown), false).Length > 0);

                // Find and invoke Test methods
                var testMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(Test), false).Length > 0);

                foreach (var testMethod in testMethods)
                {
                    AssertionResult result;
                    try
                    {
                        foreach (var setupMethod in setupMethods)
                        {
                            setupMethod.Invoke(instance, null);
                        }

                        result = (AssertionResult) testMethod.Invoke(instance, null);
                    }
                    catch (Exception ex)
                    {
                        result = NeonAssert.Fail($"Test Failed: {ex.Message}");
                    }

                    _treeView.AddTestResult(testClass.Name, testMethod.Name, result.Passed, result.Message);

                    // After the test, invoke all teardown methods
                    foreach (var teardownMethod in teardownMethods)
                    {
                        teardownMethod.Invoke(instance, null);
                    }
                }
            }

            _treeView.Reload();
        }
    }
}