# Home Assistant Add-on: Remotely add-on

# WARNING! Breaking changes in release 2024!
see https://github.com/immense/Remotely/releases/tag/v2024.02.23.1927

## Basics
this addon using amazing [Remotely](https://github.com/immense/Remotely)

## Instalation
instalation is straigth forward. Everthing may work at first try. Access port is 5000. If you need access using https, you have to use reverse proxy then. More info on Remotely web page.


## limitation
Because HAOS docker:
1) you will not see computers public IP adrdess and all computers will have same docker internal IP. It seems this has no functional impact.
2) i am not able to present recording forder to HA. Remotely has not internal screen recording management, so please, leave it off. Othervise it will consume your disk space. Every start of add-on will clear all recordings.
3) You cannot backup Remotely settins directly in application, so when you wants to move it or uninstall and install again, add-on present database for you. More in next section. 

## How to use

please read documentation on [Remotely](https://github.com/immense/Remotely) page

## Home Assistant specific info
Add-on presents itself in config folder. This is new feature in HAOS. In this folder, remotely-config, you can find database file.
From 2024 remotely present only DB engine setting. So for HA it is not nescasarry to present config file.
Never copy or replace DB file while addon running.
