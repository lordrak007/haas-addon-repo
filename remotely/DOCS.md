# Home Assistant Add-on: Remotely add-on


## Basics
this addon using amazina Rmotely [https://github.com/immense/Remotely]

## Instalation
instalation is straith forward. Everthing may work at first try.


## limitation
Because HAOS docker:
1) you will not see computers public IP adrdess but all computers will have same docker internal IP. It seems this is not functional limitations.
2) i am not able to present recording forder to HA. Remotely has not internal screen recording management, so please, leave it off. Othervise it will consume your disk space. Every start of add-on will cleard all recordings.
3) You cannot backup Remotely setins in application, so when you wants to move it or uninstall and install again, add-on present database and config for you. More in next section. 

## How to use

please read documentation on [https://github.com/immense/Remotely] page

## Home Assistant specific info
Add-on presents itself in config folder. This is new feature in HAOS. In this folder, remotely-config, you can find database file and configuration file.