#!/usr/bin/env python3
import sys
import os

def count_bytes(file_name):

	#read = 0
	byte_counts = [ 0 for _ in range(256)]
	with open(file_name,'rb') as f:
		while (single_byte := f.read(1)):
			#read += 1
			#if read % 1024 == 0 :
				#if ( read / 1024 ) % 1024 == 0 :
					#print(read/1024/1024)
			if byte_counts[int.from_bytes(single_byte,byteorder='big')] < 2000000000:
				byte_counts[int.from_bytes(single_byte,byteorder='big')] += 1
		for i in range(256):
			print(i,',"',chr(i),'",',byte_counts[i],',')
	return byte_counts

if __name__ == '__main__' :
	count_bytes(sys.argv[1])
#	with open(sys.argv[1]+'.lgc','w+') as OUTPUT:
#		OUTPUT.write(compress(sys.argv[1]))

