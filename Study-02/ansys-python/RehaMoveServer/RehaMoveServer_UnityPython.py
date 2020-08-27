# 
# by: Jingxuan Wen, 2020
# edited by: Jas Brooks
# 

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

r = Rehamove("COM18")
currentChannel = "red"

validDurations = [100, 200, 400]
validCurrent = [0, 0.25, 0.5, 0.75, 1]
frequency = 150

# the parameter to adjust current, increasing by 1, maximum is 5. increasing 1 will cause current increase 0.1
bias = 0
FoV = math.pi/2

calibrationMode = False

# Stimulation
def stimulate(pulse):
    for i in range(10):
        r.custom_pulse(currentChannel, pulse)
        time.sleep(float(1)/float(frequency))
    return pulse

##TODO
def stimulate_by_side(unused_addr, args, message):
    [x, y, z, zone] = list(map(float, message.split()))
    print('\n' + message)
    if z < 0:
        return 0
#    print('side = ' + str(sign(x)))
#    pulse = [(sign(x) * (1 + bias * 0.1), 100), (-1, 100)]
    if sign(x)<0: #left
        pulse = [(0,200), (-1,400)]
    else: #right
        pulse = [(0.5,400), (-0.5,200)]
    print('pulse = ' + str(pulse))
    stimulate(pulse)
    return pulse

def stimulate_by_radius(unused_addr, args, message):
    [x, y, z, zone] = list(map(float, message.split()))
    print('\n' + message)
    distance = math.sqrt(x*x +z*z) # x*x + y*y + z*z
    print('distance = ' + str(distance))
    if zone == 0:
        pulse = [(0.5,100), (-0.5,100)]
    elif zone == 1:
        pulse = [(0.5,400), (-1,400)]
    else: return 0

    print('pulse = ' + str(pulse))
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
    dispatcher.map("/stimulation_S", stimulate_by_side, "dummy")
    dispatcher.map("/stimulation_R", stimulate_by_radius, "dummy")

    # Server
    server = osc_server.ThreadingOSCUDPServer((args.ip, args.port), dispatcher)
    print('Serving on {}'.format(server.server_address))
    server.serve_forever()