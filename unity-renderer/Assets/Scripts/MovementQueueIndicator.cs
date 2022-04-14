using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementQueueIndicator : MonoBehaviour
{
    
    public float lineWidth = 0.2f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        
    }

    private void LateUpdate()
    {
        
    }

    public void SetupPath(Vector3[] points)
    {
        lineRenderer.widthMultiplier = lineWidth;
        if (points == null)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        lineRenderer.positionCount = points.Length;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = points[i];
            pos.y = 0.1f;
            lineRenderer.SetPosition(i, pos);
        }
    }
}