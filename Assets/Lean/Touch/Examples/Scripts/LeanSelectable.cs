using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Touch
{
	/// <summary>This component makes this GameObject selectable.
	/// If your game is 3D then make sure this GameObject or a child has a Collider component.
	/// If your game is 2D then make sure this GameObject or a child has a Collider2D component.
	/// If your game is UI based then make sure this GameObject or a child has a graphic with "Raycast Target" enabled.
	/// To then select it, you can add the LeanSelect and LeanFingerTap components to your scene. You can then link up the LeanFingerTap.OnTap event to LeanSelect.SelectScreenPosition.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanSelectable")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Selectable")]
	public class LeanSelectable : MonoBehaviour
	{
		[System.Serializable] public class LeanFingerEvent : UnityEvent<LeanFinger> {}

		public static LinkedList<LeanSelectable> Instances = new LinkedList<LeanSelectable>();

		public static event System.Action<LeanSelectable, LeanFinger> OnSelectGlobal;

		public static event System.Action<LeanSelectable, LeanFinger> OnSelectSetGlobal;

		public static event System.Action<LeanSelectable, LeanFinger> OnSelectUpGlobal;

		public static event System.Action<LeanSelectable> OnDeselectGlobal;

		[Tooltip("Should this get deselected when the selecting finger goes up?")]
		public bool DeselectOnUp;

		[Tooltip("Should IsSelected temporarily return false if the selecting finger is still being held? This is useful when selecting multiple objects using a complex gesture (e.g. RTS style selection box)")]
		public bool HideWithFinger;

		[Tooltip("If the selecting fingers are still active, only return those to RequiredSelectable queries?")]
		public bool IsolateSelectingFingers;

		/// <summary>This event is called when selection begins (finger = the finger that selected this).</summary>
		public LeanFingerEvent OnSelect { get { if (onSelect == null) onSelect = new LeanFingerEvent(); return onSelect; } } [FormerlySerializedAs("OnSelect")] [SerializeField] private LeanFingerEvent onSelect;

		/// <summary>This event is called every frame this selectable is selected with a finger (finger = the finger that selected this).</summary>
		public LeanFingerEvent OnSelectSet { get { if (onSelectSet == null) onSelectSet = new LeanFingerEvent(); return onSelectSet; } } [FormerlySerializedAs("OnSelectSet")] [SerializeField] private LeanFingerEvent onSelectSet;

		/// <summary>This event is called when the selecting finger goes up (finger = the finger that selected this).</summary>
		public LeanFingerEvent OnSelectUp { get { if (onSelectUp == null) onSelectUp = new LeanFingerEvent(); return onSelectUp; } } [FormerlySerializedAs("OnSelectUp")] [SerializeField] private LeanFingerEvent onSelectUp;

		/// <summary>This event is called when this is deselected, if OnSelectUp hasn't been called yet, it will get called first.</summary>
		public UnityEvent OnDeselect { get { if (onDeselect == null) onDeselect = new UnityEvent(); return onDeselect; } } [FormerlySerializedAs("OnDeselect")] [SerializeField] private UnityEvent onDeselect;

		/// <summary>If you want to change this, do it via the Select/Deselect methods (accessible from the context menu gear icon in editor)</summary>
		[Tooltip("If you want to change this, do it via the Select/Deselect methods (accessible from the context menu gear icon in editor)")]
		[SerializeField]
		private bool isSelected;

		// The fingers that were used to select this GameObject
		// If a finger goes up then it will be removed from this list
		[System.NonSerialized]
		private List<LeanFinger> selectingFingers = new List<LeanFinger>();

		[System.NonSerialized]
		private LinkedListNode<LeanSelectable> node;

		/// <summary>Returns isSelected, or false if HideWithFinger is true and SelectingFinger is still set.</summary>
		public bool IsSelected
		{
			set
			{
				if (value == true)
				{
					Select();
				}
				else
				{
					Deselect();
				}
			}

			get
			{
				// Hide IsSelected?
				if (HideWithFinger == true && isSelected == true && selectingFingers.Count > 0)
				{
					return false;
				}

				return isSelected;
			}
		}

		/// <summary>Bypass HideWithFinger.</summary>
		public bool IsSelectedRaw
		{
			get
			{
				return isSelected;
			}
		}

		/// <summary>This tells you how many LeanSelectable objects in your scene are currently selected.</summary>
		public static int IsSelectedCount
		{
			get
			{
				var count = 0;

				foreach (var selectable in Instances)
				{
					if (selectable.IsSelected == true)
					{
						count += 1;
					}
				}

				return count;
			}
		}

		/// <summary>This tells you the first or earliest still active finger that initiated selection of this object.
		/// NOTE: If the selecting finger went up then this may return null.</summary>
		public LeanFinger SelectingFinger
		{
			get
			{
				if (selectingFingers.Count > 0)
				{
					return selectingFingers[0];
				}

				return null;
			}
		}

		/// <summary>This tells you every currently active finger that selected this object.</summary>
		public List<LeanFinger> SelectingFingers
		{
			get
			{
				return selectingFingers;
			}
		}

		/// <summary>If requiredSelectable is set and not selected, the fingers list will be empty. If selected then the fingers list will only contain the selecting finger.</summary>
		public static List<LeanFinger> GetFingers(bool ignoreIfStartedOverGui, bool ignoreIfOverGui, int requiredFingerCount = 0, LeanSelectable requiredSelectable = null)
		{
			var fingers = LeanTouch.GetFingers(ignoreIfStartedOverGui, ignoreIfOverGui, requiredFingerCount);

			if (requiredSelectable != null)
			{
				if (requiredSelectable.IsSelected == false)
				{
					fingers.Clear();
				}

				if (requiredSelectable.IsolateSelectingFingers == true)
				{
					fingers.Clear();

					fingers.AddRange(requiredSelectable.selectingFingers);

					if (requiredFingerCount > 0 && fingers.Count != requiredFingerCount)
					{
						fingers.Clear();
					}
				}
			}

			return fingers;
		}

		private static List<LeanSelectable> tempSelectables = new List<LeanSelectable>();

		/// <summary>This allows you to fill a list of all selected instances.</summary>
		public static void GetSelected(List<LeanSelectable> list)
		{
			if (list != null)
			{
				list.Clear();

				foreach (var selectable in Instances)
				{
					if (selectable.IsSelected == true)
					{
						list.Add(selectable);
					}
				}
			}
		}

		/// <summary>This allows you to limit how many objects can be selected in your scene.</summary>
		public static void Cull(int maxCount)
		{
			GetSelected(tempSelectables);

			for (var i = maxCount; i < tempSelectables.Count; i++)
			{
				var selectable = tempSelectables[i];

				if (selectable != null) // If one selectable deselection triggers destruction of another, it may cause it to be null
				{
					selectable.Deselect();
				}
			}
		}

		/// <summary>If the specified finger selected an object, this will return the first one.</summary>
		public static LeanSelectable FindSelectable(LeanFinger finger)
		{
			foreach (var selectable in Instances)
			{
				if (selectable.IsSelectedBy(finger) == true)
				{
					return selectable;
				}
			}

			return null;
		}

		/// <summary>This allows you to replace the currently selected objects with the ones in the specified list. This is useful if you're doing box selection or switching selection groups.</summary>
		public static void ReplaceSelection(LeanFinger finger, List<LeanSelectable> selectables)
		{
			var selectableCount = 0;

			// Deselect missing selectables
			if (selectables != null)
			{
				tempSelectables.Clear();

				foreach (var selectable in Instances)
				{
					if (selectable.isSelected == true && selectables.Contains(selectable) == false)
					{
						tempSelectables.Add(selectable);
					}
				}

				foreach (var selectable in tempSelectables)
				{
					if (selectable != null) // If one selectable deselection triggers destruction of another, it may cause it to be null
					{
						selectable.Deselect();
					}
				}
			}

			// Add new selectables
			if (selectables != null)
			{
				for (var i = selectables.Count - 1; i >= 0; i--)
				{
					var selectable = selectables[i];

					if (selectable != null)
					{
						if (selectable.isSelected == false)
						{
							selectable.Select(finger);
						}

						selectableCount += 1;
					}
				}
			}

			// Nothing was selected?
			if (selectableCount == 0)
			{
				DeselectAll();
			}
		}

		/// <summary>This tells you if the current selectable was selected by the specified finger.</summary>
		public bool IsSelectedBy(LeanFinger finger)
		{
			for (var i = selectingFingers.Count - 1; i >= 0; i--)
			{
				if (selectingFingers[i] == finger)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>This tells you the IsSelected or IsSelectedRaw value.</summary>
		public bool GetIsSelected(bool raw)
		{
			return raw == true ? IsSelectedRaw : IsSelected;
		}

		/// <summary>This selects the current object.</summary>
		[ContextMenu("Select")]
		public void Select()
		{
			Select(null);
		}

		/// <summary>This selects the current object with the specified finger.</summary>
		public void Select(LeanFinger finger)
		{
			isSelected = true;

			if (finger != null)
			{
				if (IsSelectedBy(finger) == false)
				{
					selectingFingers.Add(finger);
				}
			}

			if (onSelect != null)
			{
				onSelect.Invoke(finger);
			}

			if (OnSelectGlobal != null)
			{
				OnSelectGlobal(this, finger);
			}
		}

		/// <summary>This deselects the current object.</summary>
		[ContextMenu("Deselect")]
		public void Deselect()
		{
			// Make sure we don't deselect multiple times
			if (isSelected == true)
			{
				isSelected = false;

				for (var i = selectingFingers.Count - 1; i >= 0; i--)
				{
					var selectingFinger = selectingFingers[i];

					if (selectingFinger != null)
					{
						if (onSelectUp != null)
						{
							onSelectUp.Invoke(selectingFinger);
						}

						if (OnSelectUpGlobal != null)
						{
							OnSelectUpGlobal(this, selectingFinger);
						}
					}
				}

				selectingFingers.Clear();

				if (onDeselect != null)
				{
					onDeselect.Invoke();
				}

				if (OnDeselectGlobal != null)
				{
					OnDeselectGlobal(this);
				}
			}
		}

		/// <summary>This deselects all objects in the scene.</summary>
		public static void DeselectAll()
		{
			GetSelected(tempSelectables);

			foreach (var selectable in tempSelectables)
			{
				selectable.Deselect();
			}
		}

		protected virtual void OnEnable()
		{
			Instances.AddLast(this);

			if (Instances.Count == 1)
			{
				LeanTouch.OnFingerSet      += HandleFingerSet;
				LeanTouch.OnFingerUp       += HandleFingerUp;
				LeanTouch.OnFingerInactive += HandleFingerInactive;
			}
		}

		protected virtual void OnDisable()
		{
			if (Instances.Count == 1)
			{
				LeanTouch.OnFingerSet      -= HandleFingerSet;
				LeanTouch.OnFingerUp       -= HandleFingerUp;
				LeanTouch.OnFingerInactive -= HandleFingerInactive;
			}

			Instances.Remove(this);

			if (isSelected == true)
			{
				Deselect();
			}
		}

		private static void BuildTempSelectables(LeanFinger finger)
		{
			tempSelectables.Clear();

			foreach (var selectable in Instances)
			{
				if (selectable.IsSelectedBy(finger) == true)
				{
					tempSelectables.Add(selectable);
				}
			}
		}

		private static void HandleFingerSet(LeanFinger finger)
		{
			BuildTempSelectables(finger);

			foreach (var selectable in tempSelectables)
			{
				if (selectable != null) // If one selectable deselection triggers destruction of another, it may cause it to be null
				{
					if (selectable.onSelectSet != null)
					{
						selectable.onSelectSet.Invoke(finger);
					}

					if (OnSelectSetGlobal != null)
					{
						OnSelectSetGlobal(selectable, finger);
					}
				}
			}
		}

		private bool AnyFingersSet
		{
			get
			{
				for (var i = selectingFingers.Count - 1; i >= 0; i--)
				{
					if (selectingFingers[i].Set == true)
					{
						return true;
					}
				}

				return false;
			}
		}

		private static void HandleFingerUp(LeanFinger finger)
		{
			BuildTempSelectables(finger);

			foreach (var selectable in tempSelectables)
			{
				if (selectable != null) // If one selectable deselection triggers destruction of another, it may cause it to be null
				{
					if (selectable.DeselectOnUp == true && selectable.IsSelected == true && selectable.AnyFingersSet == false)
					{
						selectable.Deselect();
					}
					// Deselection will call onSelectUp
					else
					{
						if (selectable.onSelectUp != null)
						{
							selectable.onSelectUp.Invoke(finger);
						}

						if (OnSelectUpGlobal != null)
						{
							OnSelectUpGlobal(selectable, finger);
						}
					}
				}
			}
		}

		private static void HandleFingerInactive(LeanFinger finger)
		{
			foreach (var selectable in Instances)
			{
				selectable.selectingFingers.Remove(finger);
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanSelectable))]
	public class LeanSelectable_Inspector : LeanInspector<LeanSelectable>
	{
		private bool showUnusedEvents;

		// Draw the whole inspector
		protected override void DrawInspector()
		{
			// isSelected modified?
			if (Draw("isSelected") == true)
			{
				// Grab the new value
				var isSelected = serializedObject.FindProperty("isSelected").boolValue;

				// Apply it directly to each instance before the SerializedObject applies it when this method returns
				Each(t => t.IsSelected = isSelected);
			}
			Draw("DeselectOnUp");
			Draw("HideWithFinger");
			Draw("IsolateSelectingFingers");

			EditorGUILayout.Separator();

			var usedA = Any(t => t.OnSelect.GetPersistentEventCount() > 0);
			var usedB = Any(t => t.OnSelectSet.GetPersistentEventCount() > 0);
			var usedC = Any(t => t.OnSelectUp.GetPersistentEventCount() > 0);
			var usedD = Any(t => t.OnDeselect.GetPersistentEventCount() > 0);

			showUnusedEvents = EditorGUILayout.Foldout(showUnusedEvents, "Show Unused Events");

			EditorGUILayout.Separator();

			if (usedA == true || showUnusedEvents == true)
			{
				Draw("onSelect");
			}

			if (usedB == true || showUnusedEvents == true)
			{
				Draw("onSelectSet");
			}

			if (usedC == true || showUnusedEvents == true)
			{
				Draw("onSelectUp");
			}

			if (usedD == true || showUnusedEvents == true)
			{
				Draw("onDeselect");
			}
		}
	}
}
#endif