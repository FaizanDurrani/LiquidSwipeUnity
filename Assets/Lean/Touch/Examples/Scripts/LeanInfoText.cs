using UnityEngine;
using UnityEngine.UI;

namespace Lean.Touch
{
	/// <summary>This component allows you to display formatted text from Unity events by calling one of the <b>Display</b> methods.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanInfoText")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Info Text")]
	public class LeanInfoText : MonoBehaviour
	{
		/// <summary>The text component that will be modified.</summary>
		public Text Target;

		/// <summary>The final text will use this string formatting.</summary>
		[Tooltip("The final text will use this string formatting.")]
		[Multiline]
		public string Format = "Current Value = {0}";

		public void Display(string value)
		{
			Display(value, "");
		}

		public void Display(string valueA, string valueB)
		{
			var finalText = Target;

			if (finalText == null)
			{
				finalText = GetComponentInParent<Text>();
			}

			if (finalText != null)
			{
				finalText.text = string.Format(Format, valueA, valueB);
			}
		}

		public void Display(int value)
		{
			Display(value.ToString());
		}

		public void Display(float value)
		{
			Display(value.ToString());
		}

		public void Display(Vector2 value)
		{
			Display(value.ToString());
		}

		public void Display(Vector3 value)
		{
			Display(value.ToString());
		}

		public void Display(Vector4 value)
		{
			Display(value.ToString());
		}

		public void Display(int valueA, int valueB)
		{
			Display(valueA.ToString(), valueB.ToString());
		}
	}
}