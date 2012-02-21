#!python
#
# This is a test in Python to make sure I know how it should work in C#
# (specifically, Extractor or Clyde) later on.

applications = []
applications.extend(['SRV', 'CM', 'CT', 'CWA', 'DBM', 'WMWS'])

# imap.items help me figure out which files to update.  imap.values point to
# the installation folder that holds each application.  imap.keys identifies
# the application's files.
#
# Say a 10.1 ChannelManager install is found in the registry.  It might have a
# DisplayName of "Envision Channel Manager", and a DisplayVersion of
# 10.1.15.11.  Then imap["Envision Channel Manager"] = 'ICM'... (I've got it
# backwards).

installer_map = imap = {}

# this is stupid (but maybe useful)
# http://www.msigeek.com/282/windows-installer-guids


# next steps:
# 1: find installations and versions from registry
# 2: create map of 
# 3: 

imap['ICWA'] = 'Envision Web Apps'
imap['IWMWS'] = 'Envision Windows Media Wrapper Service'

if (version == '9.10' or version == '10.0'):
  imap['ISRV'] = 'SRV'
  imap['ISRV'] = 'CM'
  imap['ICT'] = 'CT'
  imap['IDBM'] = 'DBM'
elif (version == '10.1'):
  imap['ISS'] = 'SRV'
  imap['ICM'] = 'CM'
  imap['ISS'] = 'CT'
  imap['ITOOLS'] = 'DBM'
else:
  print "something broke!"
