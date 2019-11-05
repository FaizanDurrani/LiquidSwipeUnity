using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Touch
{
	/// <summary>This struct manages a list of fingers for you, so you can easily handle controls that depend on them.
	/// The fingers can be added manually with the AddFinger method, or automatically pulled from LeanTouch using the filters you specify.</summary>
	[System.Serializable]
	public struct LeanFingerFilter
	{
		public enum FilterType
		{
			AllFingers,
			ManuallyAddedFingers
		}

		/// <summary>The method used to find fingers to use with this component.
		/// ManuallyAddedFingers = You must manually call the AddFinger function (e.g. from a UI button).</summary>
		public FilterType Filter;

		/// <summary>Ignore fingers that began touching the screen on top of a GUI element?</summary>
		public bool IgnoreStartedOverGui;

		/// <summary>If the amount of fingers doesn't match this number, ignore all fingers?
		/// 0 = Any amount.</summary>
		public int RequiredFingerCount;

		/// <summary>When using simulated fingers, should a specific combination of mouse buttons be held?
		/// 0 = Any.
		/// 1 = Left.
		/// 2 = Right.
		/// 3 = Left + Right.
		/// 4 = Middle.
		/// 5 = Left + Middle.
		/// 6 = Right + Middle.
		/// 7 = Left + Right + Middle.</summary>
		public int RequiredMouseButtons;

		/// <summary>If the specified RequiredSelectable component's IsSelected setting is false, ignore all fingers?</summary>
		public LeanSelectable RequiredSelectable;

		[System.NonSerialized]
		private List<LeanFinger> fingers;

		public LeanFingerFilter(bool newIgnoreStartedOverGui) : this(default(FilterType), newIgnoreStartedOverGui, default(int), default(int), default(LeanSelectable))
		{
		}

		public LeanFingerFilter(FilterType newFilter, bool newIgnoreStartedOverGui, int newRequiredFingerCount, int newRequiredMouseButtons, LeanSelectable newRequiredSelectable)
		{
			Filter               = newFilter;
			IgnoreStartedOverGui = newIgnoreStartedOverGui;
			RequiredFingerCount  = newRequiredFingerCount;
			RequiredSelectable   = newRequiredSelectable;
			RequiredMouseButtons = newRequiredMouseButtons;

			fingers = null;
		}

		/// <summary>If the current RequiredSelectable is null, this method allows you to try and set it based on the specified GameObject.</summary>
		public void UpdateRequiredSelectable(GameObject gameObject)
		{
			if (RequiredSelectable == null && gameObject != null)
			{
				RequiredSelectable = gameObject.GetComponentInParent<LeanSelectable>();
			}
		}

		/// <summary>If you've set Filter to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
		public void AddFinger(LeanFinger finger)
		{
			if (Filter == FilterType.ManuallyAddedFingers && finger != null)
			{
				if (fingers == null)
				{
					fingers = new List<LeanFinger>();
				}

				for (var i = fingers.Count - 1; i >= 0; i--)
				{
					if (fingers[i] == finger)
					{
						return;
					}
				}

				if (fingers.Count == 0)
				{
					LeanTouch.OnFingerUp += RemoveFinger;
				}

				fingers.Add(finger);
			}
		}

		/// <summary>If you've set Filter to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
		public void RemoveFinger(LeanFinger finger)
		{
			for (var i = fingers.Count - 1; i >= 0; i--)
			{
				if (fingers[i] == finger)
				{
					fingers.RemoveAt(i);

					if (fingers.Count == 0)
					{
						LeanTouch.OnFingerUp -= RemoveFinger;
					}

					return;
				}
			}
		}

		/// <summary>If you've set Filter to ManuallyAddedFingers, then you can call this method to manually remvoe all fingers.</summary>
		public void RemoveAllFingers()
		{
			if (fingers != null)
			{
				for (var i = fingers.Count - 1; i >= 0; i--)
				{
					RemoveFinger(fingers[i]);
				}
			}
		}

		/// <summary>This method returns a list of all fingers based on the current settings.</summary>
		public List<LeanFinger> GetFingers(bool ignoreUpFingers = false)
		{
			if (fingers == null)
			{
				fingers = new List<LeanFinger>();
			}

			switch (Filter)
			{
				case FilterType.AllFingers:
				{
					fingers.Clear();

					fingers.AddRange(LeanSelectable.GetFingers(IgnoreStartedOverGui, false, RequiredFingerCount, RequiredSelectable));
				}
				break;
			}

			if (ignoreUpFingers == true)
			{
				for (var i = fingers.Count - 1; i >= 0; i--)
				{
					if (fingers[i].Up == true)
					{
						fingers.RemoveAt(i);
					}
				}
			}

			if (RequiredMouseButtons != 0)
			{
				var keep = true;

				for (var i = 0; i < 3; i++)
				{
					var mask = 1 << i;

					if ((RequiredMouseButtons & mask) != 0 && Input.GetMouseButton(i) == false)
					{
						keep = false; break;
					}
				}

				if (keep == false)
				{
					for (var i = fingers.Count - 1; i >= 0; i--)
					{
						if (fingers[i].Index < 0)
						{
							fingers.RemoveAt(i);
						}
					}
				}
			}

			return fingers;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch
{
	[CustomPropertyDrawer(typeof(LeanFingerFilter))]
	public class LeanFingerFilter_Drawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var filter = (LeanFingerFilter.FilterType)property.FindPropertyRelative("Filter").enumValueIndex;
			var height = base.GetPropertyHeight(property, label);
			var step   = height + 2;

			switch (filter)
			{
				case LeanFingerFilter.FilterType.AllFingers: height += step * 4; break;
			}

			return height;
		}

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			var filter = (LeanFingerFilter.FilterType)property.FindPropertyRelative("Filter").enumValueIndex;
			var height = base.GetPropertyHeight(property, label);

			rect.height = height;

			DrawProperty(ref rect, property, label, "Filter", label.text, "The method used to find fingers to use with this component.\n\nManuallyAddedFingers = You must manually call the AddFinger function (e.g. from a UI button).");

			EditorGUI.indentLevel++;
			{
				switch (filter)
				{
					case LeanFingerFilter.FilterType.AllFingers:
					{
						DrawProperty(ref rect, property, label, "RequiredSelectable", null, "If the specified RequiredSelectable component's IsSelected setting is false, ignore all fingers?");
						DrawProperty(ref rect, property, label, "RequiredFingerCount", null, "If the amount of fingers doesn't match this number, ignore all fingers?\n\n0 = Any amount.");
						DrawProperty(ref rect, property, label, "RequiredMouseButtons", null, "When using simulated fingers, should a specific combination of mouse buttons be held?\n\n0 = Any.\n1 = Left.\n2 = Right.\n3 = Left + Right.\n4 = Middle.\n5 = Left + Middle.\n6 = Right + Middle.\n7 = Left + Right + Middle.");
						DrawProperty(ref rect, property, label, "IgnoreStartedOverGui", null, "Ignore fingers that began touching the screen on top of a GUI element?");
					}
					break;
				}
			}
			EditorGUI.indentLevel--;
		}

		private void DrawProperty(ref Rect rect, SerializedProperty property, GUIContent label, string childName, string overrideName = null, string overrideTooltip = null)
		{
			var childProperty = property.FindPropertyRelative(childName);

			label.text = string.IsNullOrEmpty(overrideName) == false ? overrideName : childProperty.displayName;

			label.tooltip = string.IsNullOrEmpty(overrideTooltip) == false ? overrideTooltip : childProperty.tooltip;

			EditorGUI.PropertyField(rect, childProperty, label);

			rect.y += rect.height + 2;
		}
	}
}
#endif