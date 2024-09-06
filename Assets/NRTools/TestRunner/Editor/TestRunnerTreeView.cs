using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NRTools.TestRunner
{
    public class TestRunnerTreeView : TreeView
    {
        private List<TestResult> _testResults = new();
        private List<TestTreeViewItem> _testItems = new();
        private static Dictionary<string, TestResult> _cachedTestResults = new();

        public TestRunnerTreeView(float width) : base(new TreeViewState(), CreateHeader(width))
        {
            Reload();
        }

        private static MultiColumnHeader CreateHeader(float windowWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                    {headerContent = new GUIContent("Tests"), width = windowWidth * 0.35f},
                new MultiColumnHeaderState.Column {headerContent = new GUIContent("Pass"), width = windowWidth * 0.1f},
                new MultiColumnHeaderState.Column
                    {headerContent = new GUIContent("Status"), width = windowWidth * 0.55f}
            };
            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        public void ClearItems()
        {
            foreach (var classItem in _testItems)
            {
                if (classItem.IsClassLevel && classItem.children != null)
                {
                    // For each child (test method), reset the result to "Untested"
                    foreach (var methodItem in classItem.children)
                    {
                        var testMethod = methodItem as TestTreeViewItem;
                        if (testMethod != null && testMethod.Result != null)
                        {
                            // Reset the result to "Untested"
                            testMethod.Result.Passed = false;
                            testMethod.Result.Message = "Untested";
                        }
                    }
                }
            }
            Reload();
        }
        
        public void AddTestResult(string className, string methodName, bool passed, string message)
        {
            var result = new TestResult(className, methodName, passed, message);

            // Find the class item in _testItems, or create it if it doesn't exist
            var classItem = _testItems.FirstOrDefault(item => item.displayName == className);

            if (classItem == null)
            {
                classItem = new TestTreeViewItem(_testItems.Count + 1, 0, className, null); // Depth 0 for the class
                _testItems.Add(classItem);
            }
            else
            {
                if (classItem.children != null)
                {
                    var method = classItem.children.Where(x => x.displayName == methodName).FirstOrDefault();
                    if (method != null) classItem.children.Remove(method);
                }
            }

            // Add the test method as a child of the class
            var methodItem =
                new TestTreeViewItem(_testItems.Count + 1, 1, methodName, result); // Depth 1 for the method
            classItem.children.Add(methodItem);

            // _testResults.Add(result);
        }

        private const string UNTESTED_STRING = "Untested";

        public static List<TestTreeViewItem> GetTests()
        {
            var testCaseItems = new List<TestTreeViewItem>();
            int id = 1;

            // Get all test classes
            var testClasses = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name == "Assembly-CSharp") // Reference the runtime assembly
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(typeof(Test), false).Length > 0))
                .ToList();

            // Iterate through each class that has test methods
            foreach (var testClass in testClasses)
            {
                // Create a parent class item (depth 0)
                var classItem = new TestTreeViewItem(id++, 0, testClass.Name, null);

                // Get all test methods in the class
                var testMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes(typeof(Test), false).Length > 0)
                    .ToList();

                // Add each test method as a child of the class
                foreach (var testMethod in testMethods)
                {
                    string methodName = testMethod.Name;
                    bool testPassed = false;
                    string resultMessage = UNTESTED_STRING;

                    var testCase = new TestResult(testClass.Name, methodName, testPassed, resultMessage);
                    var methodItem = new TestTreeViewItem(id++, 1, methodName, testCase);

                    if (classItem.children == null)
                        classItem.children = new List<TreeViewItem>();

                    classItem.children.Add(methodItem); // Add the method as a child of the class
                }

                // Add the class item to the root list (it contains children)
                testCaseItems.Add(classItem);
            }

            // If no tests were found, add a placeholder
            if (testCaseItems.Count == 0)
            {
                var placeholder = new TestResult("No Tests Found", "Add test cases to see results.", false,
                    "Please add test cases to your project.");
                testCaseItems.Add(new TestTreeViewItem(id++, 0, "No Tests Found", placeholder));
            }

            return testCaseItems;
        }


        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem {depth = -1, id = 0};

            if (_testItems == null || !_testItems.Any())
            {
                // Add a default placeholder if no test items are available
                var noTestsItem = new TreeViewItem {id = 1, depth = 0, displayName = "No Tests Were Found."};
                root.AddChild(noTestsItem);
            }
            else
            {
                // Add all class items (which have method children) to the root
                foreach (var item in _testItems)
                {
                    root.AddChild(item);
                }
            }

            return root;
        }

        public void SetTestResults(List<TestTreeViewItem> testItems)
        {
            _testItems = testItems;
            Reload();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as TestTreeViewItem;

            // If item is null or not a TestTreeViewItem, fallback to the default behavior
            if (item == null)
            {
                base.RowGUI(args);
                return;
            }

            if (item.depth == 0)
            {
                float classIndent = 16f;
                // Check if the item is a class-level item and determine its overall status
                bool allPassed = item.AllTestsPassed();
                bool anyFailed = item.AnyTestFailed();


                for (int i = 0; i < args.GetNumVisibleColumns(); i++)
                {
                    var cellRect = args.GetCellRect(i);
                    var columnIndex = args.GetColumn(i);
                    cellRect.x += classIndent;
                    cellRect.width -= classIndent;

                    if (columnIndex == 1) // Display the pass/fail icon for the class
                    {
                        var classIcon = EditorGUIUtility.IconContent("console.infoicon.sml"); // Default neutral icon

                        if (anyFailed)
                        {
                            classIcon = EditorGUIUtility.IconContent("TestFailed"); // Class failed
                        }
                        else if (allPassed)
                        {
                            classIcon = EditorGUIUtility.IconContent("TestPassed"); // Class passed
                        }

                        GUI.Label(cellRect, classIcon);
                    }
                    else
                    {
                        EditorGUI.LabelField(cellRect, item.displayName);
                    }
                }

                return; // Class-level item done, no further indentation needed
            }

            float indent = 32f * item.depth;
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                var cellRect = args.GetCellRect(i);
                var columnIndex = args.GetColumn(i);
                cellRect.x += indent;
                cellRect.width -= indent;
                switch (columnIndex)
                {
                    case 0: // Test name
                        EditorGUI.LabelField(cellRect, item.displayName);
                        break;

                    case 1: // Pass/Fail/Unknown icon
                        if (item.Result == null || item.Result.Message == UNTESTED_STRING)
                        {
                            var unknownIcon =
                                EditorGUIUtility.IconContent("console.infoicon.sml"); // Example of neutral icon
                            GUI.Label(cellRect, unknownIcon);
                        }
                        else
                        {
                            // Pass/Fail icons
                            var passIcon = item.Result.Passed
                                ? EditorGUIUtility.IconContent("TestPassed") // Assuming this icon exists
                                : EditorGUIUtility.IconContent("TestFailed"); // Assuming this icon exists
                            GUI.Label(cellRect, passIcon);
                        }

                        break;

                    case 2: // Status message
                        if (item.Result == null)
                        {
                            EditorGUI.LabelField(cellRect, "No test results found.");
                        }
                        else
                        {
                            EditorGUI.LabelField(cellRect, item.Result.Message);
                        }

                        break;
                }
            }
        }


        public class TestResult
        {
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public bool Passed { get; set; }
            public string Message { get; set; }

            public TestResult(string className, string methodName, bool passed, string message)
            {
                ClassName = className;
                MethodName = methodName;
                Passed = passed;
                Message = message;
            }

            public TestResult()
            {
            }
        }

        public class TestTreeViewItem : TreeViewItem
        {
            public TestResult Result { get; }

            public TestTreeViewItem(int id, int depth, string displayName, TestResult result)
                : base(id, depth, displayName)
            {
                Result = result;
            }

            public override bool hasChildren
            {
                get { return children != null && children.Count > 0; }
            }

            public bool IsClassLevel => children != null && children.Count > 0;

            public bool AnyTestFailed()
            {
                if (!IsClassLevel) return false;

                foreach (var child in children)
                {
                    var testItem = child as TestTreeViewItem;
                    if (testItem != null && !testItem.Result.Passed && testItem.Result.Message != UNTESTED_STRING)
                    {
                        return true;
                    }
                }

                return false; // No methods failed
            }

            public bool AllTestsPassed()
            {
                if (!IsClassLevel) return false;

                foreach (var child in children)
                {
                    var testItem = child as TestTreeViewItem;
                    if (testItem != null && !testItem.Result.Passed) // If any child fails, the class fails
                    {
                        return false;
                    }
                }

                return true; // All methods passed
            }
        }
    }
}