# by: Jingxuan Wen & Jas Brooks
# University of Chicago

import argparse
import math
import string
from rehamove import Rehamove
import time
import os

from pythonosc import dispatcher
from pythonosc import osc_server

# Parameters and predefinitions
clear = lambda: os.system('cls' if os.name=='nt' else 'clear')
sign = lambda x: -1 if x < 0 else 1

# RehaMove settings
r = Rehamove("COM28")
currentChannel = "red"
frequency = 150

# Stimulation
def stimulate(pulse):
    for i in range(10):
        r.custom_pulse(currentChannel, pulse)
        time.sleep(float(1)/float(frequency))
    return pulse

def stimulation(unused_addr, args, message):
    [side, intensity] = list(message.split())
    pulse = []
    if (float(intensity) < 3.0): # Low intensity 
        if (side == 'l'):
            pulse = [(-1, 250), (1, 250)]
        elif (side == 'm'):
            pulse = [(-1,100), (1,100)]
        else:
            pulse = [(-1,100), (1,100)]
    elif (float(intensity) < 6.0): # Medium intensity 
        if (side == 'l'):
            pulse = [(-1, 250), (0, 0)]
        elif (side == 'm'):
            pulse = [(1,100), (-1,250)]
        else:
            pulse = [(-1,100), (1,250)]
    else: # High intensity 
        if (side == 'l'):
            pulse = [(-1, 500), (1, 100)]
        elif (side == 'm'):
            pulse = [(-1,500), (1,500)]
        else:
            pulse = [(1,500), (-1,100)]
    stimulate(pulse)
    return pulse

# Main
if __name__ == "__main__":
    # Set the port
    parser = argparse.ArgumentParser()
    parser.add_argument("--ip", default="127.0.0.1", help = "The ip to listen on")
    parser.add_argument("--port", type=int, default=9001, help = "The port to listen on")
    args = parser.parse_args()

    # Deal with the message 
    dispatcher = dispatcher.Dispatcher()
    dispatcher.map("/stimulation", stimulation, "dummy")
    
    # Server
    server = osc_server.ThreadingOSCUDPServer((args.ip, args.port), dispatcher)
    print('Serving on {}'.format( server.server_address))
    server.serve_forever()