# README
This is a Unity3D Project which includes implementations of a transmission client for position tracking protocols.
To use the package you need access to the Pharus tracking software (developed by the Futurelab Ars Electronica Center Linz) or a TUIO simulator that generates tracking data.
The project is currently set up for 2020.3 but will very likely run without a problem in other versions.

### Which protocols are supported
* Open TUIO protocol (UDP / OSC.NET)
* Proprietary TrackLink (formerly referred to as Pharus) protocol (TCP)
* Proprietary TrackLink (formerly referred to as Pharus) protocol (UDP)

### What is included
* UnityPharusFramework.unitypackage for direct use in your project - drag and drop the prefabs in your scene or have a look at the example scene, provided with the package.
* UnityPharusAPI.dll containing the main protocol implementation. The source code can be found in the UnityPharusAPI folder and can be easily recompiled and adjusted to your needs.
* trackingConfig.xml configuration file for easy access to settings in builds, automatically exported in the Streaming Assets folder
* A simple evaluation module which records positions (and optionally also game events) to JSON format can be found in the tracking-evaluation branch

### HOW TO USE
To use the tracking, import the UnityPharusFramework.unitypackage or download the github project. In Unity create a new scene, add the tracking manager prefab and include player prefab in each of the
player managers. The tracking manager initializes the tuio and tracklink services, the player managers take care of player spawning. You can create your own player managers, inheriting from ATuioPlayerManager
and ATracklinkPlayerManager for custom player management, as well as the ATrackingEntity if you want to implement your own player logic. You can also override the main ATrackingManager for more custom logic.
The package provides you with an example of how the player lifecycle is managed, as well as scene loading functionality.

### TUIO Simulators
More information about the TUIO protocol can be found here https://tuio.org/. In essence it's an open framework that defines a common protocol for tangible multitouch surfaces.
If you want to simulate objects on your PC you can for example use https://github.com/gregharding/TUIOSimulator 
The TUIO PC simulators allow you to simulate a tracking point with your mouse cursor, but also place static objects if you want to simulate additional players.
To get this working you have to set the SubscribeTuioObjects boolean in the TuioPlayerManager to true. Otherwise the static TUIO objects will not appear. 
TUIO differentiates between 3 different types of objects: cursors, objects and blobs. The Pharus tracking software broadcasts data about individual player feet, in addition to each player tracking point. 
That means for each player the framework would spawn a total of 3 overlapping player objects, which can lead to many issues in your game logic. This is why in most cases we ONLY use TUIO cursors. 

### Android Simulator
You can also use an Android smartphone to simulate the tracking on a touch screen: https://github.com/TobiasSchwirten/tuiodroid
TIP! TUIO simulators can be blocked by your Firewall - in case you are using an Android TUIO application or sending data from another device,
you might need to turn off your Firewall or add an exception for the TUIO port 3333. Make sure you do that before debugging further, it's the most common reason for not receiving tracking data.

#### Contact
If you have questions or problems, contact us at pie@fh-hagenberg.at
Here are two projects using the UnityTrackingClient: http://game-changer.at and https://lazorlab.rohschinken.at

Unity Pharus Tracking Client (UPTC)
Copyright 2022 Playful Interactive Environments