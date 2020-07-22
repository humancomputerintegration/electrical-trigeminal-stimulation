import csv
import math
import numpy as np
from scipy.spatial import Delaunay

fMax = 300
fMin = 0
concentrionMin = 1e-4 ## kg/m^3
concentrionMax = 1.0
resolution = 0.1 ## m
roomX = 8.0
roomZ = 12.0
roomY = 3.5

for cf in range(fMin, fMax+1):
    points = []
    ## readfile
    with open(f'original/{cf}.csv', mode = 'r') as infile:
        reader = csv.reader(infile)
        i = 0
        for row in reader:
            i = i+1
            if(i<=6): continue
            if(float(row[3])<concentrionMin): continue
            points.append([float(row[2]), float(row[1]), float(row[0]), float(row[3])]) ## x, y, z, c (Ansys has inversed x and z)
    if points.__len__()<3: continue
    ## upsampling
    print(f'\n#{cf}:')
    print(f'original points num: {points.__len__()}')
    pointArray = np.array(points)
    tri = Delaunay(pointArray[:, :3])
    tris = pointArray[tri.simplices]
    for t in tris:
        center = (t[0] + t[1] + t[2] + t[3])/4
        points.append(center)
        for p in t:
            squaredDist = np.sum((p-center)**2, axis=0)
            upsampleCnt = int(round(np.sqrt(squaredDist)/resolution))
            for k in range(1, upsampleCnt):
                points.append(list(np.array(p[:3])*k/upsampleCnt + np.array(center[:3])*(upsampleCnt-k)/upsampleCnt) + [p[3]*(upsampleCnt-k)/upsampleCnt + center[3]*k/upsampleCnt])
    print(f'upsampled points num: {points.__len__()}')
    with open(f'upsampled/point_{cf}.csv', mode = 'w', newline='') as p_out:
        w = csv.writer(p_out)
        for point in points:
            w.writerow(point)