using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{

    private static Settings _instance;

    public static Settings Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 90;

    }


    // Start is called before the first frame update
    void Start()
    {
        //InstatiatePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
