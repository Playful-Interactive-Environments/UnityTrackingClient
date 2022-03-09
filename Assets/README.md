Unity Pharus Tracking Client (UPTC)
Copyright 2022 Playful Interactive Environments

This package provides implementations for the TUIO and Tracklink protocols, used together with the Pharus tracking software, developed by the Futurelab at Ars Electronica Center Linz.
To use the package you need access to the Pharus tracking software - it provides prerecorded data for testing.

We also support the TUIO protocol, https://tuio.org/. You can use one of the simulators on the website to fake tracking data and develop your application, before testing it on location.
You can also use an Android smartphone to simulate the tracking on a touch screen: https://github.com/TobiasSchwirten/tuiodroid

NOTE! TUIO simulators are usually blocked by your Firewall - in case you are using an Android TUIO application or sending data from another device,
you might need to turn off your Firewall or add an exception for the TUIO port 3333. Make sure you do that before debugging further, it's the most common reason for not receiving tracking data.

HOW TO USE
To use the tracking, simply create a new scene, add the tracking manager prefab and include player prefab in each of the player managers. The tracking manager initializes the tuio and tracklink services, 
the player managers take care of player spawning. You can create your own player managers, inheriting from TuioPlayerManager and TracklinkPlayerManager for custom player management, as well as the ATrackingEntity if you want to implement your own player logic.

