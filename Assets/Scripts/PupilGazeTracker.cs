using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityOSC;

public class PupilGazeTracker {

	static PupilGazeTracker _Instance;
	public static PupilGazeTracker Instance
	{
		get{
			if (_Instance == null) {
				_Instance = new PupilGazeTracker ();
				OSCHandler.Instance.CreateServer ("LocalServer", 9000);
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

	// Script initialization
	public PupilGazeTracker() {	
		OSCHandler.Instance.Init(); 
		OSCHandler.Instance.OnPacket+=OnPacket;
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
