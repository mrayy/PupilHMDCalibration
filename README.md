## PupilHMDCalibration

This project provides the calibration process of Pupil HMD hardware and eye gaze data streaming to Unity. 

By the date of creating this project (15-June-2016), it is designed to work with Pupil Capture branch zmq_ipc:

https://github.com/pupil-labs/pupil/tree/zmq_ipc

After installing this branch and making sure Pupil-capture is running, please follow the setup steps to get it running.

#Setup:
1- install pyOSC using pip:

pip install pyOSC

2- Copy pupil-scripts folder to pupil pc (linux/mac)

3- Modify OSCSender.py script to point to your Unity PC IP address:

>> client.connect(("192.168.11.243",9000))

Change "192.168.11.243" to your unity PC IP address.

4- Run Pupil Capture

5- Run OSCSender.py

python OSCSender.py

#Calibration:

Run Unity project, and load Calibration scene. Hit play to start receiving gaze data.

To Calibrate eye gaze, hit "C" on keyboard in Unity, a white target will appear in the HMD. 

To stop calibration, hit "S"
