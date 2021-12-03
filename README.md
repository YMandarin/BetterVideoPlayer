# BetterVideoPlayer (BVP)
The goal of this windows application is to make a video player that incorporates 
the features and style of the standard windows 10 video player and make it better and more keyboard optimized

### accepted file types: 
mp4, mvk, mov, wmv, avi

### open video
a video file can be passed as command line argument </br>
(this way video files can be open with BVP and BVP can be set as standard program to open video files) </br>
otherwise videos can be opened by drag+drop, hotkey "ctrl+o" or open file button in menu

### modes
hover mode: activate hover mode to get the player to always stay on top </br>
mouse mode: change if clicking in the middle of the window pauses/plays the video

### Key-map:
- ESC: escape fullscreen
- SPACE: play/pause video
- RIGHT: fast forward by 10 seconds
- LEFT: rewind by 10 seconds
- M: mute volume
- UP: increase volume by 10%
- DOWN: decrease volume by 10%
- F11 / F: toggle fullscreen
- H: toggle hover mode
- CTRL+O: open file

### next/previous video
when opening a file BVP checks for next/previous videos alphabetically in the directory of the opened file </br>
click on the right or left border to open the next or previous video </br>
hover over the arrow see the name of the next/previous video </br>
