# Mobile Phone Breathing Detection

Lyn Cassandra Phan @ Georgia Tech Brain Music Lab

The goal of this project was to build a cross-platform app to accurately detect breathing using a fast and efficient peak detection algorithm I designed. The app has full logging capabilities and is designed to be easily extended and used in a lab environment to gather data.

The platform chosen was Unity, due to easier compatibility, as well as ease of development since this project was built during one Fall 2020 semester at Georgia Tech. The original prototype was demoed during Spring 2020.

The project is functional but documentation is still a work in progress. Documentation about the algorithm used and procedure will be coming shortly, in addition to videos and pictures.

If you still want to try it out: BreathingDetection/bin/Android/build.apk is the most recent Android build of the software.

Everything below here is under construction. Peruse at your own risk!

# Table of Contents
- [Demo and Basic Terms Defined](https://github.com/sheepbun-monster/Mobile-Phone-Breathing-Detection/tree/development#demo-and-basic-terms-defined)
- [Breathing Detection Algorithm in Detail](https://github.com/sheepbun-monster/Mobile-Phone-Breathing-Detection/tree/development#breathing-detection-algorithm-in-detail)
- [Logs and Graphs](https://github.com/sheepbun-monster/Mobile-Phone-Breathing-Detection/tree/development#logs-and-graphs)

# Demo and Basic Terms Defined
[![Mobile Phone Breathing Detection Demo](http://img.youtube.com/vi/VqZaJIptPFM/0.jpg)](http://www.youtube.com/watch?v=VqZaJIptPFM "Mobile Phone Breathing Detection Demo")

Peak Detection - Finding maximums in a periodic signal. The algorithm used in this project outwardly functions similarly to peak detection but is more accurately described as threshold detection.

Positional Buffered Averages - Position data from each axis is recorded into a buffer of a certain length. All of these values are then averaged to produce an average value for each axis over the time duration of the buffer.

Buffered Average Differences - By getting the magnitude of the difference between a long (minutes long) and short (seconds long) buffered average for each axis, we can calculate how strong the current positional difference is from the norm. Perfect for using with threshold detection.

Detection Threshold - This is the threshold the buffered average differences are measured against. A value greater than this threshold will result in a positive flag raise for a possible detected breath.

Denoising - Removing or ignoring excess positives to reduce the chance for false positives or counting multiple times per breath. Especially useful in continuous live data streams such as this.


# Breathing Detection Algorithm in Detail


# Logs and Graphs
