using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapperCamera : MonoBehaviour
{
    public Transform target;

    public GameObject avatarRenderer;

    public int N = 5;

    private float parcelSize = 16f;
    private float distance = 50f;

    private void Start() {
        transform.LookAt(target);
        transform.parent = target;


        transform.position = target.position + new Vector3(1,1,1) * distance;
    }
    void Update()
    {

        GetComponent<Camera>().orthographicSize = N*parcelSize/2;
        // mouse wheel changes N up or down
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            N++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            N--;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            target.position += new Vector3(parcelSize/2, 0, parcelSize/2);
            GameObject debugView = GameObject.Find("DebugView(Clone)");
            debugView.SetActive(false);
            avatarRenderer.SetActive(false);
            GameObject.Find("_ExploreV2").SetActive(false);
            MapperTick();
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            target.position += new Vector3(-parcelSize, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            target.position += new Vector3(parcelSize, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            target.position += new Vector3(0, 0, parcelSize);
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            target.position += new Vector3(0, 0, -parcelSize);
        }
        
        transform.position = target.position + new Vector3(0,1,0) * distance;
        transform.LookAt(target);
    }


    private int currentX = 0, currentY = 0;
    private Vector3 currentPosition;
    private int layer = 1;
    private int leg = 0;
    void MapperTick()
    {
        Debug.Log("currentX: " + currentX + " currentY: " + currentY + " layer: " + layer + " leg: " + leg + " currentPosition: " + currentPosition);
        // move player to current position
        DCLCharacterController.i.SetPosition(new Vector3(parcelSize/2, 0, parcelSize/2) + currentPosition);

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
        currentPosition = new Vector3(currentX*N*parcelSize, 0, currentY*N*parcelSize);

        Invoke("MapperTick", 5f);
    }

}
