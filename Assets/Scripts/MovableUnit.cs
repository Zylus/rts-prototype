using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovableUnit : MonoBehaviour {

	public int moveSpeed;
	public Vector3 destination;
	public static Vector3[] path = new Vector3[0];
	public bool selected = false;
	private LineRenderer lr;
	private NavMeshAgent navMeshAgent;
	private Material unitMaterial;
	private GameController gameController;
	private Color playerColor;
	

	// Use this for initialization
	protected virtual void Start () {
		navMeshAgent = GetComponent<NavMeshAgent>();
		navMeshAgent.updateRotation = false;
		navMeshAgent.speed = moveSpeed;
		destination = gameObject.transform.position;
		lr = GetComponent<LineRenderer>();
		unitMaterial = GetComponent<Renderer>().material;
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		playerColor = gameController.playerColor;
		unitMaterial.color = playerColor;
	}
	
	// Update is called once per frame
	void Update () {
		path = navMeshAgent.path.corners;
		if (path != null && path.Length > 1)
			{
				lr.positionCount = path.Length;
				for (int i = 0; i < path.Length; i++)
				{
					lr.SetPosition(i, path[i]);
				}
			}
	}

	// void OnMouseOver() {
	// 	if(!selected)
	// 		unitMaterial.color = Color.red;
	// }

	// void OnMouseExit() {
	// 	if(!selected)
	// 		unitMaterial.color = Color.black;
	// }

	void OnSelected() {
		selected = true;
		unitMaterial.color = playerColor + (Color)new Vector4(-0.1f,-0.1f,-0-1f,0);
	}

	void OnDeselected() {
		selected = false;
		unitMaterial.color = playerColor;
	}

	void OnRightClick(Vector3 destination) {
		if (navMeshAgent != null) {
            navMeshAgent.SetDestination(destination);
			path = navMeshAgent.path.corners;
        }
		else {
			Start();
			OnRightClick(destination);
		}
	}

}
