#!/usr/bin/env python3
"""
Script to test arithmetic encoded LRU cache with twoByteStartTable 

"""

import os
import sys
import argparse
import coloredlogs,logging
from pathlib import Path
import functools
import array 
from collections import OrderedDict

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
logger = logging.getLogger('twoByteStartCompressor')
if not os.path.isfile(LOG_FILE):
	if os.path.exists(LOG_FILE):
		raise IsADirectoryError(f'{LOG_FILE} is a directory')
fh = logging.FileHandler(LOG_FILE, mode='a', encoding="UTF-8", delay=False, errors="backslashreplace")
formatter = logging.Formatter(
    '\x1B[38;5;0;48;5;9m%(asctime)s %(name)s %(levelname)s: %(message)s\x1b[0m')
fh.setFormatter(formatter)
# logger.addHandler(fh)
coloredlogs.install(level='DEBUG')
def log_entry_exit(function):
	def new_function(*args,**kwargs):
		logger.debug('%s() called',function.__name__)
		output = function(*args,**kwargs)
		logger.debug('%s exiting',function.__name__)
		return output
	return new_function

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
	def __init__(self,thing,initial_range):
		self.data = thing
		self.range_length = inital_range
		logger.debug('adding a thing - %s %s',self.data,self.stamp,self.range_length)
		self.update(initial_range)

	def update(self,range_add):
		self.stamp = LRU_Cache_Thing.monotonic
		self.range_length += range_add
		logger.debug('updating a thing - %s %s',self.data,self.stamp,self.range_length)
 
class LRUCache:
    """ based on https://www.geeksforgeeks.org/lru-cache-in-python-using-ordereddict/ """
    # initialising capacity
    def __init__(self, capacity: int):
        self.cache = OrderedDict( ) 
        for i in range(capacity):
        	self.cache[str(i)]=i 

    # we return the value of the key
    # that is queried in O(1) and return -1 if we
    # don't find the key in out dict / cache.
    # And also move the key to the end
    # to show that it was recently used.
    # @log_this
    def get(self, key: str) -> int:
        if key not in self.cache:
            return -1
        else:
            self.cache.move_to_end(key)
            return self.cache[key]
 
    # first, we add / update the key by conventional methods.
    # And also move the key to the end to show that it was recently used.
    # But here we will also check whether the length of our
    # ordered dictionary has exceeded our capacity,
    # If so we remove the first key (least recently used)
    @log_this
    def put(self, key: str, value: int) -> None:
    	if key in self.cache :
	        self.cache[key] = value
    	    # self.cache.move_to_end(key)
    	else :
	        self.cache.popitem(last = False)		
	        self.cache[key] = value
    @log_this
    def pop_last(self):
    	return self.cache.popitem(last = False)
    # @log_this
    def replace_return_lru(self,key: str) -> int:
    	index = self.cache.popitem(last = False)[1]
    	self.cache[key] = index
    	return index

class LRU_Token_manager:
	@log_this
	def __init__(self,lru_capacity=4096,lru_initial_size=1,lru_grow_by=1,two_byte_range = 4096): 
		logger.debug('lru_capacity %s, lru_initial_size %s , lru_grow_by %s , two_byte_range %s',lru_capacity,lru_initial_size,lru_grow_by,two_byte_range)
		self.LRU = LRUCache(lru_capacity)
		self.lru_initial_size = lru_initial_size 
		self.lru_grow_by = lru_grow_by
		self.two_byte_range = two_byte_range
		self.two_byte_range_end = 65536*two_byte_range
		self.range_sizes = array.array('L')
		self.range_ends = array.array('L')
		self.range_limit = 2**32 - ( self.lru_initial_size * 2 )

		range_end = self.two_byte_range_end
		self.range_ends.append(range_end)
		for _ in range(lru_capacity):
			self.range_sizes.append(self.lru_initial_size)
			range_end += self.lru_initial_size
			self.range_ends.append(range_end)
		self.range_sizes.append(1)
		self.range_ends.append(range_end+1)

	def match_buffer_front(self,buffer):
		for cutoff in range(len(buffer),2,-1):
			if ( index := self.LRU.get(buffer[:cutoff])) != -1 :
				low = self.range_ends[index] 
				high = self.range_ends[index+1] - 1
				logger.debug('found match at index: %s',index)
				old_with_new = self.LRU.replace_return_lru(buffer[:cutoff + 1 ])
				if (old_range_diff := self.range_sizes[old_with_new] - self.lru_initial_size ) > 0 :
					self.range_sizes[old_with_new] = self.lru_initial_size
					for range_index in range(old_with_new,len(self.range_sizes)):
						self.range_ends[range_index] -= old_range_diff

				if self.range_ends[-1] < self.range_limit :
					self.range_sizes[index] += self.lru_grow_by
					for range_index in range(index,len(self.range_sizes)): 
						self.range_ends[range_index] += self.lru_grow_by
				else :
					logger.warn('out of range!!!')

				return cutoff, low, high

		low = (int(buffer[0])+(int(buffer[1])*256))*self.two_byte_range
		high = low + self.two_byte_range - 1
		old_with_new = self.LRU.replace_return_lru(buffer[:3 ])
		if (old_range_diff := self.range_sizes[old_with_new] - self.lru_initial_size ) > 0 :
			self.range_sizes[old_with_new] = self.lru_initial_size
			for range_index in range(old_with_new,len(self.range_sizes)):
				self.range_ends[range_index] -= old_range_diff

		return 2, low, high

	def add_update(self,thing):
		if len(thing) == 2 :
			return 
		if ( index := self.LRU.get(thing) ) == -1 :
			self.LRU.put(thing,)

@log_entry_exit
def compress_datastream(ds):
	""" please sir, pass me only byte-like objects!! """
	LRU = LRU_Token_manager()
	logger.info('LRU initialized.. range_ends[-1]: %s -  Random BS GO!!!',LRU.range_ends[-1])

	slices_at = 64
	buffer = ds[:slices_at]
	next_read_at=slices_at
	read_until = 0
	# debug_out = []
	outstream = []
	while len(buffer) > 1 :
		# out_tokens = []
		logger.debug('top of while - %s %s',next_read_at,buffer)
		cut_at,low,high = LRU.match_buffer_front(buffer)
		print(low,high,buffer[:cut_at],cut_at,high-low)
		buffer = buffer[cut_at:]
		# print(out_tokens)
		outstream.append(high-low+1)
		if len(buffer) < slices_at :
			read_until = min(len(ds),next_read_at + slices_at -len(buffer))
			if next_read_at < read_until :
				buffer += ds[next_read_at:read_until]
				next_read_at = read_until
	average_range = sum(outstream)/len(outstream)
	bits = 32
	ar = average_range
	while ar > 0.5 : 
		bits -= 1 
		ar /=2
	estimated_file_size = int((bits * len(outstream) + 7)/8)
	logger.debug('end stream allowed range of %s',2**32 - LRU.range_ends[-1])
	logger.info('length of instream: %s, length of outstream: %s, compression_ratio: %s with average range: %s using bits %s',len(ds),len(outstream),len(ds)/len(outstream),average_range,bits)
	logger.info('expected size in bytes <%s, expected total compression ratio: ~%s',estimated_file_size,len(ds)/estimated_file_size)
	# return outstream

def compress_file(path):
	with open(path,'rb') as fin:
		with open(path+'.hfs_cc','wb') as fout:
			fout.write(compress_datastream(fin.read()))

with open(sys.argv[1],'rb') as f:
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