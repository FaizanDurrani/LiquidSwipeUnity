using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to spawn a prefab at the specified world point.
	/// NOTE: To trigger the prefab spawn you must call the Spawn method on this component from somewhere.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanSpawn")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Spawn")]
	public class LeanSpawn : MonoBehaviour
	{
		/// <summary>The prefab that this component can spawn.</summary>
		[Tooltip("The prefab that this component can spawn.")]
		public Transform Prefab;

		/// <summary>This will spawn Prefab at the specified finger based on the ScreenDepth setting.</summary>
		public void Spawn(Vector3 position)
		{
			if (Prefab != null)
			{
				var clone = Instantiate(Prefab);

				clone.position = position;

				clone.gameObject.SetActive(true);
			}
		}
	}
}