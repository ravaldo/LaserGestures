# LaserGestures

### Concept & Demo
What if we were to take a webcam, face it towards a blank wall, and use a laser pointer to "draw" on the wall? Could we use this as a means of gesture recognition? Click the thumbnail below to watch a demo.

<div align="center">

[![Watch the video](https://i.imgur.com/zhBTHlc.jpg)](https://youtu.be/IBJamp9SOWQ)
</div>

### Project Details
The app was written in C# using Visual Studio and makes use of the [AForge](http://www.aforgenet.com/framework/) framework (specifically v2.2.2). The AForge dlls are included in the libs folder and copies should be placed in the same folder as the LaserGestures executable.

The app listens on TCP port 5678 and can forward recognised gestures to any connected clients. In the demo above I am forwarding gestures to [AutoHotKey](https://www.autohotkey.com/) to control the music player.

### Design

Gestures are drawn with a focus on strokes going in one of eight directions. By focusing on straight lines this lends itself well to what the average person could reliably draw on a wall from a distance and gives us a reasonable margin of error (users can be off by 22.5 degrees to either side of their intended stroke).

<div align="center">

![compass diagram](https://i.imgur.com/Ibt41pJ.png)
</div>

With single-segment strokes we would have only eight possible gestures. We can, however, increase our gesture space exponentially by allowing strokes to be chained together and also broken up (by a small delay). 

<div align="center">

![range of possible gestures](https://i.imgur.com/61V2QMC.png)
</div>

The eight directions are also given a single character alias (which maps to the numpad of a keyboard). This allows us to describe a gesture succinctly as a string in the app.


### Dynamic Gestures?
Static gestures are great; they are easy to define, quick to draw and we can perform a single repeatable action. But what if we wanted the app to act dynamically, that is, perform a sustained action that changes (and possibly scales) depending on the laser point's continued motion? This would be better suited for actions that benefit from a finer degree of control, such as changing the volume as shown in the demo.

The app currently recognises two types of dynamic gestures:
* TURN: this simulates the turning of a knob and can send a "-" or "+" message.
* SEEK: this measures the laser point's horizontal distance from its starting location and will send a message with a ± number that scales proportionally.

To perform a dynamic gesture you simply need to draw a "dot" before the intended gesture and then immediately press again to keep the laser point on. The app will remain in that particular dynamic mode until the point disappears.


### Fine Tuning
A captured gesture is essentially a list of 2d points; the higher the framerate of the webcam then the more points there will be. The app defines an interface called Filter which will process the list of points in some fashion to "clean" the input. Several Filters are implemented and they can be arranged into a sequence where the points list will be processed by each Filter in turn. We can adjust certain thresholds on some of the Filters, add/remove/reorder them within the sequence or indeed create new ones.

<div align="center">

![filter sequence](https://i.imgur.com/WltFXKq.png)
</div>

### Other Features

##### Keystoning
If the webcam isn't orthogonal to the drawing surface then the angles of certain strokes may not be interpreted correctly ([Keystone effect](https://en.wikipedia.org/wiki/Keystone_effect)). In the webcam calibration menu, we can have the user draw their best approximation of a square on the wall and use that to apply an image transformation to the stream.

![keystoning](https://i.imgur.com/l9ouY1E.jpg)


##### Closest Match

When a user draws a gesture, the app will take the filtered input and check to see if it matches any in the current gesture list. To give the user some leeway in their drawing ability, if no match is found then the app checks to see how close they may have came. We use a modified [Levenshtein distance](https://en.wikipedia.org/wiki/Levenshtein_distance) algorithm which scores similar stroke directions higher (for example with the character U then a 9 is more similar than R).

<div align="center">

![close match](https://i.imgur.com/O3HFi9f.png)
</div>
