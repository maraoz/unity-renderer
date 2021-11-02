using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Controllers;

public class MapperCamera : MonoBehaviour
{
    public Transform target;

    public GameObject avatarRenderer;

    public int N = 5;

    public float flyingHeight = 10f;

    private float parcelSize = 16f;
    private float distance = 50f;

    private void Start() {
        transform.LookAt(target);
        transform.parent = target;


        transform.position = target.position + new Vector3(1,1,1) * distance;
    }

    private bool showResolution = false;
    private bool started = false;

    void Update()
    {

        GetComponent<Camera>().orthographicSize = N*parcelSize/2;

        if (Input.GetKeyDown(KeyCode.Space) && !started)
        {
            target.position += new Vector3(parcelSize/2, 0, parcelSize/2);
            GameObject debugView = GameObject.Find("DebugView(Clone)");
            debugView.SetActive(false);
            avatarRenderer.SetActive(false);
            GameObject.Find("_ExploreV2").SetActive(false);

            started = true;

            // go to first position
            DCLCharacterController.i.SetPosition(new Vector3(parcelSize/2, 0, parcelSize/2));
            WaitForScreenshot();
        }

        // print screen resolution if P is pressed
        if (Input.GetKeyDown(KeyCode.P))
        {
            showResolution = !showResolution;
        }
        if (showResolution)
            Debug.Log("Screen resolution: " + Screen.width + "x" + Screen.height);


        transform.position = target.position + new Vector3(0,1,0) * distance;
        transform.LookAt(target);
    }


    private int currentX = 0, currentY = 0;
    private Vector3 currentPosition;
    private int layer = 1;
    private int leg = 0;
    void GoToNextParcel()
    {
        // calculate next position
        // spiral around (0,0) through the grid
        // layer 0 -> (0,0)
        // layer 1 -> (1,0), (1,1), (0,1), (-1,1), (-1,0), (-1,-1), (0,-1), (1,-1)
        // layer 2 -> (2,0), (2,1), (2,2), (1,2), (0,2), (-1,2), (-2,2), (-2,1), (-2,0), (-2,-1), (-2,-2), (-1,-2), (0,-2), (1,-2), (2,-2), (2,-1)
        // etc...
        switch (leg)
        {
            case 0: ++currentX; if(currentX  == layer)  ++leg; break;
            case 1: ++currentY; if(currentY  == layer)  ++leg; break;
            case 2: --currentX; if(-currentX == layer)  ++leg; break;
            case 3: --currentY; if(-currentY == layer) { leg = 0; ++layer; } break;
        }
        currentPosition = new Vector3(currentX*N*parcelSize, flyingHeight, currentY*N*parcelSize);

        if (Mathf.Abs(currentPosition.x) > 150f) {
            Debug.Log("Reached end of the world at " + currentPosition + ", layer " + layer);
            Application.Quit();
        }

        string fullScreenshotPath = GetCurrentScreenshotPath();
        // check if screenshot was already taken and skip if so
        if (System.IO.File.Exists(fullScreenshotPath))
        {
            Debug.Log("Screenshot already exists for coordinate (" + currentX * N + "," + currentY * N + "), skipping...");
            GoToNextParcel();
            return;
        }

        Debug.Log("now moving to position: (" + currentX*N + ", " + currentY*N + ")");
        // move player to current position
        DCLCharacterController.i.SetPosition(new Vector3(parcelSize/2, 0, parcelSize/2) + currentPosition);

        waitStartTime = Time.time;
        Invoke("WaitForScreenshot", 3f);
    }

    private float waitStartTime = 0f;
    public float waitTimeout = 60f;

    void WaitForScreenshot()
    {
        string fullScreenshotPath = GetCurrentScreenshotPath();
        // check if screenshot was already taken
        if (System.IO.File.Exists(fullScreenshotPath))
        {
            Debug.Log("Screenshot already exists for coordinate (" + currentX * N + "," + currentY * N + ")");
            GoToNextParcel();
            return;
        }

        // check if waited too long (timeout)
        float waitedTime = Time.time - waitStartTime;
        bool timeoutExpired = waitedTime > waitTimeout;
        
        bool allScenesLoaded = true;
        // check if all scenes are loaded
        ParcelScene[] scenes = FindObjectsOfType<ParcelScene>();
        foreach (ParcelScene scene in scenes) {
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
            if (!importantScene) {
                // Debug.Log("Scene " + scene.gameObject.name + " is not important, skipping...");
                continue;
            }
            // Debug.Log("Scene " + scene.gameObject.name + " is important, checking if loaded...");

            if (!scene.gameObject.name.Contains("ready!")) {
                Debug.Log(scene.gameObject.name + " is not loaded, waiting...");
                allScenesLoaded = false;
            }
        }

        if (allScenesLoaded || timeoutExpired) {
            if (allScenesLoaded) {
                Debug.Log("all scenes are loaded, preparing to take screenshot at (" + currentX * N + "," + currentY * N + ")");
            }
            if (timeoutExpired && !allScenesLoaded) {
                Debug.Log("Timeout waiting for screenshot at coordinate (" + currentX * N + "," + currentY * N + ")");
            }
            Invoke("TakeScreenshot", 5f);
        }
        else
        {
            Debug.Log("waiting for screenshot at (" + currentX * N + "," + currentY * N + ") for " + waitedTime + " seconds");
            Invoke("WaitForScreenshot", 5f);
        }

    }

    private void TakeScreenshot()
    {
        string fullScreenshotPath = GetCurrentScreenshotPath();
        Debug.Log("Taking screenshot at (" + currentX * N + "," + currentY * N + ") now!");
        ScreenCapture.CaptureScreenshot(fullScreenshotPath);
        Invoke("GoToNextParcel", 2f);
    }

    private string GetCurrentScreenshotPath()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string name = currentX * N + "," + currentY * N + ".png";
        string fullScreenshotPath = desktopPath + "/map/" + name;
        return fullScreenshotPath;
    }
}
