using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WallAngle : MonoBehaviour {

    TextMesh textObject;

	// Use this for initialization
	void Start () {
        textObject = GetComponentInChildren<TextMesh>();
        textObject.text = transform.rotation.eulerAngles.z.ToString();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
