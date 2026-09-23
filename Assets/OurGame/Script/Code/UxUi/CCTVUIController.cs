using UnityEngine;

public class CCTVUIController : MonoBehaviour
{
    [Header("References")]
    public CCTVManager cctvManager;
    public GameObject cctvUI;

    private void Awake()
    {
        if (cctvManager == null)
        {
            cctvManager = FindFirstObjectByType<CCTVManager>();
        }

        if (cctvUI != null)
        {
            cctvUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (cctvManager == null || cctvUI == null)
            return;

        cctvUI.SetActive(cctvManager.isWatchingCCTV);
    }
}