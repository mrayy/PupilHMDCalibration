## PupilHMDCalibration

#Setup:
To use this project, copy pupil-scripts folder to pupil pc (linux/mac), and install pyOSC using pip:
pip install pyOSC

You need to set your Unity PC IP address in oscsender.py file, todo so, modify the following line:
>> client.connect(("192.168.11.243",9000))
Change "192.168.11.243" to your unity PC IP address.

While pupil capture is running, run oscsender.py to pass the required gaze and calibration data from pupil pc to unity pc.


#Calibration:

Run Unity project, and load Calibration scene. Hit play to start receiving gaze data.

To calibrate, run hmd_calibration_client.py in pupil pc, and follow calibration process in unity.
