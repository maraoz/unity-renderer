using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    
    public float speed = 10f;

    public float EPSILON = 0.01f;
    private Vector3 target = Vector3.zero;

    public MovementQueueIndicator movementQueueIndicator;

    public Queue<Vector3> commands = new Queue<Vector3>();
    
    void FixedUpdate()
    {

        if (target != Vector3.zero)
        {
            Vector3 delta = target - transform.position;
            Vector3 movement = delta.normalized * speed * Time.deltaTime;

            GetComponent<CharacterController>().Move(movement);

            //transform.position = transform.position + movement;

            if (delta.magnitude < EPSILON)
            {
                target = Vector3.zero;
            }
        }

        if (target == Vector3.zero && commands.Count > 0)
        {
            Vector3 point = commands.Dequeue();
            target = new Vector3(point.x, transform.position.y, point.z);
            transform.LookAt(target);
        }

        if (movementQueueIndicator)
        {
            if (target != Vector3.zero)
            {
                Vector3[] path = new Vector3[commands.Count + 2];
                Vector3 delta = target - transform.position;
                path[0] = transform.position + delta.normalized * 0.75f;
                path[1] = target;
                if (commands.Count > 0)
                {
                    Vector3[] commandsAsArray = commands.ToArray();
                    Array.Copy(commandsAsArray, 0, path, 2, commands.Count);
                }
                movementQueueIndicator.SetupPath(path);
            } else
            {
                movementQueueIndicator.SetupPath(null);
            }
        }
        

    }

    public void MoveTo(Vector3 point)
    {
        if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            target = Vector3.zero;
            commands.Clear();
        }
        commands.Enqueue(point);
    }
}