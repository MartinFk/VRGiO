using UnityEngine;

public class followGyroscope : MonoBehaviour
{
    [SerializeField] private Quaternion baseRotation = new Quaternion(0, 0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        GyroscopeManager.Instance.EnableGyro();
    }

    // Update is called once per frame
    void Update()
    {
        if (GyroscopeManager.Instance.isEnabled())
        {
            transform.localRotation = GyroscopeManager.Instance.GetGyroRotation() * baseRotation;
        }
        else
        {
            if (Input.GetKey("up"))
            {
                transform.Rotate(1, 0, 0, Space.World);
            }

            if (Input.GetKey("down"))
            {
                transform.Rotate(-1, 0, 0, Space.World);
            }
            if (Input.GetKey("left"))
            {
                transform.Rotate(0, -1, 0, Space.World);
            }

            if (Input.GetKey("right"))
            {
                transform.Rotate(0, 1, 0, Space.World);
            }
        }

    }
}
