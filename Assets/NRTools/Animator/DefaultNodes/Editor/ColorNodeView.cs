using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(ColorNode))]
public class ColorNodeView : BaseNodeView
{
	public override void Enable()
	{
		var colorNode = nodeTarget as ColorNode;

		ColorField colorField = new ColorField()
		{
			value = colorNode.color
		};

		colorNode.onProcessed += () => colorField.value = colorNode.color;

		colorField.RegisterValueChangedCallback((v) => {
			owner.RegisterCompleteObjectUndo("Updated Color Value");
			colorNode.color = v.newValue;
		});

		controlsContainer.Add(colorField);
	}
}