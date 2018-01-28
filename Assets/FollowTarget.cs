using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour {

    public Transform Target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Target != null) {
            transform.position = new Vector3(Target.position.x, Target.position.y, transform.position.z);
        }
	}
}
