using UnityEngine;

public class AnomalyTest : MonoBehaviour
{
    public AnomalyCounter anomalyCounter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            anomalyCounter.AnomalySpawned();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            anomalyCounter.AnomalyRemoved();
        }
    }
}