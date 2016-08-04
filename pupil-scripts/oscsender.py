from zmq_tools import *
from OSC import OSCClient, OSCMessage,OSCServer
import time,threading
import types
from hmd_calibration_client import *

current_milli_time = lambda: int(round(time.time() * 1000))

#global variables
samples=0
t=current_milli_time()
lastData=''
startedCalib=False
shouldStartCalib=False
isDone=False

ctx=zmq.Context()
req=ctx.socket(zmq.REQ)
req.connect('tcp://localhost:50020')


req.send('SUB_PORT')
ipc_subPort=req.recv()

monitor=Msg_Receiver(ctx,'tcp://localhost:%s'%ipc_subPort,topics=('gaze','notify.',))

#create a client to send data to unity
client=OSCClient()
client.connect(("192.168.1.50",9000))

#create receiver from Unity, assign local address
server=OSCServer(("192.168.1.62",9090))
server.timeout=0

#init_calibration(ctx,req)

#calibration commands handler from unity
def calib_callback(path,tags,args,source):
	global shouldStartCalib
	if(args[0]=="start"):
		start_calibration()
		sendOSCMessage("/pupil/calib/start")
		print 'calibration started'
		shouldStartCalib=True

	if(args[0]=="stop"):
		shouldStartCalib=False
		cancel_calibration()
		sendOSCMessage("/pupil/calib/stop")
		print 'calibration stopped'

def process_callback(path,tags,args,source):
	global shouldStartCalib
	if(args[0]=="start"):
		startEyes()
		print 'Eyes started'

	if(args[0]=="stop"):
		closeEyes()
		print 'Eyes stopped'


def osctimeout(self):
	self.timed_out=True

server.addDefaultHandlers()
server.addMsgHandler("/pupil/calib",calib_callback)
server.addMsgHandler("/pupil/process",process_callback)

#start receiver
st=threading.Thread(target=server.serve_forever)
st.start()

def calibThread():
	global shouldStartCalib
	while isDone==False:
		if shouldStartCalib:
			calibration_procedure()
			shouldStartCalib=False
		

ct=threading.Thread(target=calibThread)
ct.start()	


def processGaze(g):
	global lastData,t,samples
	conf=g['confidence']
	if conf>0.5:
		if lastData<>'' and lastData==g['norm_pos']:
			return
		lastData=g['norm_pos']
		t1=current_milli_time()
		msg=OSCMessage("/pupil/pos"+str(g['base'][0]['id']))
		msg.append(g['norm_pos'])
		msg.append(g['confidence'])
		try:
			client.send(msg)
		except Exception, e:
			print "failed to send osc message: "+str(e)
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
	except Exception, e:
		print "failed to send osc message: "+str(e)

def sendOSCMessage(m):
	msg=OSCMessage(m)
	try:
		client.send(msg)
	except Exception, e:
		print "failed to send osc message: "+str(e)


def startHandling():
	global startedCalib
	while True:
		topic,g=monitor.recv()
		if topic=='gaze' and startedCalib==False:
			processGaze(g)
		if topic=='notify.calibration.should_start':
			startedCalib=True
			sendOSCMessage("/pupil/calib/start")
		if topic=='notify.calibration.should_stop':
			startedCalib=False
			sendOSCMessage("/pupil/calib/stop")
		if topic=='notify.calibration':
			processCalib(g)

try:
	print "started"
	startHandling()
except Exception, e:
	print "error occured: "+str(e)
finally:
	if shouldStartCalib==True:
		shouldStartCalib=False
		cancel_calibration()
	isDone=True
	server.close()
	st.join()
	ct.join()
	closeEyes()
	print "Done"
