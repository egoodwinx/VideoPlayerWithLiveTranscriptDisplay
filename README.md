# Video Player with Live Transcript Display
## Introduction
I created this application because I prefer having the all of the captions available for my lectures, with the current caption being highlighted. This is how zoom displayers their live transcripts and after my school moved from the zoom cloud, I took it upon myself to create a video player that mimicked this functionality. I created the initial prototype in two days and therefore it's not fully functional.

## Design Decisions
The subtitles in the .srt file are combined if a start time is the same as the end time of a previous subtitle. 

## How to Use
1. Open your .srt file using open button on the right
2. Open the media file using the open button on the top left menu

* Use the "Play" button to play and the "Pause" button to pause
* Use the slider at the bottom of the video to change the video position
* Use the slider beside the "Pause" button to change the volume
* Double click a caption to change the video to that timestamp

## Todo
* Add configuration details to save information on closing & opening application
* Update UI to be more usefriendly/better overall
* Support more than .srt files
* Allow changing of video speed
* Better error handling
* Logging
