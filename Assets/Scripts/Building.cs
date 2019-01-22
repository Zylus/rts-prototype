using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

	public bool selected = false;
	private Material buildingMaterial;
	private GameController gameController;
	private Color playerColor;

	// Use this for initialization
	void Start () {
		buildingMaterial = GetComponent<Renderer>().material;
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		playerColor = gameController.playerColor;
		buildingMaterial.color = playerColor;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected virtual void OnSelected() {
		selected = true;
		buildingMaterial.color = playerColor + (Color)new Vector4(-0.1f,-0.1f,-0-1f,0);
	}

	protected virtual void OnDeselected() {
		selected = false;
		buildingMaterial.color = playerColor;
	}

	protected virtual void OnRightClick(Vector3 destination) {

	}
}
