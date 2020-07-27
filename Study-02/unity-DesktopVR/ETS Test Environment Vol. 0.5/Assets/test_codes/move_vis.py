from mpl_toolkits import mplot3d
import numpy as np
import matplotlib.pyplot as plt
import math
import scipy.linalg as lin

def rotate_mat(axis, degree):
	radian = float(degree)/180*math.pi
	rot_matrix = lin.expm(np.cross(np.eye(3), axis/lin.norm(axis)*radian))
	return rot_matrix

log_path = 'C:\\Users\\The Lab\\Desktop\\ETS test environment\\ETS Test Environment Vol. 0.5\\Movement\\'
name = input('Enter the name of participate: ')

fo = open(log_path+'MovementLog_'+name+'.txt.', 'r')
lines = fo.readlines()

positions = []
directions = []
timestamps = []

axis_x, axis_y, axis_z = [1,0,0], [0,1,0], [0,0,1]

target = np.array(lines[-2][1:-2].split(', '), dtype = float)

lines = lines[:-2]

for line in lines:
	strs = line.split(', (')
	timestamps.append(float(strs[0]))
	positions.append(np.array(strs[1][:-1].split(', '), dtype=float))
	directions.append(np.array(strs[2][:-2].split(', '), dtype=float))

timestamps = np.array(timestamps)
positions = np.array(positions)
directions = np.array(directions)


fig = plt.figure()
ax = plt.gca(projection = '3d')
ax.plot3D(positions[:,0], positions[:,2], positions[:,1], linewidth = 3, c = 'grey')

num = len(positions)

first = True
for i in range(num):
	s = positions[i]
	d = directions[i]
	ax.quiver(s[0],s[2],s[1],d[0],d[2],d[1], linewidth = 1, normalize = True)

# ax.scatter(positions[-5,0], positions[-5,2], positions[-5,1], s = 200, c = 'red')
ax.scatter(target[0], target[2], target[1], s = 200, c = 'red')

plt.axis([-5, 5, -5, 5])
ax.set_xlabel( "X" )
ax.set_ylabel( "Y" )
ax.set_zlabel( "Z" )
ax.set_zlim(-5, 5)
plt.show()