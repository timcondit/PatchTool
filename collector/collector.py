#!python

# collector.py
#
# Returns a dictionary of files from a given root.  Accepts full or relative
# paths.
#
# Key: path plus basename, value: basename (e.g., {'workdir\headers.bat',
# 'headers.bat'})

import fnmatch
import os
import sys

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

rootPath = sys.argv[1]
outFile = sys.argv[2]

sources = {}
for root, dirs, files in os.walk(rootPath):
  for p in patterns:
    for filename in fnmatch.filter(files, p):
      path = os.path.join(root, filename)
      sources[path] = filename

f = open(outFile, 'w')
for k, v in sources.items():
    f.write(k + "," + v + "\n")

