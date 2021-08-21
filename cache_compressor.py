#!/usr/bin/env python3
"""Script to test compression using arithmetic encoded rolling cache"""

import os
import sys
import argparse
import coloredlogs,logging
from pathlib import Path
import functools
import array 

#Base configuration stuffs
# --------------------------------------------------
LIBRARY_FOLDER = os.path.join(Path.home(), '.cache_compressor')
if not os.path.isdir(LIBRARY_FOLDER):
            if os.path.exists(LIBRARY_FOLDER):
                raise NotADirectoryError(
                    f'{LIBRARY_FOLDER} is not a directory')
            os.mkdir(LIBRARY_FOLDER)
CONFIG_FILE = os.path.join(LIBRARY_FOLDER, 'config.json')
LOG_FILE = os.path.join(LIBRARY_FOLDER,'cache_compressor.log')
# --------------------------------------------------

# Logging configuration stuffs
# --------------------------------------------------
logger = logging.getLogger('cache_compressor')
if not os.path.isfile(LOG_FILE):
	if os.path.exists(LOG_FILE):
		raise IsADirectoryError(f'{LOG_FILE} is a directory')
fh = logging.FileHandler(LOG_FILE, mode='a', encoding="UTF-8", delay=False, errors="backslashreplace")
formatter = logging.Formatter(
    '\x1B[38;5;0;48;5;9m%(asctime)s %(name)s %(levelname)s: %(message)s\x1b[0m')
fh.setFormatter(formatter)
logger.addHandler(fh)
coloredlogs.install(level='DEBUG')
def log_this(function):
	def new_function(*args,**kwargs):
		logger.debug('%s() %s %s',function.__name__,args,kwargs)
		output = function(*args,**kwargs)
		logger.debug('%s returned: %s',function.__name__,output)
		return output
	return new_function
def log_this_without_return(function):
	def new_function(*args,**kwargs):
		logger.debug('%s() %s %s',function.__name__,args,kwargs)
		output = function(*args,**kwargs)
		return output
	return new_function

logger.debug('if you can see this, logging has been initialized')
# --------------------------------------------------

@log_this
def parse_args():
	"""Parse the arguments provided in the command line."""
	parser = argparse.ArgumentParser(
		description='I iz gonna squishies themz gud!')

	parser.add_argument('files', metavar='file', nargs='+',required=True,
			help='file(s) to compress')

	args = parser.parse_args()

	return args

@log_this_without_return
def make_ranges():
	ranges = array.array('I')
	max_num = 268435456
	num_range = 1
	end_range = 0
	range_ends = array.array('L')
	range_ends.append(0)
	ranges.append(0)
	while max_num >= 65536 :
		logger.debug('starting range of: %s at: %s with weights %s',num_range,len(ranges),max_num)
		for item in range(num_range):
			end_range+=max_num
			ranges.append(end_range)
		range_ends.append(len(ranges))
		logger.debug('range of: %s ending at: %s with weights %s',num_range,end_range,max_num)
		num_range += num_range
		max_num = int(max_num/2)
	logger.debug('starting range of: 65279 at: %s with weights 1',len(ranges))
	logger.debug('starting rawBytes range of: 256 at: %s with weights 1',len(ranges))
	range_ends.append(len(ranges))
	for item in range(256):
		end_range+=65536
		ranges.append(end_range)
	logger.debug('range of: 256 ending at: %s with weights of: 65536',end_range)	
	range_ends.append(len(ranges))
	ranges.append(end_range+536870912)
	range_ends.append(len(ranges))
	logger.debug('returning ranges - length: %s, range_ends: %s',len(ranges),range_ends)
	return ranges,range_ends

make_ranges()


@log_this
def compress_datastream(ds):
	""" please sir, pass me only byte-like objects!! """
	buffer = bytearray(ds.read(1024))
	cache = ['']
	cache += [ bytes([i]) for i in range(256)]
	cache += [ None for _ in range(130813-255)]
	cache += [ bytes([i]) for i in range(256)]
	ranges, range_ends = make_ranges()
	backbuffer = bytearray(0)
	outstream = bytearray(0)
	while True :
		if len(buffer) < 1024 :
			buffer += bytearray(ds.read(1024 - len(buffer)))
			if len(buffer) == 0:
				break
		buffer_start = buffer[0]
		tokens_to_encode = []
		potential_cache_hits = sorted([ [index,item] for index,item in enumerate(cache) if item[0] == buffer_start],key = lambda x:len(x[1]))
		for cutoff in range(len(buffer),1,-1):
			buffer_slice = buffer[:cutoff]
			while len(potential_cache_hits[-1][-1]) > cutoff : 
				potential_cache_hits.pop()
			while len(potential_cache_hits[-1][-1]) == cutoff :
				index,item = potential_cache_hits.pop()
				if item == buffer_slice :
					backbuffer += buffer_slice
					tokens_to_encode = [range_ends[index-1],range_ends[index]]
					buffer = buffer[cutoff:]
					break
			if tokens_to_encode :
				break
		logger.debug('got tokens to encode - %s',tokens_to_encode)
	return outstream

def compress_file(path):
	with open(path,'rb') as fin:
		with open(path+'.hfs_cc','wb') as fout:
			fout.write(compress_datastream(fin))

with open('cache_compressor.py','rb') as f:
	print(compress_datastream(f))

sys.exit()

@log_this
def confirm_compression(input_stream,compressed_stream):
	pass

@log_this
def main():
    args = parse_args()
    if sys.stdin.isatty():
    	for source in args.files :
    		compress_file(source)
    else :
    	logger.debug('stdin, writing out to file: %s',args.files[0])
    	with open(args.files[0],'wb') as fout :
    		fout.write(compress_datastream(stdin))

if __name__ == '__main__':
    main()