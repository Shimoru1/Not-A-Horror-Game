using UnityEngine;

public class CCTVAmmoSystem : MonoBehaviour
{
    [Header("Camera Count")]
    public int cameraCount = 5;

    [Header("Ammo Settings")]
    public int magazineSize = 30;
    public int startingReserveAmmo = 0;

    private int[] currentAmmo;
    private int[] reserveAmmo;

    private void Awake()
    {
        currentAmmo = new int[cameraCount];
        reserveAmmo = new int[cameraCount];

        for (int i = 0; i < cameraCount; i++)
        {
            currentAmmo[i] = magazineSize;
            reserveAmmo[i] = startingReserveAmmo;
        }
    }

    public bool TryUseBullet(int cameraIndex)
    {
        if (!IsValidCamera(cameraIndex))
            return false;

        if (currentAmmo[cameraIndex] <= 0)
            return false;

        currentAmmo[cameraIndex]--;
        return true;
    }

    public void Reload(int cameraIndex)
    {
        if (!IsValidCamera(cameraIndex))
            return;

        int needed = magazineSize - currentAmmo[cameraIndex];

        if (needed <= 0)
            return;

        int amountToReload = Mathf.Min(needed, reserveAmmo[cameraIndex]);

        currentAmmo[cameraIndex] += amountToReload;
        reserveAmmo[cameraIndex] -= amountToReload;
    }

    public void AddReserveAmmo(int cameraIndex, int amount)
    {
        if (!IsValidCamera(cameraIndex))
            return;

        reserveAmmo[cameraIndex] += amount;

        Debug.Log(
            "Camera " + (cameraIndex + 1) +
            " ได้กระสุนสำรอง +" + amount +
            " | Reserve = " + reserveAmmo[cameraIndex]
        );
    }

    public int GetCurrentAmmo(int cameraIndex)
    {
        if (!IsValidCamera(cameraIndex))
            return 0;

        return currentAmmo[cameraIndex];
    }

    public int GetReserveAmmo(int cameraIndex)
    {
        if (!IsValidCamera(cameraIndex))
            return 0;

        return reserveAmmo[cameraIndex];
    }

    public string GetAmmoText(int cameraIndex)
    {
        if (!IsValidCamera(cameraIndex))
            return "0/0";

        return currentAmmo[cameraIndex] + "/" + reserveAmmo[cameraIndex];
    }

    private bool IsValidCamera(int cameraIndex)
    {
        return cameraIndex >= 0 &&
               cameraIndex < cameraCount;
    }
}