using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	public ParticleSystem mouseClickNotifierPrefab;
	public Text unitUITextPrefab;
	[HideInInspector] public ParticleSystem mouseClickNotifier;
	public List<GameObject> selectableUnits;
	public List<GameObject> selectableBuildings;
	public List<GameObject> selectedUnits;
	public List<GameObject> selectedBuildings;
	public List<Text> unitUITexts;
	public GameObject selectedObjectsGroup;
	public Texture marqueeGraphics;
	public Color playerColor = new Color(0.4320488f,0.6886792f,0.5438234f,1);
    private Vector2 marqueeOrigin;
    private Vector2 marqueeSize;
    private Rect marqueeRect;
	private Rect backupRect;
	private enum selectionModes {none, units, buildings};
	private int selectionMode = (int)selectionModes.none; 
	private static List<string> terrains = new List<string>() { "Floor" };
	
	void Awake () {
		if(instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
		mouseClickNotifier = Instantiate(mouseClickNotifierPrefab,new Vector3(0,0,0), Quaternion.Euler(new Vector3(-90,0,0)));
		selectedObjectsGroup = GameObject.Find("SelectedObjectsGroup");
	}
	
	private void OnGUI() {
        GUI.color = new Color(0, 0, 0, .3f);
        marqueeRect = new Rect(marqueeOrigin.x, marqueeOrigin.y, marqueeSize.x, marqueeSize.y);
        GUI.DrawTexture(marqueeRect, marqueeGraphics);
    }

	void Update () {
		GetInput();
	}

	void GetInput() {

		/* Q */
		/* temporary measure to circumvent button not working */
		if(Input.GetKeyDown("q")) {
			if(selectionMode == (int)selectionModes.buildings && selectedBuildings.Count > 0) {
				foreach(GameObject building in selectedBuildings) {
					building.SendMessage("CreateNewSwordsman", SendMessageOptions.DontRequireReceiver);
				}
			} 
		}

		/* LEFT MOUSE CLICK */
		if(Input.GetMouseButtonDown(0)) {
			float _invertedY = Screen.height - Input.mousePosition.y;
			marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);
		}

		/* LEFT MOUSE HELD DOWN */
		if(Input.GetMouseButton(0)) {
			//draw marquee
			float _invertedY = Screen.height - Input.mousePosition.y;
            marqueeSize = new Vector2(Input.mousePosition.x - marqueeOrigin.x, (marqueeOrigin.y - _invertedY) * -1);
            //FIX FOR RECT.CONTAINS NOT ACCEPTING NEGATIVE VALUES
            if (marqueeRect.width < 0) {
                backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y, Mathf.Abs(marqueeRect.width), marqueeRect.height);
            } else if (marqueeRect.height < 0) {
                backupRect = new Rect(marqueeRect.x, marqueeRect.y - Mathf.Abs(marqueeRect.height), marqueeRect.width, Mathf.Abs(marqueeRect.height));
            }
            if (marqueeRect.width < 0 && marqueeRect.height < 0) {
                backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y - Mathf.Abs(marqueeRect.height), Mathf.Abs(marqueeRect.width), Mathf.Abs(marqueeRect.height));
            }

		}


		/* LEFT MOUSE UP */
		if(Input.GetMouseButtonUp(0)) {
			PreSelection();
			DeselectAll();
			Select();
			
			//reset marquee
			marqueeRect.width = 0;
            marqueeRect.height = 0;
            backupRect.width = 0;
            backupRect.height = 0;
            marqueeSize = Vector2.zero;

			//update selection UI
			int i = 0;
			if(selectionMode == (int)selectionModes.units) {
				foreach (GameObject selectedUnit in selectedUnits) {
					Text instance = Instantiate(unitUITextPrefab);
					instance.transform.SetParent(selectedObjectsGroup.transform,false);
					instance.text = selectedUnits[i].name;
					unitUITexts.Add(instance);
					i++;
				}
			} else if (selectionMode == (int)selectionModes.buildings) {
				foreach (GameObject selectedBuilding in selectedBuildings) {
					Text instance = Instantiate(unitUITextPrefab);
					instance.transform.SetParent(selectedObjectsGroup.transform,false);
					instance.text = selectedBuildings[i].name;
					unitUITexts.Add(instance);
					i++;
				}
			}
		}

		/* RIGHT MOUSE CLICK */
		if(Input.GetMouseButtonUp(1)) {
			if(selectionMode == (int)selectionModes.units) {
				OrderUnitsRightClick();
			} else if (selectionMode == (int)selectionModes.buildings) {
				OrderBuildingsRightClick();
			}
		}
	}

	void DeselectAll() {
		//clear current selection
		if(selectionMode == (int)selectionModes.units) {
			foreach (GameObject unit in selectedUnits) {
				unit.SendMessage("OnDeselected", SendMessageOptions.DontRequireReceiver);
			}
			selectedUnits.Clear();
		} else if (selectionMode == (int)selectionModes.buildings) {
			foreach (GameObject building in selectedBuildings) {
				building.SendMessage("OnDeselected", SendMessageOptions.DontRequireReceiver);
			}
			selectedBuildings.Clear();
		}
		foreach (Text unitText in unitUITexts) {
			Destroy(unitText.gameObject);
		}
		unitUITexts.Clear();
	}

	void Select() {
		//select all objects in marquee OR select single object

		foreach (GameObject unit in selectableUnits) {
			//Convert the world position of the unit to a screen position and then to a GUI point
			Vector3 _screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
			Vector2 _screenPoint = new Vector2(_screenPos.x, Screen.height - _screenPos.y);
			//Ensure that any units not within the marquee are currently unselected
			if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
					unit.SendMessage("OnDeselected", SendMessageOptions.DontRequireReceiver);
					selectedUnits.Remove(unit);
			}
			if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
				if (!selectedUnits.Contains(unit)) {
					unit.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
					selectedUnits.Add(unit);
				}
			}
		}

		if(selectedUnits.Count == 0) {
			//select buildings only if no unit is selected to ensure that buildings and units are never selected at the same time 
			foreach (GameObject building in selectableBuildings) {
				//Convert the world position of the unit to a screen position and then to a GUI point
				Vector3 _screenPos = Camera.main.WorldToScreenPoint(building.transform.position);
				Vector2 _screenPoint = new Vector2(_screenPos.x, Screen.height - _screenPos.y);
				//Ensure that any units not within the marquee are currently unselected
				if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
						building.SendMessage("OnDeselected", SendMessageOptions.DontRequireReceiver);
						selectedBuildings.Remove(building);
				}
				if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
					if (!selectedUnits.Contains(building)) {
						building.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
						selectedBuildings.Add(building);
					}
				}
			}
		}

		//single object selection
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			if (selectableUnits.Contains(hit.transform.gameObject) && !selectedUnits.Contains(hit.transform.gameObject)) {
				selectableUnits.Remove(hit.transform.gameObject);
				selectedUnits.Add(hit.transform.gameObject);
				hit.transform.gameObject.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
			} else if (selectableBuildings.Contains(hit.transform.gameObject) && !selectedBuildings.Contains(hit.transform.gameObject)) {
				selectableBuildings.Remove(hit.transform.gameObject);
				selectedBuildings.Add(hit.transform.gameObject);
				hit.transform.gameObject.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
			}
		}

		if(selectedUnits.Count > 0 && selectedBuildings.Count == 0) {
			selectionMode = (int)selectionModes.units;
		} else if(selectedUnits.Count == 0 && selectedBuildings.Count > 0) {
			selectionMode = (int)selectionModes.buildings;
		} else if (selectedUnits.Count == 0 && selectedBuildings.Count == 0) {
			selectionMode = (int)selectionModes.none;
		} else {
			/* TODO error handling */
		}
	}

	void PreSelection() {
		//update lists of selectable objects
		selectableUnits.Clear();
		selectableUnits.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Selectable")));
		selectableBuildings.Clear();
		selectableBuildings.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("SelectableBuilding")));
	}

	void OrderUnitsRightClick() {
		if(selectedUnits.Count != 0) {
				RaycastHit hit;
				Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(r, out hit)) {
						while (!terrains.Contains(hit.transform.gameObject.name)) {
							if (!Physics.Raycast(hit.transform.position, r.direction, out hit)) {
								break;
							}
							break;
						}
				}

				if (hit.transform != null) {
					foreach (GameObject unit in selectedUnits) {
						 unit.SendMessage("OnRightClick", hit.point, SendMessageOptions.DontRequireReceiver);
					}
					mouseClickNotifier.transform.position = hit.point;
					mouseClickNotifier.Play();
				}
			}
	}

	void OrderBuildingsRightClick() {
		if(selectedBuildings.Count != 0) {
			RaycastHit hit;
				Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(r, out hit)) {
						while (!terrains.Contains(hit.transform.gameObject.name)) {
							if (!Physics.Raycast(hit.transform.position, r.direction, out hit)) {
								break;
							}
							break;
						}
				}
				
				if (hit.transform != null) {
					foreach (GameObject building in selectedBuildings) {
						 building.SendMessage("OnRightClick", hit.point, SendMessageOptions.DontRequireReceiver);
					}
					mouseClickNotifier.transform.position = hit.point;
					mouseClickNotifier.Play();
				}
		}
	}
}
