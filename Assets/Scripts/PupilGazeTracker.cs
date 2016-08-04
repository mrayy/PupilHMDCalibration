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
	class MovingAverage
	{
		List<float> samples=new List<float>();
		int length=5;

		public MovingAverage(int len)
		{
			length=len;
		}
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
	class EyeData
	{
		MovingAverage xavg;
		MovingAverage yavg;

		public EyeData(int len)
		{
			 xavg=new MovingAverage(len);
			 yavg=new MovingAverage(len);
		}
		public Vector2 gaze=new Vector2();
		public Vector2 AddGaze(float x,float y)
		{
			gaze.x = xavg.AddSample (x);
			gaze.y = yavg.AddSample (y);
			return gaze;
		}
	}
	EyeData leftEye;
	EyeData rightEye;

	Vector2 _eyePos;
	float confidence;

	public delegate void OnCalibrationStartedDeleg(PupilGazeTracker manager);
	public delegate void OnCalibrationDoneDeleg(PupilGazeTracker manager);
	public delegate void OnEyeGazeDeleg(PupilGazeTracker manager);
	public delegate void OnCalibDataDeleg(PupilGazeTracker manager,float x,float y);

	public event OnCalibrationStartedDeleg OnCalibrationStarted;
	public event OnCalibrationDoneDeleg OnCalibrationDone;
	public event OnEyeGazeDeleg OnEyeGaze;
	public event OnCalibDataDeleg OnCalibData;

	public string ServerIP="";

	public int SamplesCount=4;
	public float Width = 1;
	public float Height=1 ;

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

	public Vector2 EyePos
	{
		get{ return new Vector2((_eyePos.x-0.5f)*Width,(_eyePos.y-0.5f)*Height); }
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
	string _oscclientID="PupilGazeTracker";


	public PupilGazeTracker()
	{
		_Instance = this;
	}
	void Start()
	{
		if (PupilGazeTracker._Instance == null)
			PupilGazeTracker._Instance = this;
		leftEye = new EyeData (SamplesCount);
		rightEye= new EyeData (SamplesCount);
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


			_eyePos.x=(leftEye.gaze.x+rightEye.gaze.x)*0.5f;
			_eyePos.y=(leftEye.gaze.y+rightEye.gaze.y)*0.5f;

			leftEye.AddGaze (x, y);
			if (OnEyeGaze != null)
				OnEyeGaze(this);
		}

		if (packet.Address == "/pupil/pos1") {
			x = float.Parse (packet.Data [0].ToString ());
			y = float.Parse (packet.Data [1].ToString ());

			_eyePos.x=(leftEye.gaze.x+rightEye.gaze.x)*0.5f;
			_eyePos.y=(leftEye.gaze.y+rightEye.gaze.y)*0.5f;

			rightEye.AddGaze (x, y);
			if (OnEyeGaze != null)
				OnEyeGaze(this);
		}

		if (packet .Address == "/pupil/calib/start") {
			if (OnCalibrationStarted != null)
				OnCalibrationStarted (this);
		} else if (packet.Address == "/pupil/calib/stop") {
			if (OnCalibrationDone != null)
				OnCalibrationDone(this);
		}  else if (packet.Address == "/pupil/calib/data") {
			x = float.Parse (packet.Data [0].ToString ());
			y = float.Parse (packet.Data [1].ToString ());
			OnCalibData (this,x, y);
		}
	}
}
