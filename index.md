---
title: Mobile Phone Breathing Detection
---

{% include image.html url="WIN_20201205_01_33_25_Pro.jpg" description="source: Lyn Phan" %}

# Table of Contents
- [Table of Contents](#table-of-contents)
- [Basics](#basics)
  * [Objective](#objective)
  * [What Features Does This Have?](#what-features-does-this-have-)
  * [Why Unity Instead Of Native Code?](#why-unity-instead-of-native-code-)
  * [Main Contributors](#main-contributors)
- [How To Use](#how-to-use)
  * [Basic Procedure](#basic-procedure)
  * [Interface Guide](#interface-guide)
  * [Video Demonstration](#video-demonstration)
- [How Does This Work?](#how-does-this-work-)
  * [The Breathing Detection Algorithm](#the-breathing-detection-algorithm)
  * [Is This Even Good?](#is-this-even-good-)
  * [Glossary (In Order of Appearance)](#glossary--in-order-of-appearance-)
- [Sample Logs and Graphs](#sample-logs-and-graphs)
- [Try It Out!](#try-it-out-)

[comment]: # (TOC Generator Used: https://ecotrust-canada.github.io/markdown-toc/)

# Basics
## Objective
An cross-platform app that can gather breathing data as cheaply and efficiently as possible for use in a lab environment.

This is important because it allows for accurate breathing detection on a common device, the smartphone. This project can help increase the availability of information on hardware-minimal software-oriented biometric measurement tools, since they can be used where more complicated machinery might not be available.


## What Features Does This Have?
- cross-platform
- accurate breathing detection
- efficient enough to run on phones
- uses as few sensors as possible (especially avoiding gyroscope as many low end phones don't have one)
- full logging capabilities
- enough visual feedback to be easily used in a lab environment


## Why Unity Instead Of Native Code?
- Development time.
  - I only had one semester to complete this before I finished my undergrad degree, so this would be much faster than developing natively.
- Easy cross platform compatibility.
  - This allows for the app to be used on multiple types of phones in our research lab.
- Why not port the original prototype from last spring?
  - The original prototype was done in Max/MSP, which interfaced with phone sensors through the TouchOSC app. This setup was more suited to working with data streams, but required two devices (a computer and smartphone) to operate.
  - There are also no documented or functional APIs for using Max/MSP with Android or iOS at this time of writing.
- Handing off the project to less experienced coders in the future.
  - There was a strong possibility that his project will be passed on to someone else in the lab after I graduated. This unfortunately rules out React Native since that workflow may be too complicated for someone who doesn't code.


## Main Contributors
- Lyn Cassandra Phan (Georgia Tech Brain Music Lab)
  - [phan@gatech.edu](mailto:phan@gatech.edu)
  - [github.com/sheepbun-monster](https://github.com/sheepbun-monster)


# How To Use

## Basic Procedure
1. Open the app on a compatible device with an accelerometer.
2. Place the app on a participant's abdomen. Participant can be sitting up with the phone leaned against them, but lying down produces the best results since it reduce movement.
3. Wait for the app to calibrate. Total difference should be relatively close to zero and the threshold value should have settled to within a ±0.05 range. If this is taking too long, the app can be opened on the phone after it is placed so that it immediately zeroes to that position.
4. Make a note of the directory where the log will be stored if you want to retrieve it later.
5. Leave the phone on the participant as long as needed to collect breathing data.
6. Remove the phone from the participant and close the app.
7. If further analysis is desired, connect the phone up to a computer to retrieve the CSV log file. This is feature may require tweaking to work on iOS.

## Interface Guide
{% include image.html url="interface-guide.png" description="source: Lyn Phan" %}

Note that each axis position value is a floating point value measured between -1.0f and +1.0f. Any values referring to position are simply a planar representation of the rotational position. You can observe how this works by running the app and watching how the position values react to movement of the phone.

## Video Demonstration
{% include youtubePlayer.html id="VqZaJIptPFM" description="source: Lyn Phan" %}




# How Does This Work?

## The Breathing Detection Algorithm
{% include image.html url="breathing-detection-algorithm-diagram.png" description="source: Lyn Phan" %}

## Is This Even Good?
Yes it definitely is. Using this method with data buffers, instead of more traditional reference point measurements, allows for slow deviations in participant movement, resulting in more accuracy over long term data collection. Since the thresholds are calculated automatically, no prior information or manual calibration is needed from either the device or the participant. And this works on even cheap smartphones and only uses a single accelerometer sensor. This is a true set it and forget it solution.

However, the main limitation is the use of only a single sensor, resulting in occasionally missed peaks or false positives. Much more precise motion tracking could be done through the use of a gyroscope and accelerometer in tandem. This is how extremely precise motion controls are achieved on modern game controllers, notably on the Nintendo Switch Pro Controller. Since not every phone contains a gyroscope, and adding support would dramatically increase implementation time, it wasn't included for this project.

## Glossary (In Order of Appearance)
**Peak Detection** - Finding maximums in a periodic signal. The algorithm used in this project outwardly functions similarly to peak detection but can be more accurately described as threshold detection.

**Position Buffers** - Position data from each axis is recorded into a buffer of a certain length.

**Position Buffer Averages** - All of these values are then averaged to produce an average value for each axis over the time duration of the buffer.

**Buffered Average Differences** - By getting the magnitude of the difference between a long (minutes long) and short (seconds long) buffered average for each axis, we can calculate how strong the current positional difference is from the norm. Perfect for using with threshold detection.

**Detection Threshold** - This is the threshold the buffered average differences are measured against. A value greater than this threshold will result in a positive flag raise for a possible detected breath.

**Denoising and Debouncing**- Removing or ignoring excess positives to reduce the chance for false positives or counting multiple times per breath. Especially useful in continuous live data streams such as this.





# Sample Logs and Graphs

## Example 1: Sitting Up With Normal Breathing

{% include image.html url="sample-log-1-screenshot.png" description="source: Lyn Phan" %}

{% include image.html url="01-01 buffer averages.png" description="source: Lyn Phan" %}

{% include image.html url="01-02 accelerometer raw data.png" description="source: Lyn Phan" %}

{% include image.html url="01-03 total difference and maginitude.png" description="source: Lyn Phan" %}

{% include image.html url="b01-04 breathing rate.png" description="source: Lyn Phan" %}


# Try It Out!

If you want to try out the Android build, BreathingDetection/bin/Android/build.apk is the most recent Android build of the software. You'll need to allow apps to be sideloaded on your device to run this. This build was tested on a Motorola Moto G5 Plus.

There's no iOS build of this yet as I didn't have time to ensure compatibility, but you should be able to clone this repository and build it yourself. There may be issues with the way files are stored on iPhones, but that's something I didn't get a chance to look into yet.
