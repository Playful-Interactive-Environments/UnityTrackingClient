Unity Pharus Tracking Client (UPTC)
Copyright 2022 Playful Interactive Environments

This package provides implementations for the TUIO and Tracklink protocols, used together with the Pharus tracking software, developed by the Futurelab at Ars Electronica Center Linz.
To use the package you need access to the Pharus tracking software - it provides prerecorded data for testing or use the TUIO framework for simulating tracking data.
The data that your application receives is converted by our framework and delivers you simple game objects with an id, x and y coordinates.

HOW TO USE
To use the tracking, import the Unity Tracking Client package or download the github project. In Unity create a new scene, add the tracking manager prefab and include player prefab in each of the
player managers. The tracking manager initializes the tuio and tracklink services, the player managers take care of player spawning. You can create your own player managers, inheriting from TuioPlayerManager
and TracklinkPlayerManager for custom player management, as well as the ATrackingEntity if you want to implement your own player logic. You can also override the main ATrackingManager for more custom logic.
The package provides you with an example of how the player lifecycle is managaged, as well as scene loading functionality.

TUIO Simulators
More information about the TUIO protocol can be found here https://tuio.org/. In essence it's an open framework that defines a common protocol for tangible multitouch surfaces.

If you want to simulate objects on your PC you can use this https://github.com/gregharding/TUIOSimulator 
TIP! TUIO has 3 different types of objects: cursors, objects and blobs. By default our framework ONLY uses TUIO cursors, because the Pharus tracking software sends additional data about 
player feet that gets represented as additional game objects.
The TUIO PC simulators allow you to simulate a tracking point with your mouse cursor, but also place static objects if you want to simulate additional players. To get this working you have to 
set the SubscribeTuioObjects boolean in the TuioPlayerManager to true. Otherwise the static TUIO objects will not appear, since by default we only use TUIO cursors. 
Don't forget to turn it off before you test the applicationw with Pharus!

Android Simulator
You can also use an Android smartphone to simulate the tracking on a touch screen: https://github.com/TobiasSchwirten/tuiodroid
TIP! TUIO simulators are can often be blocked by your Firewall - in case you are using an Android TUIO application or sending data from another device,
you might need to turn off your Firewall or add an exception for the TUIO port 3333. Make sure you do that before debugging further, it's the most common reason for not receiving tracking data.

Contact
If you have questions or problems, contact us at pie@fh-hagenberg.at

