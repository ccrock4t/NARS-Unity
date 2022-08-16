/*
    Author: Christian Hahm
    Created: May 12, 2022
    Purpose: Specific configuration settings for the NARS
*/

using UnityEngine;


public class NARSConfig {
    public NARSConfig()
    {

    }

    /*
        System Parameters
    */
    public int k = 1;  // evidential horizon
    public float T = 0.6f;  // decision rule (goal decision-making) threshold
    public float MINDFULNESS = 1.0f;
    public int BAG_GRANULARITY = 100;
    public float FOCUSX = 1.23f;
    public float FOCUSY = 1.04f;

    public int TAU_WORKING_CYCLE_DURATION = 25;  // time in milliseconds per working cycle

    public float POSITIVE_THRESHOLD = 0.51f;
    public float NEGATIVE_THRESHOLD = 0.5f;

    public int MEMORY_CONCEPT_CAPACITY = 1000;  // how many concepts can this NARS have?
    public int EVENT_BUFFER_CAPACITY = 15;
    public int GLOBAL_BUFFER_CAPACITY = 1000;
    public int CONCEPT_LINK_CAPACITY = 100;  // how many of each concept link can this NARS have?

    /*
        Sensors
    */
    public int[,] VISION_DIMENSIONS = new int[28, 28];

    /*
        GUI
    */
    public bool SILENT_MODE = true;  // the system will only output executed operations
    public bool DEBUG = false;  // set to true for useful debug statements
    public bool paused = false;


    /*
        Inference
    */
    public float PROJECTION_DECAY_DESIRE = 0.95f;
    public float PROJECTION_DECAY_EVENT = 0.95f;

    public int NUMBER_OF_ATTEMPTS_TO_SEARCH_FOR_SEMANTICALLY_RELATED_CONCEPT = 3;  // The number of times to look for a semantically related concept to public interact with
    public int NUMBER_OF_ATTEMPTS_TO_SEARCH_FOR_SEMANTICALLY_RELATED_BELIEF = 5; // The number of times to look for a semantically related belief to public interact with
    public float PRIORITY_DECAY_VALUE = 0.29f; // value in [0,1] weaken band w/ priority during priority decay
    public float PRIORITY_STRENGTHEN_VALUE = 0.99f;  // priority strengthen bor multiplier when concept == activated

    /*
        Bags
    */
    public int BAG_DEFAULT_CAPACITY = 1000; // default for how many items can fit in a bag

    /*
        Tables
    */
    public int TABLE_DEFAULT_CAPACITY = 5;

    /*
        Other Structures
    */
    public static int MAX_EVIDENTIAL_BASE_LENGTH = 30; // maximum IDs to store documenting evidential base

    /*
        Default Input Task Values
    */
    public const float DEFAULT_JUDGMENT_FREQUENCY = 1.0f;
    public const float DEFAULT_GOAL_FREQUENCY = 1.0f;

    public const float DEFAULT_DISAPPOINT_CONFIDENCE = 0.5f;

    public const float DEFAULT_JUDGMENT_PRIORITY = 0.9f;
    public const float DEFAULT_QUESTION_PRIORITY = 0.9f;
    public const float DEFAULT_GOAL_PRIORITY = 0.9f;
    public const float DEFAULT_QUEST_PRIORITY = 0.9f;

}