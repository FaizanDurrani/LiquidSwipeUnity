using UnityEngine;
using UnityEngine.UI;

namespace Lean.Touch
{
	/// <summary>This component rotates the current GameObject based on the current Angle value.
	/// NOTE: This component overrides and takes over the rotation of this GameObject, so you can no longer externally influence it.</summary>
	[ExecuteInEditMode]
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanRoll")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Roll")]
	public class LeanRoll : MonoBehaviour
	{
		/// <summary>The current angle in degrees.</summary>
		[Tooltip("The current angle in degrees.")]
		public float Angle;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = - 1.0f;

		[HideInInspector]
		[SerializeField]
		private float currentAngle;

		/// <summary>The <b>Angle</b> value will be incremented by the specified angle in degrees.</summary>
		public void IncrementAngle(float delta)
		{
			Angle += delta;
		}

		/// <summary>The <b>Angle</b> value will be decremented by the specified angle in degrees.</summary>
		public void DecrementAngle(float delta)
		{
			Angle -= delta;
		}

		/// <summary>This method will update the Angle value based on the specified vector.</summary>
		public void RotateToDelta(Vector2 delta)
		{
			if (delta.sqrMagnitude > 0.0f)
			{
				Angle = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
			}
		}

		protected virtual void Start()
		{
			currentAngle = Angle;
		}

		protected virtual void Update()
		{
			// Get t value
			var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

			// Lerp angle
			currentAngle = Mathf.LerpAngle(currentAngle, Angle, factor);

			// Update rotation
			transform.rotation = Quaternion.Euler(0.0f, 0.0f, -currentAngle);
		}
	}
}