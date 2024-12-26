using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Minave.Map
{
	[CustomPropertyDrawer(typeof(LayerRatio))]
	public class LayerRatioDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (GetDataObject(property) is not LayerRatio layerRatio) { return; }
			
			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			
			Rect textureRect = new Rect(position.x, position.y, position.width, position.height);

			var currentEvent = Event.current;
			if (currentEvent.type == EventType.Repaint)
			{
				var layerRatioStyle = new GUIStyle();
				layerRatioStyle.normal.background = layerRatio.GetTexture((int)position.width);
				GUI.Label(textureRect, GUIContent.none, layerRatioStyle);
			}
			else
			{
				if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
				{
					if (textureRect.Contains(currentEvent.mousePosition))
					{
						LayerRatioEditor window = EditorWindow.GetWindow<LayerRatioEditor>();
						window.SetLayerRatio(layerRatio);
					}
				}
			}

			EditorGUI.EndProperty();
		}
		
		public object GetDataObject(SerializedProperty property)
		{
			string path = property.propertyPath.Replace(".Array.data[", "[");
			object obj = property.serializedObject.targetObject;

			string[] elements = path.Split('.');
			foreach (string element in elements)
			{
				if (element.Contains("["))
				{
					string elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
					int index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
					obj = GetValue(obj, elementName, index);
				}
				else
				{
					obj = GetValue(obj, element);
				}
			}

			return obj;
		}

		public object GetValue(object source, string name)
		{
			if (source == null) { return null; }

			var type = source.GetType();
			var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (f == null)
			{
				var p = type.GetProperty(name,
						BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p == null)
					return null;
				return p.GetValue(source, null);
			}

			return f.GetValue(source);
		}

		public object GetValue(object source, string name, int index)
		{
			if (GetValue(source, name) is not IEnumerable enumerable) { return null; }
			var enumerator = enumerable.GetEnumerator();
			try
			{
				while (index-- >= 0)
				{
					enumerator.MoveNext();
				}

				return enumerator.Current;
			}
			catch
			{
				return null;
			}
		}
	}
}