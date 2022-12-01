using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Controllers;
using System;
using System.IO;

public class MapperCamera : MonoBehaviour
{
    public Transform target;

    public GameObject avatarRenderer;

    public bool goToFirstPosition;

    public int N = 5;

    public float flyingHeight = 10f;

    private float parcelSize = 16f;
    private float distance = 50f;

    private List<string> dirtyCoords = new List<string>();

    private void Start()
    {
        transform.LookAt(target);
        transform.parent = target;
        transform.position = target.position + new UnityEngine.Vector3(1, 1, 1) * distance;

        getCoordsFromFile();
    }

    private bool showResolution = false;
    private bool started = false;

    void Update()
    {

        GetComponent<Camera>().orthographicSize = N * parcelSize / 2;

        if (Input.GetKeyDown(KeyCode.Space) && !started)
        {
            Destroy(GameObject.Find("Sun"));
            target.position += new UnityEngine.Vector3(parcelSize / 2, 0, parcelSize / 2);
            GameObject debugView = GameObject.Find("DebugView(Clone)");
            debugView.SetActive(false);
            avatarRenderer.SetActive(false);

            started = true;

            // go to first position
            if (goToFirstPosition)
            {
                DCLCharacterController.i.SetPosition(new UnityEngine.Vector3(parcelSize / 2, 0, parcelSize / 2));
            }
            GoToNextParcel();
        }

        // print screen resolution if P is pressed
        if (Input.GetKeyDown(KeyCode.P))
        {
            showResolution = !showResolution;
        }
        if (showResolution)
            Debug.Log("Screen resolution: " + Screen.width + "x" + Screen.height);


        transform.position = target.position + new UnityEngine.Vector3(0, 1, 0) * distance;
        transform.LookAt(target);
    }


    private int currentX = 0, currentY = 0;

    private UnityEngine.Vector3 currentPosition;
    void GoToNextParcel()
    {
        //Grab first position from array
        string[] splittedCoords = dirtyCoords[0].Split(',');
        //Convert to currentX and currentY positions
        currentX = int.Parse(splittedCoords[0]);
        currentY = int.Parse(splittedCoords[1]);
        currentPosition = new UnityEngine.Vector3(currentX * parcelSize, flyingHeight, currentY * parcelSize);

        dirtyCoords.RemoveAt(0);

        if (Mathf.Abs(currentX) > 150f)
        {
            Debug.Log("Reached end of the world at " + currentPosition);
            Application.Quit();
            return;
        }

        string fullScreenshotPath = GetCurrentScreenshotPath();
        // check if screenshot was already taken and skip if so
        if (System.IO.File.Exists(fullScreenshotPath))
        {
            Debug.Log("Screenshot already exists for coordinate (" + currentX + "," + currentY + "), skipping...");
            Invoke("GoToNextParcel", 0.01f);
            return;
        }

        Debug.Log("now moving to position: (" + currentX + ", " + currentY + ")");
        // move player to current position
        UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(parcelSize / 2, 0, parcelSize / 2) + currentPosition;
        UnityEngine.Vector3 delta = DCLCharacterController.i.characterPosition.worldPosition - targetPosition;
        if (delta.magnitude > 0.1f)
        {
            DCLCharacterController.i.SetPosition(targetPosition);
        }

        waitStartTime = Time.time;
        Invoke("WaitForScreenshot", 2f);
    }

    //private int layer = 1;
    //private int leg = 0;
    //    void GoToNextParcel()
    //    {
    //        // calculate next position
    //        // spiral around (0,0) through the grid
    //        // layer 0 -> (0,0)
    //        // layer 1 -> (1,0), (1,1), (0,1), (-1,1), (-1,0), (-1,-1), (0,-1), (1,-1)
    //        // layer 2 -> (2,0), (2,1), (2,2), (1,2), (0,2), (-1,2), (-2,2), (-2,1), (-2,0), (-2,-1), (-2,-2), (-1,-2), (0,-2), (1,-2), (2,-2), (2,-1)
    //        // etc...
    //        switch (leg)
    //        {
    //            case 0: ++currentX; if (currentX == layer) ++leg; break;
    //            case 1: ++currentY; if (currentY == layer) ++leg; break;
    //            case 2: --currentX; if (-currentX == layer) ++leg; break;
    //            case 3: --currentY; if (-currentY == layer) { leg = 0; ++layer; } break;
    //        }
    //        currentPosition = new UnityEngine.Vector3(currentX * N * parcelSize, flyingHeight, currentY * N * parcelSize);

    //        if (Mathf.Abs(currentX * N) > 150f)
    //        {
    //            Debug.Log
    //("Reached end of the world at " + currentPosition + ", layer " + layer);
    //            Application.Quit();
    //            return;
    //        }

    //        string fullScreenshotPath = GetCurrentScreenshotPath();
    //        // check if screenshot was already taken and skip if so
    //        if (System.IO.File.Exists(fullScreenshotPath))
    //        {
    //            Debug.Log("Screenshot already exists for coordinate (" + currentX * N + "," + currentY * N + "), skipping...");
    //            Invoke("GoToNextParcel", 0.01f);
    //            return;
    //        }

