using UnityEngine;

public class CCTVCleaningTarget : MonoBehaviour
{
    [Header("Cleaning Settings")]
    public float maxDirt = 100f;
    public float dirtLevel = 0f;

    [Header("Dirt Increase")]
    public float dirtIncreasePerSecond = 0.5f;

    private void Update()
    {
        if (dirtLevel < maxDirt)
        {
            dirtLevel += dirtIncreasePerSecond * Time.deltaTime;
            dirtLevel = Mathf.Clamp(dirtLevel, 0f, maxDirt);
        }
    }

    public bool IsDirty()
    {
        return dirtLevel > 0f;
    }

    public float GetDirtPercent()
    {
        return dirtLevel / maxDirt;
    }

    public void CleanCamera()
    {
        dirtLevel = 0f;

        Debug.Log(gameObject.name + " เช็ดกล้องสะอาดแล้ว!");
    }
}