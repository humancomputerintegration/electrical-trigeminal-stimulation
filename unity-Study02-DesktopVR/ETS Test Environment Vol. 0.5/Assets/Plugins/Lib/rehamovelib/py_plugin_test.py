from rehamove import Rehamove
import sys
print(sys.path)
import time

class stimulator():
    def __init__(self, stimPort, channel):
        self.side = ''
        # self.r = rehamove.Rehamove(stimPort)
        # self.currentChannel = channel
    
    def stimulate(self, side):
        self.side = side
#        if(side == 'left'):
#            self.pulse = [(-1, 300), (-1,100)]
#        else:
#            self.pulse = [(1, 300), (-1, 100)]
#        self.r.custom_pulse(self.currentChannel, self.pulse)
        return 'ETS on '+ self.side +' side'