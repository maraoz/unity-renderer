using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    public int vertexCount = 40; // 4 vertices == square
    public float lineWidth = 0.2f;
    public float radius = 10;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    void Update()
    {
        SetupCircle();
    }

    private void SetupCircle()
    {

        float deltaTheta = (2f * Mathf.PI) / vertexCount;
        float theta = 0f;

        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.positionCount = vertexCount+1;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = transform.parent.position + new Vector3(radius * Mathf.Cos(theta), 1f
                , radius * Mathf.Sin(theta));
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }
}