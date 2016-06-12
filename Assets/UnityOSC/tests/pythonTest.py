
import time
from OSC import OSCClient, OSCMessage
client = OSCClient()
client.connect( ("192.168.11.140", 9000) )

value=0
while True:
	msg=OSCMessage("/pupil/norm_pos")
	msg.append(value)
	value+=0.001
	client.send( msg )
	time.sleep(0.001)
