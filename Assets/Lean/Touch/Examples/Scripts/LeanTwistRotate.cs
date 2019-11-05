using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to transform the current GameObject relative to the specified camera using a twist gesture.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanTwistRotate")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Twist Rotate")]
	public class LeanTwistRotate : MonoBehaviour
	{
		/// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
		public LeanFingerFilter Use = new LeanFingerFilter(true);

		/// <summary>The camera we will be used to calculate relative rotations.
		/// None = MainCamera.</summary>
		[Tooltip("The camera we will be used to calculate relative rotations.\n\nNone = MainCamera.")]
		public Camera Camera;

		/// <summary>Should the rotation be performanced relative to the finger center?</summary>
		[Tooltip("Should the rotation be performanced relative to the finger center?")]
		public bool Relative;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = -1.0f;

		[HideInInspector]
		[SerializeField]
		private Vector3 remainingTranslation;

		[HideInInspector]
		[SerializeField]
		private Quaternion remainingRotation = Quaternion.identity;

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
			// Store
			var oldPosition = transform.localPosition;
			var oldRotation = transform.localRotation;

			// Get the fingers we want to use
			var fingers = Use.GetFingers();

			// Calculate the rotation values based on these fingers
			var twistDegrees = LeanGesture.GetTwistDegrees(fingers);

			if (twistDegrees != 0.0f)
			{
				if (Relative == true)
				{
					var twistScreenCenter = LeanGesture.GetScreenCenter(fingers);

					if (transform is RectTransform)
					{
						TranslateUI(twistDegrees, twistScreenCenter);
						RotateUI(twistDegrees);
					}
					else
					{
						Translate(twistDegrees, twistScreenCenter);
						Rotate(twistDegrees);
					}
				}
				else
				{
					if (transform is RectTransform)
					{
						RotateUI(twistDegrees);
					}
					else
					{
						Rotate(twistDegrees);
					}
				}
			}

			// Increment
			remainingTranslation += transform.localPosition - oldPosition;
			remainingRotation    *= Quaternion.Inverse(oldRotation) * transform.localRotation;

			// Get t value
			var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

			// Dampen remainingDelta
			var newRemainingTranslation = Vector3.Lerp(remainingTranslation, Vector3.zero, factor);
			var newRemainingRotation    = Quaternion.Slerp(remainingRotation, Quaternion.identity, factor);

			// Shift this transform by the change in delta
			transform.localPosition = oldPosition + remainingTranslation - newRemainingTranslation;
			transform.localRotation = oldRotation * Quaternion.Inverse(newRemainingRotation) * remainingRotation;

			// Update remainingDelta with the dampened value
			remainingTranslation = newRemainingTranslation;
			remainingRotation    = newRemainingRotation;
		}

		protected virtual void TranslateUI(float twistDegrees, Vector2 twistScreenCenter)
		{
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			// Screen position of the transform
			var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, transform.position);

			// Twist screen point around the twistScreenCenter by twistDegrees
			var twistRotation = Quaternion.Euler(0.0f, 0.0f, twistDegrees);
			var screenDelta   = twistRotation * (screenPoint - twistScreenCenter);

			screenPoint.x = twistScreenCenter.x + screenDelta.x;
			screenPoint.y = twistScreenCenter.y + screenDelta.y;

			// Convert back to world space
			var worldPoint = default(Vector3);

			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, screenPoint, camera, out worldPoint) == true)
			{
				transform.position = worldPoint;
			}
		}

		protected virtual void Translate(float twistDegrees, Vector2 twistScreenCenter)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				// Screen position of the transform
				var screenPoint = camera.WorldToScreenPoint(transform.position);

				// Twist screen point around the twistScreenCenter by twistDegrees
				var twistRotation = Quaternion.Euler(0.0f, 0.0f, twistDegrees);
				var screenDelta   = twistRotation * ((Vector2)screenPoint - twistScreenCenter);

				screenPoint.x = twistScreenCenter.x + screenDelta.x;
				screenPoint.y = twistScreenCenter.y + screenDelta.y;

				// Convert back to world space
				transform.position = camera.ScreenToWorldPoint(screenPoint);
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}

		protected virtual void RotateUI(float twistDegrees)
		{
			transform.rotation *= Quaternion.Euler(0.0f, 0.0f, twistDegrees);
		}

		protected virtual void Rotate(float twistDegrees)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				var axis = transform.InverseTransformDirection(camera.transform.forward);

				transform.rotation *= Quaternion.AngleAxis(twistDegrees, axis);
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}
	}
}