using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


// Dear future person who is going to work on this,
//
// Okay I know what you're thinking. Why did I literally put every last bit of scripting in this one file?
// Isn't that bad? Shouldn't I be making separate classes and stuff? Can't. And the reason I'm doing it is
// efficiency. Running multiple scripts simultaneously to gather and share data between each other causes a
// decent performance hit on weaker devices like phones, resulting in reduced sampling rates. Don't worry. 
// I promise doing this is hurting me inside too!
//
// -Lyn



// TODO: throw some of the features into functions because 
// reading this long thing is annoying.

public class MobilePhoneBreathingDetection : MonoBehaviour
{
    // Serialize frame rate field for UI
    [SerializeField]
    private Text framerateText;
    // Framerate / samplerate counter
    private int frameCounter = 0;
    private float timeCounter = 0.0f;
    private float refreshTime = 0.01f;
    private float sampleRate = 0.0f; // this one is for putting in the log


    // ------------------------------------------------------------------------


    // Serialize accelerometer position values for UI
    [SerializeField]
    private Text xPosText;
    [SerializeField]
    private Text yPosText;
    [SerializeField]
    private Text zPosText;
    // Initialize buffers for peak detection.
    // Buffers will be done with LinkedLists (a doubly-linked list) instead of arrays or Lists (which are just ArrayLists) 
    //  to reduce operations and time complexity while maintaining a certain size (since most actions are performed at 
    //  either end of the list, resulting in a time complexity of O(1) rather than sometimes being O(n) with a regular
    //  ArrayList). Tradeoff is the slightly increased memory usage, but most modern phones can handle this number of values
    //  easily.
    LinkedList<float> xShortBuffer = new LinkedList<float>();
    LinkedList<float> yShortBuffer = new LinkedList<float>();
    LinkedList<float> zShortBuffer = new LinkedList<float>();
    LinkedList<float> xLongBuffer = new LinkedList<float>();
    LinkedList<float> yLongBuffer = new LinkedList<float>();
    LinkedList<float> zLongBuffer = new LinkedList<float>();
    int shortBufferSize = 12;   // @ 60Hz = .2 seconds
    int longBufferSize = 12000;  // @ 60Hz = 200 seconds


    // ------------------------------------------------------------------------


    // Serialize log display fields for UI
    [SerializeField]
    private Text logLocationText;
    [SerializeField]
    private Text logDisplayText;
    // Initialize a short log list (for displaying) to avoid reading the whole log file every frame.
    LinkedList<string> logDisplayBuffer = new LinkedList<string>();
    // Initialize a count for how many entries were made in this session. 
    // Unfortunately measuring the log file will always result in slowdown even with the most efficient method.
    int sessionEntryCount = 0;
    int displayedLogSizeLimit = 50;
    // Serialize other logged values for UI
    [SerializeField]
    private Text xShortAvgText;
    [SerializeField]
    private Text yShortAvgText;
    [SerializeField]
    private Text zShortAvgText;
    [SerializeField]
    private Text xLongAvgText;
    [SerializeField]
    private Text yLongAvgText;
    [SerializeField]
    private Text zLongAvgText;
    [SerializeField]
    private Text xDifferenceText;
    [SerializeField]
    private Text yDifferenceText;
    [SerializeField]
    private Text zDifferenceText;


    // ------------------------------------------------------------------------


    // breathing detection fields

    // displayed fields
    [SerializeField]
    private Text detectionThresholdText;
    [SerializeField]
    private Text peakDetectedText; // this is a temporary one to aid in debugging
    [SerializeField]
    private Text breathDetectedText; 
    [SerializeField]
    private Text totalDifferenceDisplay;
    [SerializeField]
    private Text breathingRateDisplay;

    // threshold calculation variables
    private float startingThreshold = 0.05f;
    private float detectionThreshold = 0.0f;
    private float thresholdDecrement = 0.000005f; // value to decrement threshold by when a peak isn't found
    private float thresholdIncrement = 0.000005f; // value to increment threshold by when a peak is found

    // peak detection variables
    private bool peakDetected = false; // if a peak is detected

    // peak detection variables - implementation with buffer
    // a buffer to make things work easier
    LinkedList<bool> peakDetectionBuffer = new LinkedList<bool>();
    int peakDetectionBufferSize = 60; // length of buffer in samples ***change this if there are any sensitivity issues where***
    bool goingDown = false; //  prevents breathing from being counted twice as the difference crosses the threshold on the way back down
    private bool peakDetectedFiltered = false; //after using buffer to prevent double counting
                                               // this also prevents anything from being counted until the number of peaks
                                               // drops down to zero, so a shorter buffer size here is better

