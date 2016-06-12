using UnityEngine;
using System.Collections;
using System.Net;

public class OSCLocal : MonoBehaviour {

	// Use this for initialization
	void Start () {

		OSCHandler.Instance.CreateServer ("LocalServer", 9000);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
