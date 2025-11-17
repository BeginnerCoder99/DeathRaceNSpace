using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    static private FollowCam S;
    static public GameObject POI;
    public GameObject defaultTarget;
    [Header("Inscribed")]
    public float easing = 0.05f;
    public Vector2 minXY = Vector2.zero;

    [Header("Dynamic")]
    public float camZ;

    void Awake()
    {
        S = this;
        camZ = this.transform.position.z;
    }

    void LateUpdate()
    {
        Vector3 destination = Vector3.zero;

        if (POI != null)
        {
            destination = POI.transform.position;
        }
        else if (defaultTarget != null)
        {
            destination = defaultTarget.transform.position;
        }
        

        destination.x = Mathf.Max(minXY.x, destination.x);

        destination = Vector3.Lerp(transform.position, destination, easing);
        destination.z = camZ;
        transform.position = destination;
        //Camera.main.orthographicSize = destination.y + 10;
    }

    public static void FollowDefault (GameObject go)
    {
        POI = null;
        if (S != null)
        {
            S.defaultTarget = go;
        }
    }
 

}
