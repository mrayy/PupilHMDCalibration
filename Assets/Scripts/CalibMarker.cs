using UnityEngine;
using System.Collections;

public class CalibMarker : MonoBehaviour {

	RectTransform _transform;
	//
	Vector2[] locations=new Vector2[]{
		new Vector2(0.5f,0.5f),
		new Vector2(0,0),
		new Vector2(0,0.5f),
		new Vector2(0,1.0f),
		new Vector2(0.5f,1.0f),
		new Vector2(1.0f,1.0f),
		new Vector2(1.0f,0.5f),
		new Vector2(1.0f,0),
		new Vector2(0.5f,0)
	};

	int currLocation=0;

	// Use this for initialization
	void Start () {
		_transform = GetComponent<RectTransform> ();
	}

	void _SetLocation(float x,float y)
	{
		Canvas c = _transform.GetComponentInParent<Canvas> ();
		if (c == null)
			return;
		Vector3 pos=new Vector3 ((x-0.5f)*c.pixelRect.width,(y-0.5f)*c.pixelRect.height,0);
		Debug.Log ("Setting location:" + pos.ToString ());
		_transform.localPosition = pos;
	}
	// Update is called once per frame
	void Update () {
		ServerLog s = OSCHandler.Instance.GetServer ("LocalServer");
		if(s!=null){
			if (s.log.Count > 0) {
				for (int i = s.packets.Count - 1; i >= 0; i--) {
					if (s.packets [i].Address == "/pupil/calib") {

						float x = float.Parse (s.packets [i].Data [0].ToString ());
						float y = float.Parse (s.packets [i].Data [1].ToString ());

						_SetLocation (x, y);
						break;
					}
				}
			}
		}


	}
}
