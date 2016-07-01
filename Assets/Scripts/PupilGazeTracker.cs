using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityOSC;
using System.Net;

public class PupilGazeTracker:MonoBehaviour
{

	static PupilGazeTracker _Instance;
	public static PupilGazeTracker Instance
	{
		get{
			if (_Instance == null) {
				_Instance = new GameObject("PupilGazeTracker").AddComponent<PupilGazeTracker> ();
			}
			return _Instance;
		}
	}
	class EyeData
	{
		MovingAverage xavg=new MovingAverage();
		MovingAverage yavg=new MovingAverage();

		public Vector2 gaze=new Vector2();
		public Vector2 AddGaze(float x,float y)
		{
			gaze.x = xavg.AddSample (x);
			gaze.y = yavg.AddSample (y);
			return gaze;
		}
	}
	EyeData leftEye = new EyeData ();
	EyeData rightEye=new EyeData();

	Vector2 _eyePos;
	float confidence;

	public string ServerIP="";

	public enum GazeSource
	{
		LeftEye,
		RightEye,
		BothEyes
	}

	public Vector2 NormalizedEyePos
	{
		get{ return _eyePos; }
	}


	void Start()
	{
		if (PupilGazeTracker._Instance == null)
			PupilGazeTracker._Instance = this;
		OSCHandler.Instance.Init(); 
		OSCHandler.Instance.OnPacket+=OnPacket;
		OSCHandler.Instance.DestroyEvent+=OSCDestroyed;
		OSCHandler.Instance.CreateClient (_oscclientID, IPAddress.Parse(ServerIP), 9090);
		OSCHandler.Instance.CreateServer ("LocalServer", 9000);

		StartProcess ();

	}

	void OSCDestroyed()
	{
		StopProcess ();
	}
	void OnDestroy()
	{
		if (OSCHandler.Exists ()) {
			StopProcess ();
		}
	}

	public Vector2 LeftEyePos
	{
		get{ return leftEye.gaze; }
	}
	public Vector2 RightEyePos
	{
		get{ return rightEye.gaze; }
	}

	public Vector2 GetEyeGaze(GazeSource s)
	{
		if (s == GazeSource.RightEye)
			return RightEyePos;
		if (s == GazeSource.LeftEye)
			return LeftEyePos;
		return NormalizedEyePos;
	}
	class MovingAverage
	{
		List<float> samples=new List<float>();
		int length=5;

		public float AddSample(float v)
		{
			samples.Add (v);
			while (samples.Count > length) {
				samples.RemoveAt (0);
			}
			float s = 0;
			for (int i = 0; i < samples.Count; ++i)
				s += samples [i];

			return s / (float)samples.Count;

		}
	}
	string _oscclientID="PupilGazeTracker";
	// Script initialization
	public PupilGazeTracker() {	

	}
	public void StartProcess()
	{
		OSCHandler.Instance.SendMessageToClient (_oscclientID, "/pupil/process", "start");
	}
	public void StopProcess()
	{
		OSCHandler.Instance.SendMessageToClient (_oscclientID, "/pupil/process", "stop");
	}

	public void StartCalibration()
	{
		OSCHandler.Instance.SendMessageToClient (_oscclientID, "/pupil/calib", "start");
	}
	public void StopCalibration()
	{
		OSCHandler.Instance.SendMessageToClient (_oscclientID, "/pupil/calib", "stop");
	}
	void OnPacket(OSCServer server,OSCPacket packet)
	{

		float x,y;
		if (packet.Address == "/pupil/pos0") {
			x = float.Parse (packet.Data [0].ToString ());
			y = float.Parse (packet.Data [1].ToString ());
			leftEye.AddGaze (x, y);
		}

		if (packet.Address == "/pupil/pos1") {
			x = float.Parse (packet.Data [0].ToString ());
			y = float.Parse (packet.Data [1].ToString ());
			rightEye.AddGaze (x, y);
		}

		_eyePos.x=(leftEye.gaze.x+rightEye.gaze.x)*0.5f;
		_eyePos.y=(leftEye.gaze.y+rightEye.gaze.y)*0.5f;
	}
}
