using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveDistance = 2f;

    public void MoveForward ()
    {
        transform.position += new Vector3(moveDistance, 0, 0);
    }
}
