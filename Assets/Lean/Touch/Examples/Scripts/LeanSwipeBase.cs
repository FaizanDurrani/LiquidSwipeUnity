using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Touch
{
	/// <summary>This is the base class for all swiping actions.</summary>
	public abstract class LeanSwipeBase : MonoBehaviour
	{
		public enum ModifyType
		{
			None,
			Normalize,
			Normalize4
		}

		public enum CoordinateType
		{
			ScaledPixels,
			ScreenPixels,
			ScreenPercentage
		}

		[System.Serializable] public class LeanFingerEvent : UnityEvent<LeanFinger> {}
		[System.Serializable] public class FloatEvent : UnityEvent<float> {}
		[System.Serializable] public class Vector2Event : UnityEvent<Vector2> {}
		[System.Serializable] public class Vector3Event : UnityEvent<Vector3> {}
		[System.Serializable] public class Vector3Vector3Event : UnityEvent<Vector3, Vector3> {}

		/// <summary>The required angle of the swipe in degrees.
		/// 0 = Up.
		/// 90 = Right.
		/// 180 = Down.
		/// 270 = Left.</summary>
		[Tooltip("The required angle of the swipe in degrees.\n\n0 = Up.\n\n90 = Right.\n\n180 = Down.\n\n270 = Left.")]
		[FormerlySerializedAs("Angle")]
		public float RequiredAngle;

		/// <summary>The angle of the arc in degrees that the swipe must be inside.
		/// -1 = No requirement.
		/// 90 = Quarter circle (+- 45 degrees).
		/// 180 = Semicircle (+- 90 degrees).</summary>
		[Tooltip("The angle of the arc in degrees that the swipe must be inside.\n\n-1 = No requirement.\n\n90 = Quarter circle (+- 45 degrees).\n\n180 = Semicircle (+- 90 degrees).")]
		[FormerlySerializedAs("AngleThreshold")]
		public float RequiredArc = -1.0f;

		/// <summary>Called on the first frame the conditions are met.</summary>
		public LeanFingerEvent OnFinger { get { if (onFinger == null) onFinger = new LeanFingerEvent(); return onFinger; } } [FormerlySerializedAs("onSwipe")] [FormerlySerializedAs("OnSwipe")] [SerializeField] private LeanFingerEvent onFinger;

		/// <summary>Should the swipe delta be modified before use?
		/// Normalize = The swipe delta magnitude/length will be set to 1.
		/// Normalize4 = The swipe delta will be + or - 1 on either the x or y axis.</summary>
		[Tooltip("Should the swipe delta be modified before use?\n\nNormalize = The swipe delta magnitude/length will be set to 1.\n\nNormalize4 = The swipe delta will be + or - 1 on either the x or y axis.")]
		[FormerlySerializedAs("Clamp")]
		public ModifyType Modify;

		/// <summary>The coordinate space of the OnDelta values.</summary>
		[Tooltip("The coordinate space of the OnDelta values.")]
		public CoordinateType Coordinate;

		/// <summary>The swipe delta will be multiplied by this value.</summary>
		[Tooltip("The swipe delta will be multiplied by this value.")]
		public float Multiplier = 1.0f;

		/// <summary>Called on the first frame the conditions are met.
		/// Vector2 = The scaled swipe delta.</summary>
		public Vector2Event OnDelta { get { if (onDelta == null) onDelta = new Vector2Event(); return onDelta; } } [FormerlySerializedAs("onDelta")] [FormerlySerializedAs("onSwipeDelta")] [FormerlySerializedAs("OnSwipeDelta")] [SerializeField] private Vector2Event onDelta;

		/// <summary>Called on the first frame the conditions are met.
		/// Float = The distance/magnitude/length of the swipe delta vector.</summary>
		public FloatEvent OnDistance { get { if (onDistance == null) onDistance = new FloatEvent(); return onDistance; } } [SerializeField] private FloatEvent onDistance;

		/// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
		public LeanScreenDepth ScreenDepth = new LeanScreenDepth(LeanScreenDepth.ConversionType.DepthIntercept);

		/// <summary>Called on the first frame the conditions are met.
		/// Vector3 = Start point in world space.</summary>
		public Vector3Event OnWorldFrom { get { if (onWorldFrom == null) onWorldFrom = new Vector3Event(); return onWorldFrom; } } [SerializeField] private Vector3Event onWorldFrom;

		/// <summary>Called on the first frame the conditions are met.
		/// Vector3 = End point in world space.</summary>
		public Vector3Event OnWorldTo { get { if (onWorldTo == null) onWorldTo = new Vector3Event(); return onWorldTo; } } [SerializeField] private Vector3Event onWorldTo;

		/// <summary>Called on the first frame the conditions are met.
		/// Vector3 = The vector between the start and end points in world space.</summary>
		public Vector3Event OnWorldDelta { get { if (onWorldDelta == null) onWorldDelta = new Vector3Event(); return onWorldDelta; } } [SerializeField] private Vector3Event onWorldDelta;

		/// <summary>Called on the first frame the conditions are met.
		/// Vector3 = Start point in world space.
		/// Vector3 = End point in world space.</summary>
		public Vector3Vector3Event OnWorldFromTo { get { if (onWorldFromTo == null) onWorldFromTo = new Vector3Vector3Event(); return onWorldFromTo; } } [FormerlySerializedAs("onSwipeFromTo")] [SerializeField] private Vector3Vector3Event onWorldFromTo;

		protected bool AngleIsValid(Vector2 vector)
		{
			if (RequiredArc >= 0.0f)
			{
				var angle      = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
				var angleDelta = Mathf.DeltaAngle(angle, RequiredAngle);

				if (angleDelta < RequiredArc * -0.5f || angleDelta >= RequiredArc * 0.5f)
				{
					return false;
				}
			}

			return true;
		}
		
		protected void HandleFingerSwipe(LeanFinger finger, Vector2 screenFrom, Vector2 screenTo)
		{
			var finalDelta = screenTo - screenFrom;

			if (AngleIsValid(finalDelta) == true)
			{
				if (onFinger != null)
				{
					onFinger.Invoke(finger);
				}

				switch (Coordinate)
				{
					case CoordinateType.ScaledPixels:     finalDelta *= LeanTouch.ScalingFactor; break;
					case CoordinateType.ScreenPercentage: finalDelta *= LeanTouch.ScreenFactor;  break;
				}

				switch (Modify)
				{
					case ModifyType.Normalize:
					{
						finalDelta = finalDelta.normalized;
					}
					break;

					case ModifyType.Normalize4:
					{
						if (finalDelta.x < -Mathf.Abs(finalDelta.y)) finalDelta = -Vector2.right;
						if (finalDelta.x >  Mathf.Abs(finalDelta.y)) finalDelta =  Vector2.right;
						if (finalDelta.y < -Mathf.Abs(finalDelta.x)) finalDelta = -Vector2.up;
						if (finalDelta.y >  Mathf.Abs(finalDelta.x)) finalDelta =  Vector2.up;
					}
					break;
				}

				finalDelta *= Multiplier;

				if (onDelta != null)
				{
					onDelta.Invoke(finalDelta);
				}

				if (onDistance != null)
				{
					onDistance.Invoke(finalDelta.magnitude);
				}

				var worldFrom = ScreenDepth.Convert(screenFrom, gameObject);
				var worldTo   = ScreenDepth.Convert(screenTo, gameObject);

				if (onWorldFrom != null)
				{
					onWorldFrom.Invoke(worldFrom);
				}

				if (onWorldTo != null)
				{
					onWorldTo.Invoke(worldTo);
				}

				if (onWorldDelta != null)
				{
					onWorldDelta.Invoke(worldTo - worldFrom);
				}

				if (onWorldFromTo != null)
				{
					onWorldFromTo.Invoke(worldFrom, worldTo);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch
{
	public abstract class LeanSwipeBase_Inspector<T> : LeanInspector<LeanSwipeBase>
		where T : LeanSwipeBase
	{
		private bool showUnusedEvents;

		protected override void DrawInspector()
		{
			Draw("RequiredAngle");
			Draw("RequiredArc");

			EditorGUILayout.Separator();

			var usedA = Any(t => t.OnFinger.GetPersistentEventCount() > 0);
			var usedB = Any(t => t.OnDelta.GetPersistentEventCount() > 0);
			var usedC = Any(t => t.OnDistance.GetPersistentEventCount() > 0);
			var usedD = Any(t => t.OnWorldFrom.GetPersistentEventCount() > 0);
			var usedE = Any(t => t.OnWorldTo.GetPersistentEventCount() > 0);
			var usedF = Any(t => t.OnWorldDelta.GetPersistentEventCount() > 0);
			var usedG = Any(t => t.OnWorldFromTo.GetPersistentEventCount() > 0);

			EditorGUI.BeginDisabledGroup(usedA && usedB && usedC && usedD && usedE && usedF && usedG);
				showUnusedEvents = EditorGUILayout.Foldout(showUnusedEvents, "Show Unused Events");
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Separator();

			if (usedA == true || showUnusedEvents == true)
			{
				Draw("onFinger");
			}

			if (usedB == true || usedC == true || showUnusedEvents == true)
			{
				Draw("Modify");
				Draw("Coordinate");
				Draw("Multiplier");
			}

			if (usedB == true || showUnusedEvents == true)
			{
				Draw("onDelta");
			}

			if (usedC == true || showUnusedEvents == true)
			{
				Draw("onDistance");
			}

			if (usedD == true || usedE == true || usedF == true || usedG == true || showUnusedEvents == true)
			{
				Draw("ScreenDepth");
			}

			if (usedD == true || showUnusedEvents == true)
			{
				Draw("onWorldFrom");
			}

			if (usedE == true || showUnusedEvents == true)
			{
				Draw("onWorldTo");
			}

			if (usedF == true || showUnusedEvents == true)
			{
				Draw("onWorldDelta");
			}

			if (usedG == true || showUnusedEvents == true)
			{
				Draw("onWorldFromTo");
			}
		}
	}
}
#endif