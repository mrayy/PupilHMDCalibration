from zmq_tools import *
from OSC import OSCClient, OSCMessage
import time


current_milli_time = lambda: int(round(time.time() * 1000))

ctx=zmq.Context()
requester=ctx.socket(zmq.REQ)
requester.connect('tcp://localhost:50020')

requester.send('SUB_PORT')
ipc_subPort=requester.recv()

monitor=Msg_Receiver(ctx,'tcp://localhost:%s'%ipc_subPort,topics=('gaze','notify.',))

client=OSCClient()
client.connect(("192.168.1.40",9000))



print('connected')
samples=0

t=current_milli_time()
lastData=''
startedCalib=False


def processGaze(g):
	global lastData,t,samples
	conf=g['confidence']
	if conf>0.5:
		if lastData<>'' and lastData==g['norm_pos']:
			return
		lastData=g['norm_pos']
		t1=current_milli_time()
		msg=OSCMessage("/pupil/pos"+str(g['id']))
		msg.append(g['norm_pos'])
		msg.append(g['confidence'])
		try:
			client.send(msg)
		except:
			print "failed to send osc message"
		samples=samples+1
		if t1-t>1000:
			print 'FPS:',samples
			t=t1
			samples=0

def processCalib(g):
	msg=OSCMessage("/pupil/calib/data")
	msg.append(g['pos'])
	try:
		client.send(msg)
	except:
		print "failed to send osc message"

def sendOSCMessage(m):
	msg=OSCMessage(m)
	try:
		client.send(msg)
	except:
		print "failed to send osc message"


while True:
	topic,g=monitor.recv()
	if startedCalib== False and topic=='gaze':
		processGaze(g)
	if topic=='notify.calibration.start':
		startedCalib=True
		sendOSCMessage("/pupil/calib/start")
		print 'calibration started'
	if topic=='notify.calibration.done':
		startedCalib=False
		print 'calibration Done'
		sendOSCMessage("/pupil/calib/stop")
	if topic=='notify.calibration':
		processCalib(g)
