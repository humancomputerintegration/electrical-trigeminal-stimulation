from rehamove import Rehamove
import time
import random

r = Rehamove('COM15')
channel = 'red'

preset = [
    [(2, 100), (-2, 100)],
    [(-2, 100), (2, 100)],
    [(1, 500), (-1, 100)],
    [(1, 100), (-1, 500)],
    [(-1, 500), (1, 100)],
    [(-1, 100), (1, 500)],
]

selection = [
    [1,2,0,1,0],
    [2,0,0,1,0],
    [2,1,0,1,0],
    
]

Frequency = [3, 150, 300]

Current = [0, 0.5, 1] # abs

Width = [100, 200, 400] # ms

def make_pulse(c1, w1, c2, w2, phase): # phase = 1 for positive first, -1 for negative first
    phase1 = (phase * Current[c1], Width[w1])
    phase2 = (-phase * Current[c2], Width[w2])
    return [phase1, phase2]

def stimulate(waveform, f):
    for i in range(20):
        r.custom_pulse(channel, waveform)
        time.sleep(float(1)/float(f))

def test(c1,w1,c2,w2,f,p): # phase = 1 for positive first, -1 for negative first
    stimulate(make_pulse(c1,w1,c2,w2,p), Frequency[f])

def test(*args):
    if len(args) == 1


if __name__ == '__main__':
    while(True):
        k = list(map(int, input('\nenter the pulse num: ').split()))
        time.sleep(2)
        for t in range(len(k)):
            if k[t] >= len(preset):
                print(str(k[t]) + ' out of range')
                continue    
            pulse = preset[k[t]]
            print(pulse)
            stimulate(pulse)