using UnityEngine;

public class RotateQuestMarker : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0,50 * Time.deltaTime,0, Space.World);
    }
}
