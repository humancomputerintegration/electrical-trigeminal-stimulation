import argparse
import math
import string
from rehamove import *
import time

from pythonosc import dispatcher
from pythonosc import osc_server

leftFactor = 1.0
rightFactor = 1.0

# Stimulation
def Stimulate(unused_addr, args, message):
    if message == 0:
        pulse = [(-1, int(leftFactor*100)), (-1,100)]
        side = 'left'
    else:
        pulse = [(1, int(rightFactor*100)), (-1,100)]
        side = 'right'
    
    for i in range(0,5): # increasing
        r.custom_pulse(currentChannel, pulse)
        time.sleep(0.01)
    
    print('ETS is on the ' + side + ' side')
    print('Left: '+str(leftFactor)+', right:' + str(rightFactor)+'\n')

def ChangeParameter(unused_addr, args, message):
    [l,r] = message.split()
    leftFactor = float(l)
    rightFactor = float(r)

# Main
if __name__ == "__main__":
    # ETS initialization
    r = Rehamove("COM18")   # Open USB port (on VIVE)
    currentChannel = "red"

    # Listening to the port
    parser = argparse.ArgumentParser()
    parser.add_argument("--ip", default="127.0.0.1", help = "The ip to listen on")
    parser.add_argument("--port", type=int, default=9001, help = "The port to listen on")
    args = parser.parse_args()

    # Deal with the message
    dispatcher = dispatcher.Dispatcher()
    dispatcher.map("/stimulation", Stimulate, "dummy")
    dispatcher.map("/parameter", ChangeParameter, "dummy")


    server = osc_server.ThreadingOSCUDPServer((args.ip, args.port), dispatcher)
    print('Serving on {}'.format( server.server_address))
    server.serve_forever()
