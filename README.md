# README

This is a Unity3D Project which includes implementations of a transmission client for position tracking protocols.
The project currently is set up for 2019.2 but will very likely run flawlessly in newer versions. If you require an older version, refer to commit [215c665](https://github.com/Playful-Interactive-Environments/UnityTrackingClient/tree/215c665cc2beefb568fc62beb5e5fcea7b1103d4), which targets 5.3.2f1.

### Which protocols are supported?

* Open TUIO protocol (UDP / OSC.NET)
* Proprietary TrackLink (formerly referred to as Pharus) protocol (TCP)
* Proprietary TrackLink (formerly referred to as Pharus) protocol (UDP)

### What else is included?

* A simple evaluation module which records positions (and optionally also game events) to JSON format
* config.xml files for easy access to settings in builds

### OMGWTF, there is no proper documentation available!!!11

* That's right. We are truly sorry as there was no time yet to document the code appropriately. However, if you have inqueries about the project try to get in touch with us via our website: https://pie-lab.at/

By the way, here are two projects using the UnityTrackingClient: http://game-changer.at and https://lazorlab.rohschinken.at
