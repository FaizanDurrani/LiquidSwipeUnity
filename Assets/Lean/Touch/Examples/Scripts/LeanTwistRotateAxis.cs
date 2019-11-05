using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to rotate the current GameObject around the specified axis using finger twists.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanTwistRotateAxis")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Twist Rotate Axis")]
	public class LeanTwistRotateAxis : MonoBehaviour
	{
		/// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
		public LeanFingerFilter Use = new LeanFingerFilter(true);

		/// <summary>The axis of rotation.</summary>
		[Tooltip("The axis of rotation.")]
		public Vector3 Axis = Vector3.down;

		/// <summary>Rotate locally or globally?</summary>
		[Tooltip("Rotate locally or globally?")]
		public Space Space = Space.Self;

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
		public void AddFinger(LeanFinger finger)
		{
			Use.AddFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
		public void RemoveFinger(LeanFinger finger)
		{
			Use.RemoveFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
		public void RemoveAllFingers()
		{
			Use.RemoveAllFingers();
		}
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}
#endif
		protected virtual void Awake()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}

		protected virtual void Update()
		{
			// Get the fingers we want to use
			var fingers = Use.GetFingers();

			// Calculate the rotation values based on these fingers
			var twistDegrees = LeanGesture.GetTwistDegrees(fingers);

			// Perform rotation
			transform.Rotate(Axis, twistDegrees, Space);
		}
	}
}