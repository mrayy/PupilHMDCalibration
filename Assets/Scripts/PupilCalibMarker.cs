using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityOSC;

public class PupilCalibMarker : MonoBehaviour {

	RectTransform _transform;
	Image _image;
	bool _started=false;
	float x,y;

	// Use this for initialization
	void Start () {
		OSCHandler.Instance.Init(); 
		OSCHandler.Instance.OnPacket+=OnPacket;

		_transform = GetComponent<RectTransform> ();
		_image = GetComponent<Image> ();
		_image.enabled = false;
	}

	void OnPacket(OSCServer server,OSCPacket packet)
	{
		if (packet .Address == "/pupil/calib/start") {
			_started = true;
		} else if (packet.Address == "/pupil/calib/stop") {
			_started = false;
		} else if (packet.Address == "/pupil/calib/data") {

			x = float.Parse (packet.Data [0].ToString ());
			y = float.Parse (packet.Data [1].ToString ());

		}
	}

	void _SetLocation(float x,float y)
	{
		Canvas c = _transform.GetComponentInParent<Canvas> ();
		if (c == null)
			return;
		Vector3 pos=new Vector3 ((x-0.5f)*c.pixelRect.width,(y-0.5f)*c.pixelRect.height,0);
		_transform.localPosition = pos;
	}
	// Update is called once per frame
	void Update () {
		_image.enabled = _started;
		if(_started)
			_SetLocation (x, y);
	}
}
