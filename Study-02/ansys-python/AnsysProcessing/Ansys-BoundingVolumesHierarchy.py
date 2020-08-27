import csv
import os, stat
import numpy


# DIMENSIONS:
# (-4, 0, -6)
# (4, ,3.5, 6)
# You can give it two corners and a number of dividers in the X, Y, Z dimensions
cornerA = numpy.array([-4, 0, -6])
cornerB = numpy.array([4, 3.5, 6])
lowestBVHCorner = numpy.array([min(cornerA[0], cornerB[0]), min(cornerA[1], cornerB[1]), min(cornerA[2], cornerB[2])])

#configurations
divisions = 7
boxDimensions = numpy.divide(numpy.absolute(cornerA - cornerB),divisions)

# First make the smallest point in space (0,0,0) for evaluation
# In this case, p' = p - (-4, 0, -6).
# then (int)floor(p' / S), where S is (xD, yD, zD) and xD = (4 - (-4)) / divisionX

# Function: Calculates the bounding volume (bin/box) from the point's position.
def boxIndex(p):
	pPrime = p - lowestBVHCorner # Now the origin is right.
	index = numpy.array([numpy.floor(pPrime[0]/boxDimensions[0]), numpy.floor(pPrime[1]/boxDimensions[1]), numpy.floor(pPrime[2]/boxDimensions[2])])
	# print(index)
	return index

boxes = numpy.empty((divisions+1, divisions+1, divisions+1), list)

# Function: Shoves the point into the correct bin.
def binPoint(p):
	pPos = numpy.array([p[0],p[1],p[2]]) # Omit concentration value
	i = boxIndex(pPos)
	if boxes[int(i[0])][int(i[1])][int(i[2])] is None: # If it's empty (no points here yet)
		boxes[int(i[0])][int(i[1])][int(i[2])] = [] # Instantiate a list.
	boxes[int(i[0])][int(i[1])][int(i[2])].append(p) # add the current point.
	# print(box)


# Make a Bounding Volumes folder to store results.
currDir = os.getcwd()
mode = 0o666
resultsFolderName = "AnsysBoundingVolumes"
framePath = os.path.join(currDir, resultsFolderName)
if not os.path.exists(framePath):
	os.mkdir(framePath)

framesDir = os.path.join(currDir, "AnsysFrames")
for frame in os.listdir(framesDir):
	ansysFrame = os.fsdecode(frame)
	print(ansysFrame)

	boxes = numpy.empty((divisions+1, divisions+1, divisions+1), list)

	with open("AnsysFrames/" + ansysFrame) as ansysData:
		ansysReader = csv.reader(ansysData, delimiter=",",  quoting=csv.QUOTE_NONNUMERIC)
		for point in ansysReader:
			# print(point)
			binPoint(point)

	# For each bounding volume, write it's CSV file.
	frameNumber = str(''.join(i for i in ansysFrame if i.isdigit()))
	for i in range(divisions+1):
		for j in range(divisions+1):
			for k in range(divisions+1):
				boxName = "Box_i" + str(i) + "_j" + str(j) + "_k" + str(k)
				boxPath = os.path.join(framePath, boxName)
				if not(boxes[i][j][k] is None): 
					if not os.path.exists(boxPath):
						os.mkdir(boxPath, mode)
						os.chmod(boxPath, mode)
					with open(resultsFolderName + "/" + boxName + "/" + frameNumber + ".csv", mode='w', encoding='UTF-8', newline='') as bvCSV:
						writer = csv.writer(bvCSV)
						for pnt in boxes[i][j][k]:
							writer.writerow(pnt)



# above works. Next step: Loop over all files in a folder.
