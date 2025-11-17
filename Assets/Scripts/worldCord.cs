using UnityEngine;
using UnityEngine.UI;

public class worldCord : MonoBehaviour
{
    public Vector3 worldPosition = new Vector3(16.5f, 0f, -10f);
    private RectTransform rectTransform;
    private Camera mainCam;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null) return;

        // Convert the world position to screen coordinates
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPosition);

    }
}
