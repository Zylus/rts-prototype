using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TownCenter : Building
{
    public GameObject actionButtonsGroup;
    public GameObject createSwordsmanButtonPrefab;
    public List<GameObject> actionButtons;
    public GameObject swordsmanPrefab;
	public GameObject rallyPointPrefab;
    public GameObject rallyPoint;

    public void Awake() {
        actionButtonsGroup = GameObject.Find("PossibleOptionsGroup");
    }

    protected override void OnSelected() {
        GameObject instance = Instantiate(createSwordsmanButtonPrefab);
        instance.transform.GetComponent<Button>().onClick.AddListener(CreateNewSwordsman);
		instance.transform.SetParent(actionButtonsGroup.transform,false);
        actionButtons.Add(instance);
        if(rallyPoint) {
            rallyPoint.SetActive(true);
        }
        base.OnSelected();
	}

    protected override void OnDeselected() {
        foreach (GameObject instance in actionButtons) {
			Destroy(instance);
        }
        if(rallyPoint) {
            rallyPoint.SetActive(false);
        }
        base.OnDeselected();
	}

    protected override void OnRightClick(Vector3 destination) {
        Destroy(rallyPoint);
        rallyPoint = Instantiate(rallyPointPrefab);
        rallyPoint.transform.SetParent(this.transform,false);
        rallyPoint.transform.position = destination;
    }

    public void CreateNewSwordsman() {
        GameObject instance = Instantiate(swordsmanPrefab);
        instance.transform.position = this.transform.position + new Vector3(2,0,-2);
        instance.transform.name="Swordsman";
        if(rallyPoint) {
            instance.SendMessage("OnRightClick", rallyPoint.transform.position, SendMessageOptions.DontRequireReceiver);
        }
        
    }
}
