using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class InventoryDatabaseWindow : EditorWindow{

	private bool actionInfoFoldout;
	private Vector2 listScrollPos;
	private Vector2 mainScrollPos;
	private string searchString;
	private bool searchChanged;
	private int selectedAddition;

	[NonSerialized]
	private InventoryElement editItem;
	[NonSerialized]
	private InventoryElement selectedItem;
	[NonSerialized]
	private ElementType editType;
	[NonSerialized]
	private ElementType selectedType;

	public enum ListState
	{
		DEFAULT,
		SEARCHITEMS
	}
	
	public enum EditState
	{
		EMPTY,
		EDITITEM,
		EDITTYPE,
		PLAYING
	}
	
	private EditState editState;
	private ListState listState;
	private GameObject prefab;

	[MenuItem("Database/Inventory Database %#w")]
	public static void Init () 
	{
		InventoryDatabaseWindow window = EditorWindow.GetWindow<InventoryDatabaseWindow>();
		window.Show ();
		window.autoRepaintOnSceneChange = true;
	}

	public void OnFocus()
	{
		EditorApplication.MarkSceneDirty ();
	}

	void OnGUI() 
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		ListArea();
		MainArea();
		EditorGUILayout.EndHorizontal();
		
		if(Application.isPlaying)
			editState = EditState.PLAYING;
		
		if (GUI.changed) 
		{
			EditorUtility.SetDirty (InventoryDatabase.Instance);

			if(prefab == null)
				prefab = Resources.Load ("InventoryDatabase") as GameObject;

			if(prefab != null)
				PrefabUtility.ReplacePrefab(InventoryDatabase.Instance.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
		}

		this.Repaint();
	}
	
	void ListArea() 
	{
		EditorGUILayout.BeginVertical(GUILayout.Width(250));
		EditorGUILayout.Space();
		listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos, "box", GUILayout.ExpandHeight(true));
		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		GUILayout.Label ("Search");
		searchString = EditorGUILayout.TextField (searchString);
		EditorGUILayout.EndHorizontal();
		searchChanged = EditorGUI.EndChangeCheck();
		
		if(searchString != "")
		{
			if(searchChanged)
				listState =	ListState.SEARCHITEMS;
		}
		else
		{
			if(listState == ListState.SEARCHITEMS)
			{
				listState = ListState.DEFAULT;
				editState = EditState.EMPTY;
			}
		}
		GUILayout.Space (15);
		GUIStyle gs = new GUIStyle();
		gs.fontStyle = FontStyle.BoldAndItalic;
		gs.alignment = TextAnchor.MiddleCenter;

		GUIStyle gs2 = new GUIStyle();
		gs2.fontStyle = FontStyle.BoldAndItalic;
		gs2.fontSize = 10;
		gs2.alignment = TextAnchor.MiddleCenter;

		switch(listState)
		{
		case ListState.DEFAULT:
			if(selectedType != null)
			{
				if(GUILayout.Button("Back", GUILayout.ExpandWidth(true))) 
				{
					GUI.FocusControl (null);
					selectedType = InventoryDatabase.GetElementType(selectedType.parentID);
					editState = EditState.EMPTY;
					listState = ListState.DEFAULT;
				}
			}
			if(selectedType != null)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label (selectedType.name + "/...", gs,GUILayout.ExpandWidth(true));
				GUILayout.EndHorizontal ();

				DisplayItems();
			}
			if(selectedType != null)
			{
				if(selectedType.GetSubTypes().Count > 0)
					GUILayout.Label ("Types");
			}

			DisplayItemTypes ();
			break;
		case ListState.SEARCHITEMS:
			Search ();
			break;
		}

		EditorGUILayout.EndScrollView();
		UnderList ();
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

	void MainArea() 
	{
		EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		mainScrollPos = EditorGUILayout.BeginScrollView (mainScrollPos);
		EditorGUILayout.Space();
		
		switch(editState)
		{
		case EditState.EDITITEM:
			EditElement (editItem);
			break;
		case EditState.EDITTYPE:
			EditItemType ();
			break;
		case EditState.EMPTY:
			break;
		case EditState.PLAYING:
			EditorGUILayout.HelpBox ("Cannot edit items during Play Mode", MessageType.Info);
			break;
		}
		
		EditorGUILayout.EndScrollView ();
		EditorGUILayout.EndVertical();
	}

	void Search()
	{
		if(InventoryDatabase.ElementCount > 0)
			GUILayout.Label ("Elements", EditorStyles.boldLabel);
		
		for(int i = 0; i < InventoryDatabase.ElementCount; i++)
		{
			InventoryElement item = InventoryDatabase.GetElement(i);

			if(item != null)
			{
				if(item.name.IndexOf (searchString, StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					if(GUILayout.Button (item.name))
					{
						editItem = item;
						editState = EditState.EDITITEM;
					}
				}
			}
		}
		
		if(InventoryDatabase.ElementTypeCount > 0)
			GUILayout.Label ("Types", EditorStyles.boldLabel);

		for(int i = 0; i < InventoryDatabase.ElementTypeCount; i++)
		{
			ElementType elemType = InventoryDatabase.GetElementType(i);

			if(elemType != null)
			{
				if(elemType.name != null)
				{
					if(elemType.name.IndexOf (searchString, StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						if(GUILayout.Button (elemType.name))
						{
							editType = elemType;
							editState = EditState.EDITTYPE;
						}
					}
				}
			}
		}
	}
	
	void DisplayItems()
	{
		if(selectedType != null)
		{
			//for(int i = 0; i < InventoryDatabase.ElementCount; i++)
			for(int i = 0; i < selectedType.elementIDs.Count; i++)
			{
				InventoryElement item = InventoryDatabase.GetElement(selectedType.elementIDs[i]);

				if(item != null)
				{
					if(item.type != null)
					{
						//if(selectedType.ID == item.type.ID)
						{
							EditorGUILayout.BeginHorizontal();
							if(GUILayout.Button(item.name, GUILayout.ExpandWidth(true))) 
							{
								GUI.FocusControl (null);
								editItem = item;
								editState = EditState.EDITITEM;
							}
							EditorGUILayout.EndHorizontal ();
						}
					}
				}
			}
		}
	}
	
	void DisplayItemTypes()
	{
		if(selectedType != null)
		{
			foreach(int i in selectedType.childrenIDs)
			{
				ElementType e = InventoryDatabase.GetElementType(i);

				if(e != null)
				{
					EditorGUILayout.BeginHorizontal();
					if(GUILayout.Button(e.name, GUILayout.ExpandWidth(true)))
					{
						GUI.FocusControl (null);
						editType = e;
						editState = EditState.EDITTYPE;
					}

					if(GUILayout.Button (">",  GUILayout.MaxWidth (25)))
					{
						GUI.FocusControl (null);
						selectedType = e;
						listState = ListState.DEFAULT;
						editState = EditState.EMPTY;
					}
					EditorGUILayout.EndHorizontal ();
				}
			}
		}
		else
		{
			for(int n = 0; n < InventoryDatabase.ElementTypeCount; n++)
			{
				ElementType et = InventoryDatabase.GetElementType(n);

				if(et != null)
				{
					EditorGUILayout.BeginHorizontal();
					if(et.ID > -1 && et.parentID == -1)
					{
						if(GUILayout.Button(et.name, GUILayout.ExpandWidth(true)))
						{
							GUI.FocusControl (null);
							editType = et;
							editState = EditState.EDITTYPE;
						}
						if(GUILayout.Button (">",  GUILayout.MaxWidth (25)))
						{
							GUI.FocusControl (null);
							selectedType = et;
							listState = ListState.DEFAULT;
							editState = EditState.EMPTY;
						}
					}
					EditorGUILayout.EndHorizontal ();
				}
			}
		}
	}
	
	void UnderList()
	{
		EditorGUILayout.BeginHorizontal();
		switch(listState)
		{
		case ListState.DEFAULT:
			if(GUILayout.Button ("Add Type"))
			{
				GUI.FocusControl (null);
				ElementType e = new ElementType();

				if(selectedType != null)
					e.parentID = selectedType.ID;

				InventoryDatabase.Add (e, selectedType);

				editType = e;
				editState = EditState.EDITTYPE;
			}

			if(selectedType != null)
			{
				if(GUILayout.Button ("Add Element"))
				{
					GUI.FocusControl (null);
					InventoryElement invEl = new InventoryElement();
					InventoryDatabase.Add (invEl, selectedType);

					selectedItem = invEl;
					editItem = invEl;
					editState = EditState.EDITITEM;
				}
			}
			break;
		}
		EditorGUILayout.EndHorizontal();
	}
	
	void EditItemType()
	{
		if(editType != null)
		{
			editType.name = EditorGUILayout.TextField ("Name", editType.name);
			editType.tooltipColor = EditorGUILayout.ColorField("Tooltip Color", editType.tooltipColor);

			GUILayout.Space (10);

			editType.deleteFoldout = EditorGUILayout.Foldout (editType.deleteFoldout, "Delete");
			
			if(editType.deleteFoldout)
			{
				editType.areYouSure = EditorGUILayout.Toggle ("Are you sure?", editType.areYouSure);
				if(editType.areYouSure)
				{
					editType.areYouSure2 = EditorGUILayout.Toggle ("Are you REALLY sure?", editType.areYouSure2);
					if(editType.areYouSure2)
					{
						if(GUILayout.Button ("Delete"))
						{
							GUI.FocusControl (null);
							editState = EditState.EMPTY;

							InventoryDatabase.Remove (editType);
						}
					}
				}
			}
		}
	}

	void EditElement(InventoryElement inventoryElement) 
	{
		if(inventoryElement != null)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.name = EditorGUILayout.TextField ("Name", inventoryElement.name);
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.description = EditorGUILayout.TextField ("Description", inventoryElement.description);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.gameObject = EditorGUILayout.ObjectField ("GameObject", inventoryElement.gameObject, typeof(GameObject), true) as GameObject;
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.isStackable = EditorGUILayout.Toggle ("Is Stackable", inventoryElement.isStackable);
			GUILayout.EndHorizontal ();

			if(inventoryElement.isStackable)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Space (15);
				inventoryElement.maxStack = EditorGUILayout.IntField ("Max Stack", inventoryElement.maxStack);
				GUILayout.EndHorizontal ();
			}

			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.nameColor = EditorGUILayout.ColorField ("Name Tooltip Color", inventoryElement.nameColor);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.descriptionColor = EditorGUILayout.ColorField ("Description Tooltip Color", inventoryElement.descriptionColor);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.icon = EditorGUILayout.ObjectField ("Icon", inventoryElement.icon, typeof(Texture), true) as Texture;
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			inventoryElement.windowActionFoldout = EditorGUILayout.Foldout (inventoryElement.windowActionFoldout, "Action Management");
			GUILayout.EndHorizontal ();

			if(inventoryElement.windowActionFoldout)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Space (30);
				actionInfoFoldout = EditorGUILayout.Foldout (actionInfoFoldout, "Info");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Space (30);
				if(actionInfoFoldout)
				{
					EditorGUILayout.HelpBox ("When you choose an Object for an action, make sure you choose an Object in the current scene" +
						" if you need to work with an instance of a script. Example: If you need to affect your character's health, then " +
						"select the object that has the Health script in the Scene as opposed to the Assets.", MessageType.Info);
				}
				GUILayout.EndHorizontal ();

				for(int m = 0; m < inventoryElement.actions.Count; m++)
				{
					ElementAction itemAction = inventoryElement.actions[m];
					
						if(itemAction != null)
						{
							GUILayout.BeginHorizontal ();
							GUILayout.Space (30);
							itemAction.foldout = EditorGUILayout.Foldout (itemAction.foldout, "Action " + m.ToString ());
							if(GUILayout.Button ("x"))
								inventoryElement.actions.RemoveAt (m);
							GUILayout.EndHorizontal ();

							if(itemAction.foldout)
							{
								EditorGUI.BeginChangeCheck ();
								GUILayout.BeginHorizontal ();
								GUILayout.Space (45);
								itemAction.activationObject = EditorGUILayout.ObjectField("Object", itemAction.activationObject, typeof(GameObject), true) as GameObject;
								GUILayout.EndHorizontal ();
								if(EditorGUI.EndChangeCheck ())
								{
									if(itemAction.activationObject != null)
										itemAction.activationObjectName = itemAction.activationObject.name;
									else
										itemAction.activationObjectName = "";
								}

								if(itemAction.activationObject != null)
								{
									//Get Components
									MonoBehaviour[] components = itemAction.activationObject.GetComponents<MonoBehaviour>();
									
									if(components != null)
									{
										if(components.Length > 0)
										{
											//Fix mismatching
											string[] componentNames = new string[components.Length];
											for(int k = 0; k < components.Length; k++)
											{
												if(components[k] != null)
													componentNames[k] = components[k].GetType ().ToString ();
												
												if(components[k].GetType ().ToString () == itemAction.selectedComponentName)
													itemAction.selectedComponent = k;
											}
											
											if(itemAction.selectedComponent >= components.Length)
												itemAction.selectedComponent = components.Length - 1;
											
											//Get Methods
											MethodInfo[] methods = components[itemAction.selectedComponent].GetType ().GetMethods (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
											
											//Get Fields
											FieldInfo[] fields = components[itemAction.selectedComponent].GetType ().GetFields ();
											
											GUILayout.BeginHorizontal ();
											GUILayout.Space (45);
											itemAction.selectedComponent = EditorGUILayout.Popup ("Components", itemAction.selectedComponent, componentNames);
											GUILayout.EndHorizontal ();

											if(itemAction.selectedComponent >= components.Length)
											{
												if(components.Length <= 0)
													itemAction.selectedComponent = 0;
												else
													itemAction.selectedComponent = components.Length - 1;
											}
											
											if(itemAction.selectedComponent < componentNames.Length)
												itemAction.selectedComponentName = componentNames[itemAction.selectedComponent];
											
											GUILayout.BeginHorizontal ();
											GUILayout.Space (45);
											itemAction.selectedOption = EditorGUILayout.Popup ("Modify", itemAction.selectedOption, itemAction.options);
											GUILayout.EndHorizontal ();
											
											if(itemAction.selectedOption == 0)
											{
												string[] methodNames = new string[methods.Length];
												for(int q = 0; q < methods.Length; q++)
												{
													methodNames[q] = methods[q].Name;
													
													//Fix Mismatched methods
													if(methodNames[q] == itemAction.activationMethodName)
														itemAction.selectedMethod = q;
												}
												
												GUILayout.BeginHorizontal ();
												GUILayout.Space (45);
												itemAction.selectedMethod = EditorGUILayout.Popup ("Methods", itemAction.selectedMethod, methodNames);
												GUILayout.EndHorizontal ();
												
												if(itemAction.selectedMethod >= methodNames.Length)
												{
													if(methodNames.Length <= 0)
														itemAction.selectedMethod = 0;
													else
														itemAction.selectedMethod = methods.Length - 1;
												}
												
												if(itemAction.selectedMethod < methodNames.Length)
													itemAction.activationMethodName = methodNames[itemAction.selectedMethod];
											}
											else if(itemAction.selectedOption == 1)
											{
												string[] fieldNames = new string[fields.Length];
												for(int o = 0; o < fieldNames.Length; o++)
												{
													fieldNames[o] = fields[o].Name;
													
													if(fieldNames[o] == itemAction.selectedFieldName)
														itemAction.selectedField = o;
												}
												
												if(fieldNames.Length > 0)
												{
													GUILayout.BeginHorizontal ();
													GUILayout.Space (45);
													itemAction.selectedField = EditorGUILayout.Popup ("Fields", itemAction.selectedField, fieldNames);
													GUILayout.EndHorizontal ();
													
													if(itemAction.selectedField >= fields.Length)
													{
														if(fieldNames.Length <= 0)
															itemAction.selectedField = 0;
														else
															itemAction.selectedField = fields.Length - 1;
													}
													
													if(itemAction.selectedField < fieldNames.Length)
														itemAction.selectedFieldName = fieldNames[itemAction.selectedField];

													GUILayout.BeginHorizontal ();
													GUILayout.Space (45);
													itemAction.fieldValue = EditorGUILayout.TextField ("Field +=", itemAction.fieldValue);
													GUILayout.EndHorizontal ();
													
													GUILayout.BeginHorizontal ();
													GUILayout.Space (45);
													itemAction.hasDuration = EditorGUILayout.Toggle ("Has Duration", itemAction.hasDuration);
													GUILayout.EndHorizontal ();
													
													if(itemAction.hasDuration)
													{
														GUILayout.BeginHorizontal ();
														GUILayout.Space (45);
														itemAction.durationTime = EditorGUILayout.FloatField ("Duration Time", itemAction.durationTime);
														GUILayout.EndHorizontal ();
													}
												}
												else
												{
													itemAction.selectedFieldName = null;
													itemAction.selectedField = 0;
												}
											}
										}
									}

									GUILayout.BeginHorizontal ();
									GUILayout.Space (45);
									itemAction.repeatingInvoke = EditorGUILayout.Toggle("Invoke Repeatedly", itemAction.repeatingInvoke);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (45);
									itemAction.parameterFoldout = EditorGUILayout.Foldout (itemAction.parameterFoldout, "Parameter");
									GUILayout.EndHorizontal ();
									
									if(itemAction.parameterFoldout)
									{
										GUILayout.BeginHorizontal ();
										GUILayout.Space (60);
										itemAction.sendThisItem = EditorGUILayout.Toggle ("Send This Item", itemAction.sendThisItem);
										GUILayout.EndHorizontal ();
										
										if(itemAction.sendThisItem)
										{
											string sig = itemAction.activationMethodName + "(InventoryElement element)";
											
											GUILayout.BeginHorizontal ();
											GUILayout.Space (60);
											EditorGUILayout.LabelField ("Signature", sig, EditorStyles.miniBoldLabel);
											GUILayout.EndHorizontal ();
										}
									}
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (45);
									itemAction.cooldownsFoldout = EditorGUILayout.Foldout (itemAction.cooldownsFoldout, "Cooldown");
									GUILayout.EndHorizontal ();

									if(itemAction.cooldownsFoldout)
									{
										for(int i = 0; i < itemAction.cooldownSettings.Count; i++)
										{
											CooldownSettings cs = itemAction.cooldownSettings[i];

											GUILayout.BeginHorizontal ();
											GUILayout.Space (60);
											cs.cooldownFoldout = EditorGUILayout.Foldout(cs.cooldownFoldout, "Cooldown " + i.ToString ());
											if(GUILayout.Button ("x"))
												itemAction.cooldownSettings.RemoveAt (i);
											GUILayout.EndHorizontal ();

											if(cs.cooldownFoldout)
											{
												GUILayout.BeginHorizontal ();
												GUILayout.Space (75);
												cs.selOption = EditorGUILayout.Popup ("On", cs.selOption, cs.options);
												GUILayout.EndHorizontal ();

												if(cs.options[cs.selOption] == "Type")
												{
													GUILayout.BeginHorizontal ();
													GUILayout.Space (75);
													List<string> names = new List<string>();
													names.Add ("None");
													names.Add (inventoryElement.type.name);
													if(InventoryDatabase.Instance != null)
													{
														//Get all parents of element type
														InventoryDatabase.GetAllElementTypes ().FindAll (x => x.isAncestorOf(inventoryElement.type)).ForEach (x => names.Add (x.name));
													}
													cs.selType = EditorGUILayout.Popup ("Element Type", cs.selType, names.ToArray ());
													cs.selectedType = names[cs.selType];
													GUILayout.EndHorizontal ();
												}

												GUILayout.BeginHorizontal ();
												GUILayout.Space (75);
												cs.cooldownTime = EditorGUILayout.FloatField ("Cooldown Time", cs.cooldownTime);
												GUILayout.EndHorizontal ();
												
												GUILayout.BeginHorizontal ();
												GUILayout.Space (75);
												cs.drawCooldownAnimation = EditorGUILayout.Toggle ("Draw Cooldown Animation", cs.drawCooldownAnimation);
												GUILayout.EndHorizontal ();
												
												if(cs.drawCooldownAnimation)
												{
													GUILayout.BeginHorizontal ();
													GUILayout.Space (75);
													cs.drawCooldownTimer = EditorGUILayout.Toggle ("Draw Timer", cs.drawCooldownTimer);
													GUILayout.EndHorizontal ();
												}
											}
										}

										if(GUILayout.Button ("Add Cooldown"))
											itemAction.cooldownSettings.Add (new CooldownSettings());
									} 									
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (45);
									itemAction.tooltipFoldout = EditorGUILayout.Foldout(itemAction.tooltipFoldout, "Tooltip");
									GUILayout.EndHorizontal ();
									
									if(itemAction.tooltipFoldout)
									{
										GUILayout.BeginHorizontal ();
										GUILayout.Space (60);
										itemAction.displayInTooltip = EditorGUILayout.Toggle("Custom", itemAction.displayInTooltip);
										GUILayout.EndHorizontal ();
										
										if(itemAction.displayInTooltip)
										{
											GUILayout.BeginHorizontal ();
											GUILayout.Space (60);
											itemAction.tooltipText = EditorGUILayout.TextField("Text", itemAction.tooltipText);
											GUILayout.EndHorizontal ();

											GUILayout.BeginHorizontal ();
											GUILayout.Space (60);
											itemAction.tooltipColor = EditorGUILayout.ColorField("Color", itemAction.tooltipColor);
											GUILayout.EndHorizontal ();
										}
									}
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (45);
									EditorGUILayout.LabelField("Activate On", EditorStyles.boldLabel);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.onHotkey = EditorGUILayout.Toggle("On Slot Hotkey", itemAction.onHotkey);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.useOnActivation = EditorGUILayout.Toggle("Slot Activation", itemAction.useOnActivation);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.useOnDeactivation = EditorGUILayout.Toggle("Slot De-Activation", itemAction.useOnDeactivation);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.respondToMouse0 = EditorGUILayout.Toggle("Slot Activated + Mouse 0 Click", itemAction.respondToMouse0);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.respondToMouse1 = EditorGUILayout.Toggle("Slot Activated + Mouse 1 Click", itemAction.respondToMouse1);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.activateOnEquip = EditorGUILayout.Toggle("On Equip", itemAction.activateOnEquip);
									GUILayout.EndHorizontal ();
									
									if(itemAction.selectedOption == 0)
									{
										GUILayout.BeginHorizontal ();
										GUILayout.Space (60);
										itemAction.activateOnUnEquip = EditorGUILayout.Toggle("On UnEquip", itemAction.activateOnUnEquip);
										GUILayout.EndHorizontal ();
									}
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.clickedOnByMouse1 = EditorGUILayout.Toggle("Clicked on by Mouse 1", itemAction.clickedOnByMouse1);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (45);
									EditorGUILayout.LabelField("Post-Activation", EditorStyles.boldLabel);
									GUILayout.EndHorizontal ();
									
									GUILayout.BeginHorizontal ();
									GUILayout.Space (60);
									itemAction.destroyAfterUse = EditorGUILayout.Toggle("Destroy Item", itemAction.destroyAfterUse);
									GUILayout.EndHorizontal ();
								}
								else
									itemAction.activationObjectName = "";
							}
						}
					}
				
				GUILayout.BeginHorizontal ();
				GUILayout.Space (30);
				if(GUILayout.Button ("Add Action"))
					inventoryElement.actions.Add (new ElementAction());
				GUILayout.EndHorizontal ();
			}
	
			GUIStyle gs = new GUIStyle();
			gs.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space (10);
			editItem.deleteFoldout = EditorGUILayout.Foldout (editItem.deleteFoldout, "Delete");

			if(editItem.deleteFoldout)
			{
				editItem.areYouSure = EditorGUILayout.Toggle ("Are you sure?", editItem.areYouSure);
				if(editItem.areYouSure)
				{
					editItem.areYouSure2 = EditorGUILayout.Toggle ("Are you REALLY sure?", editItem.areYouSure2);
					if(editItem.areYouSure2)
					{
						if(GUILayout.Button ("Delete"))
						{
							GUI.FocusControl (null);
							editState = EditState.EMPTY;

							if(InventoryManager.Instance != null)
							{
								for(int i = 0; i < InventoryManager.Instance.allInventoryObjects.Count; i++)
								{
									for(int m = 0; m < InventoryManager.Instance.allInventoryObjects[i].Slots.Count; m++)
									{
										if(InventoryManager.Instance.allInventoryObjects[i].Slots[m].inventoryElement.id == editItem.id)
											InventoryManager.Instance.allInventoryObjects[i].Slots[m].inventoryElement = new InventoryElement();
									}
								}
							}

							if(InventoryDatabase.Instance != null)
							{
								if(editItem.id != -1)
									InventoryDatabase.Remove (editItem);
							}
						}
					}
				}
			}
		}
	}
}