#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIf))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var showIfAttribute = (ShowIf)attribute;

        // Get the conditional field path (even if nested)
        SerializedProperty conditionField = GetConditionalField(property, showIfAttribute.ConditionalFieldName);

        if (conditionField != null && conditionField.propertyType == SerializedPropertyType.Enum)
        {
            int enumValue = conditionField.enumValueIndex;

            // Check if the current value matches any of the compare values
            foreach (var compareValue in showIfAttribute.CompareValues)
            {
                if (enumValue == (int)compareValue)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return; // Show the field if the condition is met
                }
            }
        }
        else
        {
            Debug.LogWarning($"Field '{showIfAttribute.ConditionalFieldName}' not found or not an enum.");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var showIfAttribute = (ShowIf)attribute;
        SerializedProperty conditionField = GetConditionalField(property, showIfAttribute.ConditionalFieldName);

        if (conditionField != null && conditionField.propertyType == SerializedPropertyType.Enum)
        {
            int enumValue = conditionField.enumValueIndex;

            // Check if the current value matches any of the compare values
            foreach (var compareValue in showIfAttribute.CompareValues)
            {
                if (enumValue == (int)compareValue)
                {
                    return EditorGUI.GetPropertyHeight(property, label);
                }
            }

            return 0f; // Hide the field if no match is found
        }

        return EditorGUI.GetPropertyHeight(property, label);
    }

    // Helper method to get the correct field, even if it's nested
    private SerializedProperty GetConditionalField(SerializedProperty property, string fieldName)
    {
        string path = property.propertyPath.Replace(property.name, fieldName);
        return property.serializedObject.FindProperty(path);
    }
}
#endif
