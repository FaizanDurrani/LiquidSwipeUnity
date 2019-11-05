//Criado por Rodrigo de Toni

using UnityEngine;
using UnityEditor;
using System.Collections;


public class SnapAnchorsEditor : Editor
{
    

	[MenuItem("GameObject/Snap Anchors/in this and it's children", false, 0)]
    static void SweepingSnapAnchorsStatic()
    {
        Debug.Log("Snaping anchors of ''" + Selection.activeTransform.gameObject.name + "'' and its children.");

		StaticSweepingSnapAnchors(Selection.activeGameObject);
    }

	[MenuItem("GameObject/Snap Anchors/in this", false, 0)]
	static void SnapAnchorsStatic()
	{
		Debug.Log("Snaping anchors of ''" + Selection.activeTransform.gameObject.name + ".");
		StaticSnapAnchors(Selection.activeGameObject);
	}
		

	public static void StaticSnapAnchors(GameObject o){

		RectTransform recTransform = null;
		RectTransform parentTransform = null;
	
		if (o.transform.parent != null) {
			if (o.gameObject.tag != "IgnoreSnapAnchors") {
				if (o.GetComponent<RectTransform> () != null)
					recTransform = o.GetComponent<RectTransform> ();
				else {
					Debug.LogError (o.name + " Doesn't have RectTransform. SnapAnchors must be used only with UI objects. Please select a objet with RectTransform. Returning function.");
					return;
				}

				if (parentTransform == null) {
					parentTransform = o.transform.parent.GetComponent<RectTransform> ();
				}
				Undo.RecordObject (recTransform,"Snap Anchors");

				Vector2 offsetMin = recTransform.offsetMin;
				Vector2 offsetMax = recTransform.offsetMax;

				Vector2 anchorMin = recTransform.anchorMin;
				Vector2 anchorMax = recTransform.anchorMax;

				Vector2 parent_scale = new Vector2 (parentTransform.rect.width, parentTransform.rect.height);


				recTransform.anchorMin = new Vector2 (
					anchorMin.x + (offsetMin.x / parent_scale.x),
					anchorMin.y + (offsetMin.y / parent_scale.y));

				recTransform.anchorMax = new Vector2 (
					anchorMax.x + (offsetMax.x / parent_scale.x),
					anchorMax.y + (offsetMax.y / parent_scale.y));

				recTransform.offsetMin = Vector2.zero;
				recTransform.offsetMax = Vector2.zero;
			}
		}

	}
    public static void StaticSweepingSnapAnchors(GameObject o)
    {
		StaticSnapAnchors (o);
        for (int i = 0; i < o.transform.childCount; i++)
        {
			StaticSweepingSnapAnchors(o.transform.GetChild(i).gameObject);
        }   
    }
}