    //        int pos = Array.IndexOf(txtCoords, currentX * N + "," + currentY * N);
    //        if (pos == -1)
    //        {
    //            Debug.Log("Skipping coords not found in txt file: " + currentX * N + "," + currentY * N);
    //            Invoke("GoToNextParcel", 0.01f);
    //            return;
    //        }

    //        Debug.Log("now moving to position: (" + currentX * N + ", " + currentY * N + ")");
    //        // move player to current position
    //        UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(parcelSize / 2, 0, parcelSize / 2) + currentPosition;
    //        UnityEngine.Vector3 delta = DCLCharacterController.i.characterPosition.worldPosition - targetPosition;
    //        if (delta.magnitude > 0.1f)
    //        {
    //            DCLCharacterController.i.SetPosition(targetPosition);
    //        }

    //        waitStartTime = Time.time;
    //        Invoke("WaitForScreenshot", 2f);
    //    }
    private float waitStartTime = 0f;
    public float waitTimeout = 60f;
    public float waitBeforeScreenshot = 20f;

    void WaitForScreenshot()
    {
        string fullScreenshotPath = GetCurrentScreenshotPath();
        // check if screenshot was already taken
        if (System.IO.File.Exists(fullScreenshotPath))
        {
            Debug.Log("Screenshot already exists for coordinate (" + currentX + "," + currentY + ")");
            GoToNextParcel();
            return;
        }

        // check if waited too long (timeout)
        float waitedTime = Time.time - waitStartTime;
        bool timeoutExpired = waitedTime > waitTimeout;

        bool allScenesLoaded = true;
        // check if all scenes are loaded
        ParcelScene[] scenes = FindObjectsOfType<ParcelScene>();
        foreach (ParcelScene scene in scenes)
        {
            bool importantScene = true;
            // for (int ix = 0; ix < N && !importantScene; ix++) {
            //     for (int iy = 0; iy < N && !importantScene; iy++) {
            //         int wx = currentX*N - Mathf.FloorToInt((N-1)/2) + ix;
            //         int wy = currentY*N - Mathf.FloorToInt((N-1)/2) + iy;
            //         // Debug.Log("\tchecking if current scene is important: " + scene.gameObject.name + " vs (" + wx + ", " + wy + ")");
            //         if (scene.gameObject.name.Contains("(" + wx + ", " + wy + ")")) {
            //             importantScene = true;
            //         }
            //     }
            // }
            if (!importantScene)
            {
                // Debug.Log("Scene " + scene.gameObject.name + " is not important, skipping...");
                continue;
            }
            // Debug.Log("Scene " + scene.gameObject.name + " is important, checking if loaded...");

            if (!scene.gameObject.name.Contains("ready!"))
            {
                Debug.Log(scene.gameObject.name + " is not loaded, waiting...");
                allScenesLoaded = false;
            }
        }

        if (allScenesLoaded || timeoutExpired)
        {
            if (allScenesLoaded)
            {
                Debug.Log("all scenes are loaded, preparing to take screenshot at (" + currentX + "," + currentY + ")");
            }
            if (timeoutExpired && !allScenesLoaded)
            {
                Debug.Log("Timeout waiting for screenshot at coordinate (" + currentX + "," + currentY + ")");
            }
            Invoke("TakeScreenshot", waitBeforeScreenshot);
        }
        else
        {
            Debug.Log("waiting for screenshot at (" + currentX + "," + currentY + ") for " + waitedTime + " seconds");
            Invoke("WaitForScreenshot", 5f);
        }

    }

    private void TakeScreenshot()
    {
        string fullScreenshotPath = GetCurrentScreenshotPath();
        Debug.Log("Taking screenshot at (" + currentX + "," + currentY + ") now!");
        ScreenCapture.CaptureScreenshot(fullScreenshotPath);
        Invoke("GoToNextParcel", 2f);
    }

    private string GetCurrentScreenshotPath()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string name = currentX + "," + currentY + ".png";
        string fullScreenshotPath = desktopPath + "/map/" + name;
        return fullScreenshotPath;
    }

    private void getCoordsFromFile()
    {
        string txtPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/coords.txt";
        var sr = new StreamReader(txtPath);  //Open the file
        var fileContents = sr.ReadToEnd();  //Copy the file contents to a variable
        sr.Close();  //Close the file

        char[] delimiterChars = {';', '\n' };


        string[] estatesArr = fileContents.Split(delimiterChars) ; //estatesArr[0] ej. "-150,20;-150,25"
        Debug.Log(estatesArr.Length);
        foreach (string estateCoordsString in estatesArr)
        {
            var splitted = estateCoordsString.Split(";"[0]);
            foreach (string coordinate in splitted)
            {
                dirtyCoords.Add(coordinate);
            }
        }






        //Debug.Log(estatesArr);
        //Debug.Log(estatesArr[0]);

        //Debug.Log(txtCoords);
        //Debug.Log(fileSplit.Length);
        //Debug.Log(txtCoords[0]);

        //foreach (Array estate in estatesArr)
        //{

        //}

        //resultArray = []
        //fileSplit.forEach {
        //    estate in
        //    estatesArr = estate.split(";")
        //    resultArray.push(estatesArr)
        //}

    }
}
