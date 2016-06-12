
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class gazeReceiver : MonoBehaviour {
	
	private float randVal=0f;
	public RectTransform gaze;
	private String msg="";
	// Script initialization
	void Start() {	
		OSCHandler.Instance.Init(); //init OSC
	}

	void Update() {
		
		OSCHandler.Instance.UpdateLogs();

		Canvas c = gaze.GetComponentInParent<Canvas> ();
		ServerLog s = OSCHandler.Instance.GetServer ("LocalServer");

		if(s!=null){
			if (s.log.Count > 0) {
				for (int i = s.packets.Count - 1; i >= 0; i--) {
					if (s.packets [i].Address == "/pupil/pos") {
						int lastPacketIndex = s.packets.Count - 1;

						float x = float.Parse (s.packets [i].Data [0].ToString ());
						float y = float.Parse (s.packets [i].Data [1].ToString ());

						Debug.Log (x.ToString () + "," + y.ToString ());

						gaze.localPosition = new Vector3 ((x - 0.5f) * c.pixelRect.width, (y - 0.5f) * c.pixelRect.height, 0);
						break;
					}
				}
			}
		}
			

	}
}