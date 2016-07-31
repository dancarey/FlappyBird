using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    public Text debugText;

    string logger;
    Queue loggerQueue = new Queue();

    void OnEnable ()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable ()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logger = logString;

        string newString = "\n [" + type + "] : " + logger;
        loggerQueue.Enqueue(newString);

        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            loggerQueue.Enqueue(newString);
        }

        logger = string.Empty;

        foreach(string entry in loggerQueue)
        {
            logger += entry;
        }
    }

    void OnGUI () {
        debugText.text = logger;
    }
}

