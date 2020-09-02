from rehamove import *          # Import our library
import time
import random
import os
import numpy as np

clear = lambda: os.system('cls' if os.name=='nt' else 'clear')

comPort = "COM15" # 15 is the one nect to the WiFi adapter, 18 is the headset one.
r = Rehamove(comPort)            # Open USB port (on Windows)
currentChannel = "red"

allowedDurations = [
	100, 250, 500
	]
allowedStimulations = [
	1,
	0
	]
allowedFrequencies = [
	150
	]

def randomSign():
	return 1 if random.random() < 0.5 else -1

# We aren't doing phase-checking for this.
def phaseCheck(stimulationArray):
	integration = 0
	for point in stimulationArray:
		integration += point[0]*point[1]
	return integration

def searchSpaceGenerator():
	unsignedStimulations = []
	for d in allowedDurations:
		for s in allowedStimulations:
			# if s == 0:
			# 	unsignedStimulations.append((0, 100))
			# else:
				unsignedStimulations.append((s, d))
	signs = [1, -1]
	space = []
	for si in signs:		# adding signs back in.
		for phase1 in unsignedStimulations:
			for phase2 in unsignedStimulations:
				if (phase2[0] == 0):
					p2 = (0,0)
				else:
					p2 = (si * -1 * phase2[0], phase2[1])

				if (phase1[0] == 0):
					p1 = (0,0)
					space.append([p2, p1])
				else:
					p1 = (si*phase1[0], phase1[1])
					space.append([p1, p2])

	repeatsRemoved = [i for n, i in enumerate(space) if i not in space[:n]]
	return repeatsRemoved

def frequencyVariants(space):
	variants = []
	for freq in allowedFrequencies:
		for stim in space:
			variants.append((freq, stim))
	return variants

def pulseGenerator():
	newPulse = [(0,0)] * 2
	stimLen = len(allowedStimulations) - 1
	durLen = len(allowedDurations) - 1
	signOrder = randomSign()

	firstStim = allowedStimulations[random.randint(0,stimLen)]
	while (firstStim == 0):
		firstStim = allowedStimulations[random.randint(0,stimLen)]
	
	newPulse[0] = (signOrder * firstStim, allowedDurations[random.randint(0,durLen)])
	
	secondStim = allowedStimulations[random.randint(0,stimLen)]
	if (secondStim > 0):
		newPulse[1] = (-1 * signOrder * secondStim, allowedDurations[random.randint(0,durLen)])
	else:
		newPulse[1] = (0, 0)
	return newPulse


def likert(descriptor):
	print("On the following scale, rate how " + descriptor + " the sensation was.")
	print("\tleast \t 1 2 3 4 5 6 7 \t most")
	response = input("Evaluation: ")
	return response

def repeat(arr, count):
	return np.stack([arr for _ in range(count)], axis=0)

def is_digit(n):
	try:
		int(n)
		return True
	except ValueError:
		return  False

def pulseDurr(arr):
	duration = 0
	for phase in arr:
		duration = duration + phase[1]
	return duration

testMode = False

userNumber = input("What is the user code? ")
userResponseSheet = open("response" + str(userNumber) + ".csv", "w")

