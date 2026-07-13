using UnityEngine;
using UnityEngine.UI;

public class KinectBackgroundDisplay : MonoBehaviour
{
    private RawImage rawImage;
    private KinectManager kinectManager;

    void Start()
    {
        rawImage = GetComponent<RawImage>();

        // Find KinectManager directly
        kinectManager = FindFirstObjectByType<KinectManager>();

        Debug.Log("Found KinectManager: " + (kinectManager != null));
    }

    void Update()
    {
        if (kinectManager == null)
        {
            Debug.Log("KinectManager = NULL");
            return;
        }

        Debug.Log("Initialized: " + kinectManager.IsInitialized());

        Texture tex = kinectManager.GetUsersClrTex();

        if (tex == null)
        {
            Debug.Log("Color texture = NULL");
            return;
        }

        rawImage.texture = tex;
        Debug.Log("Color texture assigned");
    }
}