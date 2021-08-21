#!/usr/bin/env python3
"""
Script to test Actually Adaptive Arithmetic encoded LZW-style Dictionary with LRU cache

TODO - index LRU and Dictionary by start byte value
"""

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
# logger.addHandler(fh)
coloredlogs.install(level='INFO')
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
		description='Actually Adaptive Arithmetic encoded LZW-style Dictionary with LRU cache - I iz gonna squishies themz gud!')

	parser.add_argument('files', metavar='file', nargs='+',required=True,
			help='file(s) to compress')

	args = parser.parse_args()

	return args


class LRU_Cache_Thing :
	monotonic = 0
	def __init__(self,thing):
		self.data = thing 
		self.update()

	def update(self):
		self.stamp = LRU_Cache_Thing.monotonic
		logger.debug('updating a thing - %s %s',self.data,self.stamp)
		LRU_Cache_Thing.monotonic += 1 

@log_this
def compress_datastream(ds):
	""" please sir, pass me only byte-like objects!! """
	ranges = array.array('L')
	for _ in range(2048):
		ranges.append(1038576)
	for _ in range(4096):
		ranges.append(262144)
	""" # never evict these!
	for _ in range(256):
		ranges.append(4194304)
	"""
	range_left = 2**32 - ranges[-1] - 256

	range_end = 0
	range_ends = array.array('L')
	for ranger in ranges :
		range_end += ranger
		range_ends.append(range_end)
	logger.debug('%s',range_ends)
	outstream = []
	logger.debug('range_left - %s ',range_left)
	LRU_cache = [ LRU_Cache_Thing(bytes(chr(i),encoding='LATIN-1')) for i in range(256) ]
	Dictionary = [ b'X' for _ in range(4096)] #filled with illiterate Mr. Hancocks
	dictionary_index = 0
	LRU_indexes = [ [i] for i in range(256)]
	Dict_indexes = [ [] for _ in range(256)]
	raw_byte_tokens = [ bytes(chr(i),encoding='LATIN-1') for i in range(256) ]
	logger.info('LRU initialized.. Random BS GO!!!')

	slices_at=16

	buffer = ds[:slices_at]
	next_read_at=slices_at
	read_until = 0
	debug_out = []
	while buffer :
		out_tokens = []
		logger.info('top of while - %s %s',next_read_at,buffer)
		match = False
		for cutoff in range(len(buffer),0,-1):
			buffer_slice = buffer[:cutoff]
			# logger.debug('buffer_slice -%s :\n%s',cutoff,buffer_slice)
			for index in LRU_indexes[buffer[0]]:
				cached = LRU_cache[index] 
				# logger.debug('checking LRU_Cache %s :\n%s',index,cached.data)
				if buffer_slice == cached.data :
					logger.debug('LRU cache match! %s :',index)
					out_tokens = [(range_ends[index] - ranges[index],range_ends[index])]
					cached.update()
					if range_left > 0 :
						ranges[index] += 1024 
						range_left -= 1024 
						for r_index in range(index,4096):
							range_ends[r_index]+=1024
						# print(range_ends)
					if cutoff < slices_at :
						removeFrom = Dictionary[dictionary_index][0]
						if removeFrom != buffer[0] :
							try :
								Dict_indexes[removeFrom].pop(Dict_indexes[removeFrom].index(dictionary_index))
							except ValueError :
								pass
							Dict_indexes[buffer[0]].append(dictionary_index)
						Dictionary[dictionary_index] = buffer[:cutoff+1]
						dictionary_index += 1 
						if dictionary_index >=2048 :
							dictionary_index = 0
					buffer = buffer[cutoff:]
					debug_out.append(buffer_slice)
					logger.debug('cutoff : %s',cutoff)
					match = True
					break
			if match:
				break
			for index in Dict_indexes[buffer[0]] :
				if buffer_slice == Dictionary[index]:
					logger.debug('Dictionary match! %s :',index)
					if cutoff < slices_at :
						removeFrom = Dictionary[dictionary_index][0]
						if removeFrom != buffer[0] :
							try :
								Dict_indexes[removeFrom].pop(Dict_indexes[removeFrom].index(dictionary_index))
							except ValueError :
								pass
							Dict_indexes[buffer[0]].append(dictionary_index)
						Dictionary[dictionary_index] = buffer[:cutoff+1]
						dictionary_index += 1 
						if dictionary_index >=2048 :
							dictionary_index = 0
						if len(LRU_cache) < 2048 :
							LRU_indexes[buffer[0]].append(len(LRU_cache))
							LRU_cache.append(LRU_Cache_Thing(buffer_slice))
						else :
							too_old = int(LRU_Cache_Thing.monotonic - 1982 )
							# never evict the single bytes !
							for index,cached in enumerate(LRU_cache[256:],256) :
								if cached.stamp < too_old :
									logger.debug('evicting from LRU at age: %s, index %s - %s',cached.stamp,index,cached.data)
									LRU_indexes[buffer[0]].append(index)
									LRU_indexes[buffer[0]].pop(LRU_indexes[buffer[0]].index(index))
									LRU_cache[index] = LRU_Cache_Thing(buffer_slice)
									range_left += ranges[index]
									range_left -= 1038576
									ranges[index] = 1038576
									range_end = 1038576
									if index > 0 : 
										range_end += ranges[index - 1]
									range_ends[index] = range_end
									for r_index in range(index+1,4096):
										range_end += ranges[r_index]
										range_ends[r_index] = range_end
									too_old += 1
									break
					index += 2048
					out_tokens = [(range_ends[index] - ranges[index],range_ends[index])]
					buffer = buffer[cutoff:]
					debug_out.append(buffer_slice)
					logger.debug('cutoff : %s',cutoff)
					match = True
					break
			if match :
				break
		"""
		if not match :
			logger.debug('no match found at all, resulting to single byte from raw indexes')
			index = 4096 + int(buffer[0])
			Dictionary.append(buffer[0])
			Dictionary.append(buffer[:2])
			while len(Dictionary) > 2048 :
				Dictionary.pop(0)
			debug_out.append(buffer[0])
			out_tokens = [(range_ends[index] - ranges[index],range_ends[index])]
			buffer = buffer[1:]
		"""
		print(out_tokens)
		outstream.append(out_tokens)
		if len(buffer) < slices_at :
			read_until = min(len(ds),next_read_at + slices_at -len(buffer))
			if next_read_at < read_until :
				buffer += ds[next_read_at:read_until]
				next_read_at = read_until
	logger.info('length of instream: %s, length of outstream: %s',len(ds),len(outstream))
	for index,thing in enumerate(LRU_cache):
		print(index,thing.data,thing.stamp,ranges[index])
	for thing in debug_out:
		print(thing)
	sys.exit()
	return outstream

def compress_file(path):
	with open(path,'rb') as fin:
		with open(path+'.hfs_cc','wb') as fout:
			fout.write(compress_datastream(fin.read()))

with open('system-stats.txt','rb') as f:
	print(compress_datastream(f.read()))

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
    		fout.write(compress_datastream(stdin.read()))

if __name__ == '__main__':
    main()