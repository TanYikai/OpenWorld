using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchWeapon : MonoBehaviour {

    PlayerController PController;

	// Use this for initialization
	void Start () {
        PController = GetComponent<PlayerController>();

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.K))
        {
            PController.SetArsenal("Cube");
        }
	}
}
