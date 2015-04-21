using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LootableObject))]
public class LootableObjectEditor : Editor {

	List<string> itemNames = new List<string>();

	public override void OnInspectorGUI()
	{
		LootableObject lo = target as LootableObject;

		itemNames.Clear ();
		itemNames.Add ("None");
		for(int i = 0; i < InventoryDatabase.ElementCount; i++)
		{
			InventoryElement element = InventoryDatabase.GetElement (i);

			if(element != null)
			{
				if(element.name != "")
					itemNames.Add (element.name);
			}
		}

		EditorGUI.BeginChangeCheck ();
		lo.selectedElement = EditorGUILayout.Popup ("Item", lo.selectedElement, itemNames.ToArray ());
		if(EditorGUI.EndChangeCheck ())
		{
			if(itemNames[lo.selectedElement] != "None")
				lo.elementID = InventoryDatabase.FindElement (itemNames[lo.selectedElement]).id;
			else
				lo.elementID = -1;
		}

		EditorGUILayout.PropertyField (serializedObject.FindProperty("stack"));
	}
}