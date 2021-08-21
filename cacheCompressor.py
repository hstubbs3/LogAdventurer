#!/usr/bin/env python3
"""Script to test compression using arithmetic encoded rolling cache"""


#Base configuration stuffs
# --------------------------------------------------
LIBRARY_FOLDER = os.path.join(Path.home(), '.cache_compressor')
CONFIG_FILE = os.path.join(LIBRARY_FOLDER, 'config.json')
LOG_FILE = os.path.join(LIBRARY_FOLDER,'cace_compressor.log')
# --------------------------------------------------

# Logging configuration stuffs
# --------------------------------------------------
logger = logging.getLogger('cache_compressor')
fh = logging.FileHandler(LOG_FILE, mode='a', encoding="UTF-8", delay=False, errors="backslashreplace")
logger.addHandler()
coloredlogs.install(level='DEBUG')

""" please sir, pass me only byte-like objects!! """
def compress_datastream(ds):
	buffer = bytearray(0)
	cache = [ bytes([i]) for i in range(256)]
	cache += [ _ for _ in range(130813-255)]
	cache += [ bytes([i]) for i in range(256)]
	buffer = f.read
