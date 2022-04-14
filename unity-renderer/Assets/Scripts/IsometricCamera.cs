using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricCamera : MonoBehaviour
{
    public GameObject target;
    public Vector3 displacement = new Vector3(-1,1,-1);
    public float distance = 50f;

    public GameObject[] units;
    public int selected = 0;
    private GameObject selectedUnit;
    public GameObject selectionIndicator;
    public GameObject movementQueueIndicator;

    private bool started = false;

    public Material uiMaterial;

    void Start()
    {
        ChangeSelection();
    }

    void ChangeSelection()
    {
        selected = (selected + 1) % units.Length;
        if (selectedUnit)
        {
            selectedUnit.GetComponent<UnitController>().movementQueueIndicator = null;
        }
        selectedUnit = units[selected];

        GameObject avatarRenderer = GameObject.Find("AvatarRenderer");
        // give selection indicator to newly selected object
        if (selectionIndicator) {
            selectionIndicator.transform.parent = avatarRenderer.transform;
            selectionIndicator.transform.localPosition = Vector3.zero;
        }

        // give it the movement queue indicator
        if (movementQueueIndicator) {
            movementQueueIndicator.transform.parent = avatarRenderer.transform;
            movementQueueIndicator.transform.localPosition = Vector3.zero;
            selectedUnit.GetComponent<UnitController>().movementQueueIndicator = 
                movementQueueIndicator.GetComponent<MovementQueueIndicator>();
        }

    }

    void LateUpdate()
    {
        transform.position = target.transform.position + distance * displacement;
        transform.LookAt(target.transform);

        if (Input.GetKeyDown(KeyCode.Space) && !started)
        {
            GameObject debugView = GameObject.Find("DebugView(Clone)");
            GameObject avatarRenderer = GameObject.Find("AvatarRenderer");
            debugView.SetActive(false);
            //avatarRenderer.SetActive(false);

            started = true;
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            //GetComponent<CinemachineBrain>().enabled = !GetComponent<CinemachineBrain>().enabled;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {

                UnitController pc = selectedUnit.GetComponent<UnitController>();
                pc.MoveTo(hit.point);

                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                indicator.transform.localScale = Vector3.one * 0.5f;
                
                indicator.transform.position = hit.point;
                ShrinkAndSuicide shrinker = indicator.AddComponent<ShrinkAndSuicide>() as ShrinkAndSuicide;
                indicator.GetComponent<Renderer>().material = uiMaterial;
            }
        }


    }
}
