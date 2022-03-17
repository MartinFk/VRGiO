using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cameraFeed : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam; //Can be used as a normal texture
    private Texture defaultBackground; //Used to revert to the default background if something goes wrong

    public RawImage background;
    public AspectRatioFitter fit;


    // Start is called before the first frame update
    void Start()
    {
        defaultBackground = background.texture;

        WebCamDevice[] devices = WebCamTexture.devices;

        if(devices.Length == 0)
        {
            Debug.Log("No camera detected.");
            camAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++){
            if (!devices[i].isFrontFacing) //If we are looking for a backfacing camera
            {
                backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if(backCam == null)
        {
            Debug.Log("Unable to find back camera. Using default.");
            backCam = new WebCamTexture(devices[0].name, Screen.width, Screen.height);
        }

        backCam.Play(); //Start using the camera
        background.texture = backCam; //Camera is now displayed as a raw image

        camAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!camAvailable)
        {
            return;
        }

        //float ratio = (float)backCam.width / (float)backCam.height;
        //fit.aspectRatio = ratio;

        //float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
        //background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        //int orient = -backCam.videoRotationAngle;
        //background.rectTransform.localEulerAngles = new Vector3(0,0,orient);
    }
}