if (not testMode):
	clear()

	## Personal information
	participantAge = input("How old are you? ")
	participantSex = input("What is your sex? ")
	userResponseSheet.write("USER INFO: " + "," + participantAge + "," + participantSex + "\n")

	clear()

	## NOSE Scale
	print("We will now administer the NOSE Scale questionnaire.")
	print("Over the past ONE month, how much of a problem were the following conditions for you? Type the most correct response for each question.\n")
	print("\t0 = Not a Problem")
	print("\t1 = Very Mild Problem")
	print("\t2 = Moderate Problem")
	print("\t3 = Fairly Bad Problem")
	print("\t4 = Severe Problem\n")
	while True:
		noseAnswer1 = input("Nasal congestion or stuffiness: ")
		if is_digit(noseAnswer1):
			if (0 <= eval(noseAnswer1)) and (eval(noseAnswer1) <= 4):
				break
			else:
				print("\tThis value is not in the range.")
		else:
			print("\tYou did not enter a valid number.")
	while True:
		noseAnswer2 = input("Nasal blockage or obstruction: ")
		if is_digit(noseAnswer2):
			if (0 <= eval(noseAnswer2)) and (eval(noseAnswer2) <= 4):
				break
			else:
				print("\tThis value is not in the range.")
		else:
			print("\tYou did not enter a valid number.")
	
	while True:
		noseAnswer3 = input("Trouble breathing through my nose: ")
		if is_digit(noseAnswer3):
			if (0 <= eval(noseAnswer3)) and (eval(noseAnswer3) <= 4):
				break
			else:
				print("\tThis value is not in the range.")
		else:
			print("\tYou did not enter a valid number.")
	
	while True:
		noseAnswer4 = input("Trouble sleeping: ")
		if is_digit(noseAnswer4):
			if (0 <= eval(noseAnswer4)) and (eval(noseAnswer4) <= 4):
				break
			else:
				print("\tThis value is not in the range.")
		else:
			print("\tYou did not enter a valid number.")
	
	while True:
		noseAnswer5 = input("Unable to get enough air through my nose during exercise or exertion: ")
		if is_digit(noseAnswer5):
			if (0 <= eval(noseAnswer5)) and (eval(noseAnswer5) <= 4):
				break
			else:
				print("\tThis value is not in the range.")
		else:
			print("\tYou did not enter a valid number.")
	
	userResponseSheet.write("NOSE: " + noseAnswer1 + "," + noseAnswer2 + "," + noseAnswer3 + "," + noseAnswer4 + "," + noseAnswer5 + "\n")

	print("Thank you!")

clear()

while True:
	nextTask = input("When you are ready, enter 'n' to begin the study. Enter 'q' to end the study completely.\n")
	if (nextTask == 'n') or (nextTask == 'q'):
		break
	else:
		print("This is not an option.")

userResponseSheet.write("Pulse1,Pulse2,Lateralization,Intensity\n")
shuffledSearchSpace = frequencyVariants(searchSpaceGenerator())
print(len(shuffledSearchSpace))
repeatSpace = shuffledSearchSpace + shuffledSearchSpace + shuffledSearchSpace
print(len(repeatSpace))
print(repeatSpace)
print("You will be trying " + str(len(repeatSpace)) + " stimulations.")
random.shuffle(repeatSpace)
cP = 0
while True:
	if (nextTask.find('q') > -1) or (cP == len(repeatSpace)):
		print("Thank you for participating in this study.")
		userResponseSheet.close()
		exit()
	elif (nextTask.find('n') > -1):
		while True:
			clear()
			time.sleep(5 * random.random() + 1) # Initial pause to avoid expectation.
			currentPulse = repeatSpace[cP][1]
			currentFrequency = repeatSpace[cP][0]

			for i in range(20):
				r.custom_pulse(currentChannel, currentPulse)

				time.sleep(0.0083 - pulseDurr(currentPulse)/1000000) # Keep frequency at 62 Hz

			print("If you need to repeat, enter 'r'. Else, enter anything.")
			repeat = input("Enter selection: ")
			if (repeat != 'r'):
				break
		cP = cP + 1

	print("Rate the side of stimulation.")
	print("\tLeft side \t -3 -2 -1 0 1 2 3 \t Right side")
	while True:
		lateralization = input("Evaluation: ")
		if is_digit(lateralization):
			if (-3 <= float(lateralization)) and (float(lateralization) <= 3):
				break
			else:
				print("This value is not in the range.")
		else:
			print("You did not enter a valid number.")
	
	print("Rate the intensity of stiumulation.")
	print("\tDid not feel \t 1 2 3 4 5 6 7 \t Strongest")
	while True:
		intensity = input("Evaluation: ")
		if is_digit(intensity):
			if (1 <= float(intensity)) and (float(intensity) <= 7):
				break
			else:
				print("\tThis value is not in the range.")
		else:
			print("\tYou did not enter a valid number.")
	userResponseSheet.write("\"" + str(currentPulse[0]) + "\",\"" + str(currentPulse[1]) + "\",")
	userResponseSheet.write(str(lateralization) + "," + str(intensity) + "\n")

	pulsesLeft = len(repeatSpace) - cP;
	print("You have " + str(pulsesLeft) + " left.")
	print("Options:\n")
	print("\t'n' for next pulse.")
	print("\t'q' to end the study completely.\n")
	while True:
		nextTask = input("Enter selection: ")
		if (nextTask == 'n') or (nextTask == 'q'):
			break
		else:
			print("\tThis is not an option.")