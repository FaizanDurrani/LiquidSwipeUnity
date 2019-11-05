using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component will pulse the transform.localScale value over time.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanPulseScale")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Pulse Scale")]
	public class LeanPulseScale : MonoBehaviour
	{
		/// <summary>The current scale multiplier.</summary>
		[Tooltip("The transform.localScale before pulsing.")]
		public Vector3 BaseScale = Vector3.one;

		/// <summary>The current scale multiplier.</summary>
		[Tooltip("The current scale multiplier.")]
		public float Size = 1.0f;

		/// <summary>The interval between each pulse in seconds.</summary>
		[Tooltip("The interval between each pulse in seconds.")]
		public float PulseInterval = 1.0f;

		/// <summary>The amount Size will be incremented each pulse.</summary>
		[Tooltip("The amount Size will be incremented each pulse.")]
		public float PulseSize = 1.0f;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = 5.0f;

		[System.NonSerialized]
		private float counter;

		protected virtual void Update()
		{
			counter += Time.deltaTime;

			if (counter >= PulseInterval)
			{
				counter %= PulseInterval;

				Size += PulseSize;
			}

			var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

			Size = Mathf.Lerp(Size, 1.0f, factor);

			transform.localScale = Vector3.Lerp(transform.localScale, BaseScale * Size, factor);
		}
	}
}