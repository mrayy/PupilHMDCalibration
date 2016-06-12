'''
HMD calibration client example.
This script shows how to talk to Pupil Capture or Pupil Service
and run a gaze mapper calibration.
'''
import zmq, json, time
ctx = zmq.Context()
class Msg_Dispatcher(object):
    '''
    Send messages on a pub port.
    Not threadsave. Make a new one for each thread
    __init__ will block until connection is established.
    '''
    def __init__(self,ctx,url,block_until_topic_subscribed='notify'):
        self.socket = zmq.Socket(ctx,zmq.PUB)
        self.socket.connect(url)

        if block_until_topic_subscribed:
            xpub = zmq.Socket(ctx,zmq.XPUB)
            xpub.connect(url)
            while True:
                if block_until_topic_subscribed in xpub.recv():
                    break
            xpub.close()


    def send(self,topic,payload):
        '''
        send a generic message with topic, payload
        '''
        self.socket.send(str(topic),flags=zmq.SNDMORE)
        self.socket.send(json.dumps(payload))

    def notify(self,notification):
        '''
        send a pupil notification
        notification is a dict with a least a subject field
        if a 'delay' field exsits the notification it will be grouped with notifications
        of same subject and only one send after specified delay.
        '''
        if notification.get('delay',0):
            self.send("delayed_notify.%s"%notification['subject'],notification)
        else:
            self.send("notify.%s"%notification['subject'],notification)


    def __del__(self):
        self.socket.close()


#create a zmq REQ socket to talk to Pupil Service/Capture
req = ctx.socket(zmq.REQ)
req.connect('tcp://localhost:50020')
req.send('PUB_PORT')

publisher=Msg_Dispatcher(ctx,'tcp://localhost:%s'%req.recv())
time.sleep(1)

#convenience functions
def send_recv_notification(n):
    # REQ REP requirese lock step communication with multipart msg (topic,json_encoded dict)
    req.send_multipart(('notify.%s'%n['subject'], json.dumps(n)))
    return req.recv()

def get_pupil_timestamp():
    req.send('t') #see Pupil Remote Plugin for details
    return float(req.recv())


# set calibration method to hmd calibration
n = {'subject':'start_plugin','name':'HMD_Calibration', 'args':{}}
print send_recv_notification(n)

# start caliration routine with params. This will make pupil start sampeling pupil data.
n = {'subject':'calibration.should_start', 'hmd_video_frame_size':(1000,1000), 'outlier_threshold':35}
print send_recv_notification(n)


# Mockup logic for sample movement:
# We sample some reference positions (in normalized screen coords).
# Positions can be freely defined

notification={'subject':'nextReference'}
topic=notification['subject']
payload=json.dumps(notification)

ref_data = []
publisher.notify({'subject':'calibration.start'})
for pos in ((0.5,0.5),(0,0),(0,0.5),(0,1),(0.5,1),(1,1),(1,0.5),(1,0),(.5,0)):
    print 'subject now looks at position:',pos
    publisher.notify({'subject':'calibration','pos':pos})
    for s in range(120):
        # you direct screen animation instructions here

        # get the current pupil time (pupil uses CLOCK_MONOTONIC with adjustable timebase).
        # You can set the pupil timebase to another clock and use that.
        t = get_pupil_timestamp()

        # in this mockup  the left and right screen marker positions are identical.
        datum0 = {'norm_pos':pos,'timestamp':t,'id':0}
        datum1 = {'norm_pos':pos,'timestamp':t,'id':1}
        ref_data.append(datum0)
        ref_data.append(datum1)
        time.sleep(1/60.) #simulate animation speed.

publisher.notify({'subject':'calibration.done'})
# Send ref data to Pupil Capture/Service:
# This notification can be sent once at the end or multiple times.
# During one calibraiton all new data will be appended.
n = {'subject':'calibration.add_ref_data','ref_data':ref_data}
print send_recv_notification(n)

# stop calibration
# Pupil will correlate pupil and ref data based on timestamps,
# compute the gaze mapping params, and start a new gaze mapper.
n = {'subject':'calibration.should_stop'}
print send_recv_notification(n)