    // add a short interval where breaths can't be detected after each breath to prevent wobble from counting multiple times
    private float breathDetectionTimeCounter = 0.0f;
    private float breathDetectionWaitTime = 0.25f; // wait 0.25 seconds before allowing breath detection again

    // breathing rate calculation variables
    LinkedList<bool> filteredPeakDetectionBuffer = new LinkedList<bool>();
        // the buffer for this is calculated dynamically using the sample rate
    float breathingRateInterval = 60.0f; // previous time interval in seconds used to calculate breathing rate


    













    // Start is called before the first frame update
    void Start()
    {
        // Display log path
        string logPath = Application.persistentDataPath + "/breathingDetectionLog.csv"; // Application.persistentDataPath = '/storage/sdcard0/Android/data/' + package-name + '/files'
        logLocationText.text = "Log Location ~~~full log will include every numeric value on screen~~~ :\n" + logPath;
    }



    // Update is called once per frame
    void Update()
    {
        // Calculate framerate / samplerate
        if (timeCounter < refreshTime)
        {
            timeCounter += Time.deltaTime;
            frameCounter++;
        }
        else
        {
            float lastFramerate = frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f;
            framerateText.text = lastFramerate.ToString("n2");
            sampleRate = lastFramerate;
        }


        // Gather and display accelerometer position data
        float xPos = Input.acceleration.x;
        float yPos = Input.acceleration.y;
        float zPos = Input.acceleration.z;

        xPosText.text = xPos.ToString("n2");
        yPosText.text = yPos.ToString("n2");
        zPosText.text = zPos.ToString("n2");


        // Collect data into the peak detection buffers and trim to size
        xShortBuffer.AddFirst(xPos);
        while (xShortBuffer.Count > shortBufferSize)
        {
            xShortBuffer.RemoveLast();
        }

        yShortBuffer.AddFirst(yPos);
        while (yShortBuffer.Count > shortBufferSize)
        {
            yShortBuffer.RemoveLast();
        }

        zShortBuffer.AddFirst(zPos);
        while (zShortBuffer.Count > shortBufferSize)
        {
            zShortBuffer.RemoveLast();
        }

        xLongBuffer.AddFirst(xPos);
        while (xLongBuffer.Count > longBufferSize)
        {
            xLongBuffer.RemoveLast();
        }

        yLongBuffer.AddFirst(yPos);
        while (yLongBuffer.Count > longBufferSize)
        {
            yLongBuffer.RemoveLast();
        }

        zLongBuffer.AddFirst(zPos);
        while (zLongBuffer.Count > longBufferSize)
        {
            zLongBuffer.RemoveLast();
        }


        // Calculate averages of each buffer and display them
        float xShortAvg = xShortBuffer.Average();
        float yShortAvg = yShortBuffer.Average();
        float zShortAvg = zShortBuffer.Average();
        float xLongAvg = xLongBuffer.Average();
        float yLongAvg = yLongBuffer.Average();
        float zLongAvg = zLongBuffer.Average();

        xShortAvgText.text = xShortAvg.ToString("n2");
        yShortAvgText.text = yShortAvg.ToString("n2");
        zShortAvgText.text = zShortAvg.ToString("n2");
        xLongAvgText.text = xLongAvg.ToString("n2");
        yLongAvgText.text = yLongAvg.ToString("n2");
        zLongAvgText.text = zLongAvg.ToString("n2");


        // Calculate the absolute value of the difference between each x, y, and z sets
        float xDifference = Mathf.Abs(xShortAvg - xLongAvg);
        float yDifference = Mathf.Abs(yShortAvg - yLongAvg);
        float zDifference = Mathf.Abs(zShortAvg - zLongAvg);

        xDifferenceText.text = xDifference.ToString("n2");
        yDifferenceText.text = yDifference.ToString("n2");
        zDifferenceText.text = zDifference.ToString("n2");














        // Calculate thresholds
        if (detectionThreshold <= 0.0f || detectionThreshold >= 1.0)
        {
            detectionThreshold = startingThreshold; // reset if starting or it reaches zero
        }
        if (peakDetected == true)
        {
            detectionThreshold += thresholdIncrement;
        } 
        else
        {
            detectionThreshold -= thresholdDecrement;
        }
        detectionThresholdText.text = detectionThreshold.ToString("n3");


        // detect peaks for breathing rate calculation
        float breathingDetectionThreshold = detectionThreshold; 
            // calculated threshold is put in here, slightly inefficient but I think
            // it helps with code readability (jk I'm too lazy to change it)
        float totalDifference = xDifference + yDifference + zDifference;
        totalDifferenceDisplay.text = totalDifference.ToString("n3");
        if (totalDifference > breathingDetectionThreshold)
        {
            peakDetected = true;
            peakDetectedText.text = "peaks detected";
        } 
        else
        {
            peakDetected = false;
            peakDetectedText.text = "peaks not detected";
        }

        // use a buffer to filter peak detections to avoid counting groups
        peakDetectionBuffer.AddFirst(peakDetected);
        int peaksInBuffer = 0;
        foreach (bool i in peakDetectionBuffer)
        {
            if (i == true)
            {
                peaksInBuffer++;
            }
        }
                
        bool onePeakDetected = peaksInBuffer == 1;

        while (peakDetectionBuffer.Count > peakDetectionBufferSize)
        {
            peakDetectionBuffer.RemoveLast();
        }
        if (onePeakDetected)
        {
            
            if (!goingDown && breathDetectionTimeCounter > breathDetectionWaitTime)
            {
                peakDetectedFiltered = true;
                goingDown = true;
                breathDetectedText.text = "breath counted";
                breathDetectionTimeCounter = 0.0f;
            }
            else
            {
                peakDetectedFiltered = false;
                goingDown = false;
                breathDetectedText.text = "breath not detected";
            }

        } 
        else
        {
            peakDetectedFiltered = false;
        }
        breathDetectionTimeCounter += Time.deltaTime;
        
        

        // breathing rate calculations
        filteredPeakDetectionBuffer.AddFirst(peakDetectedFiltered);
        while (filteredPeakDetectionBuffer.Count > sampleRate * breathingRateInterval)
        {
            filteredPeakDetectionBuffer.RemoveLast();
        }

        int breathCounter = 0;
        foreach (bool i in filteredPeakDetectionBuffer)
        {
            if (i == true)
            {
                breathCounter++;
            }
        }

        float breathsPerMinute = breathCounter * (60 / breathingRateInterval); // 60 is the number of seconds in a minute

        breathingRateDisplay.text = breathsPerMinute.ToString("n0");
















        // Write to a log file
        string logPath = Application.persistentDataPath + "/breathingDetectionLog.csv"; // Application.persistentDataPath = '/storage/sdcard0/Android/data/' + package-name + '/files'

        string logFormat = "DateTime,sampleRate,xPosition,yPosition,zPosition,xShortBufferAvg,yShortBufferAvg,zShortBufferAvg," +
            "xLongBufferAvg,yLongBufferAvg,zLongBufferAvg,xAvgDifferenceMagnitude,yAvgDifferenceMagnitude,zAvgDifferenceMagnitude," +
            "totalDifferenceMagnitude,breathingDetectionThreshold,peakDetected,peakDetectedFiltered,breathsPerMinute\n";
        // Once the program is more finalized logFormat should be moved outside of this loop since it doesn't need to be reassigned every time.
        string logEntry = "";
        // using implicit conversions of floats to strings. not using .ToString("n2") because that would limit precision to 2 decimal places
        logEntry += System.DateTime.Now + ",";
        logEntry += sampleRate + ",";
        logEntry += xPos + "," + yPos + "," + zPos + ",";
        logEntry += xShortAvg + "," + yShortAvg + "," + zShortAvg + ",";
        logEntry += xLongAvg + "," + yLongAvg + "," + zLongAvg + ",";
        logEntry += xDifference + "," + yDifference + "," + zDifference + ",";
        logEntry += totalDifference + "," + breathingDetectionThreshold + ",";
        logEntry += peakDetected + "," + peakDetectedFiltered + "," + breathsPerMinute;
        logEntry += "\n";

        if (!File.Exists(logPath))
        {
            File.WriteAllText(logPath, logFormat + logEntry);
        } 
        else
        {
            File.AppendAllText(logPath, logEntry);
        }



        // Display a short fascimile of the log file (to avoid reading the whole file every frame)
        string displayedLogEntry = System.DateTime.Now + "," + xPos.ToString("n2") + "," + yPos.ToString("n2") + "," + zPos.ToString("n2") + "\n";
        logDisplayBuffer.AddFirst(displayedLogEntry);
        while (logDisplayBuffer.Count > displayedLogSizeLimit)
        {
            logDisplayBuffer.RemoveLast();
        }

        string logDisplayOutputString = "";
        sessionEntryCount++;
        logDisplayOutputString += "Entries made this session: " + sessionEntryCount.ToString() + "\n";
        logDisplayOutputString += "DateTime, xPos, yPos, zPos\n";

        foreach (string s in logDisplayBuffer)
        {
            logDisplayOutputString += s;
        }

        logDisplayText.text = logDisplayOutputString;




    }
}
