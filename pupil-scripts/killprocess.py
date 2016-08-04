from zmq_tools import *
from OSC import OSCClient, OSCMessage,OSCServer
import time,threading,msgpack
import types

current_milli_time = lambda: int(round(time.time() * 1000))

ctx=zmq.Context()
req=ctx.socket(zmq.REQ)
req.setsockopt(zmq.LINGER,0)
req.connect('tcp://localhost:50020')

print "connected"

req.send('SUB_PORT')
ipc_subPort=req.recv()

print "port received"


def send_recv_notification(n):
    # REQ REP requirese lock step communication with multipart msg (topic,json_encoded dict)
    req.send_multipart(('notify.%s'%n['subject'], msgpack.dumps(n)))
    return req.recv()

n = {'subject':'service_process.should_stop'}
print send_recv_notification(n)

print "Done"
