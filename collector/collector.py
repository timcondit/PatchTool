#!python

# collector.py
#
# Returns a dictionary of files from a given root.  Accepts full or relative
# paths.
#
# Key: path plus basename, value: basename (e.g., {'workdir\headers.bat',
# 'headers.bat'})

import fnmatch
import hashlib
import os
import shutil
import sys

def md5sum(in_file):
  f = open(in_file, 'rb')
  contents = f.read()
  f.close()
  m = hashlib.md5()
  m.update(contents)
  return m.hexdigest()

# the current list of file extensions in PatchLib.cs [2/6/2012]
patterns = [
        '*.application',
        '*.ascx',
        '*.aspx',
        '*.bat',
        '*.cab',
        '*.compiled',
        '*.config',
        '*.css',
        '*.deploy',
        '*.dll',
        '*.exe',
        '*.jar',
        '*.manifest',
        '*.pdb',
        '*.properties',
        '*.prx',
        '*.reg',
        '*.resx',
        '*.skin',
        '*.template',
        '*.tlb',
        '*.xsd',
        '*.xml',
        ]

origin = sys.argv[1]
out_file = sys.argv[2]
full_version = sys.argv[3]
version = full_version.rsplit('.', 1)[0]

# later: method or module
cache = r"C:\Users\Public\Builds\%s\%s" % (version, full_version)

try:
  os.makedirs(cache)
except WindowsError:
  print "Directory already exists: %s\nExiting." % cache

index = []
for root, dirs, files in os.walk(origin):
  for p in patterns:
    for filename in fnmatch.filter(files, p):
      src = os.path.join(root, filename)
      if src not in index:
        # add checksum and file's original source to index
        checksum = md5sum(src)
        index.append(checksum + " " + src + "\n")

        # rename file and copy from origin to cache
        dst = os.path.join(cache, checksum)
        shutil.copyfile(src, dst)
      else:
        raise

# last steps: write the index file to the cache
fh = open(os.path.join(cache, out_file), 'w')
for line in index:
  fh.write(line)
fh.close()

