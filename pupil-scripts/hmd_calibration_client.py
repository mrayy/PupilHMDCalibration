'''
HMD calibration client example.
This script shows how to talk to Pupil Capture or Pupil Service
and run a gaze mapper calibration.
'''
import zmq, msgpack, time
from zmq_tools import *


ctx = zmq.Context()

#create a zmq REQ socket to talk to Pupil Service/Capture
req = ctx.socket(zmq.REQ)
req.connect('tcp://localhost:50020')

should_stop=False
req.send('PUB_PORT')
publisher=Msg_Dispatcher(ctx,'tcp://localhost:%s'%req.recv())
time.sleep(1)

def init_calibration(c,r):
	global ctx,req


#convenience functions
def send_recv_notification(n):
    # REQ REP requirese lock step communication with multipart msg (topic,json_encoded dict)
    req.send_multipart(('notify.%s'%n['subject'], msgpack.dumps(n)))
    return req.recv()

def get_pupil_timestamp():
    req.send('t') #see Pupil Remote Plugin for details
    return float(req.recv())

def start_calibration():
	global should_stop
	should_stop=False

	# set calibration method to hmd calibration
	n = {'subject':'eye_process.should_start.0','eye_id':0, 'args':{}}
	print send_recv_notification(n)
	# set calibration method to hmd calibration
	n = {'subject':'eye_process.should_start.1','eye_id':1, 'args':{}}
	print send_recv_notification(n)
	time.sleep(2)

	# set calibration method to hmd calibration
	n = {'subject':'start_plugin','name':'HMD_Calibration', 'args':{}}
	print send_recv_notification(n)

	# start caliration routine with params. This will make pupil start sampeling pupil data.
	n = {'subject':'calibration.should_start', 'hmd_video_frame_size':(1000,1000), 'outlier_threshold':35}
	print send_recv_notification(n)

def cancel_calibration():
	global should_stop
	should_stop=True

def calibration_procedure():
	global should_stop
	notification={'subject':'nextReference'}
	topic=notification['subject']
	payload=msgpack.dumps(notification)

	ref_data = []

	calib_points=((0.5,0.5),(0,0),(0,0.5),(0,1),(0.5,1),(1,1),(1,0.5),(1,0),(.5,0))

	#calib_points=((0.5,0.5),(0,0),(0,1),(1,1),(1,0))

	for pos in calib_points:
	    if should_stop==True:
		break
	    print 'subject now looks at position:',pos
	    publisher.notify({'subject':'calibration','pos':pos})
	    time.sleep(1/25.)#wait until user fixtate his eye to the target
	    for s in range(120):
		if should_stop==True:
			break
		# get the current pupil time (pupil uses CLOCK_MONOTONIC with adjustable timebase).
		# You can set the pupil timebase to another clock and use that.
		t = get_pupil_timestamp()

		# in this mockup  the left and right screen marker positions are identical.
		datum0 = {'norm_pos':pos,'timestamp':t,'id':0}
		datum1 = {'norm_pos':pos,'timestamp':t,'id':1}
		ref_data.append(datum0)
		ref_data.append(datum1)
		time.sleep(1/60.) #simulate animation speed.

	# Send ref data to Pupil Capture/Service:
	# This notification can be sent once at the end or multiple times.
	# During one calibraiton all new data will be appended.
	if should_stop==False:
		n = {'subject':'calibration.add_ref_data','ref_data':ref_data}
		print send_recv_notification(n)

	# stop calibration
	# Pupil will correlate pupil and ref data based on timestamps,
	# compute the gaze mapping params, and start a new gaze mapper.
	n = {'subject':'calibration.should_stop'}
	print send_recv_notification(n)

	time.sleep(2)
	# set calibration method to hmd calibration
	n = {'subject':'service_process.should_stop'}
	print send_recv_notification(n)


