using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour {

    public GameObject Prefab;
    public int x;
    public int y;
    public int z;
    public float span;

	// Use this for initialization
	void Start ()
    {
        for (int i = 0; i < x; i++)
            for (int j = 0; j < y; j++)
                for (int k = 0; k < z; k++)
                    Instantiate(Prefab, new Vector3(i * span, j * span, k * span), Quaternion.AngleAxis(0, Vector3.left), this.transform);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
