using UnityEngine;
using TMPro;

public class CCTVCleaningInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public Camera playerCamera;
    public float interactionDistance = 3f;

    [Header("Layer")]
    public LayerMask cameraLayer;

    [Header("UI")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;

    [Header("Cleaning Manager")]
    public CCTVCleaningManager cleaningManager;

    private CCTVCleaningTarget currentTarget;

    private void Update()
    {
        // Do not show interaction UI while cleaning
        if (CCTVCleaningManager.IsCleaning)
        {
            HideInteraction();
            return;
        }

        CheckCamera();

        if (currentTarget != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCleaning();
            }
        }
    }

    private void CheckCamera()
    {
        currentTarget = null;

        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, cameraLayer))
        {
            CCTVCleaningTarget target =
                hit.collider.GetComponent<CCTVCleaningTarget>();

            if (target == null)
            {
                target = hit.collider.GetComponentInParent<CCTVCleaningTarget>();
            }

            if (target != null && target.IsDirty())
            {
                currentTarget = target;

                ShowInteraction();

                return;
            }
        }

        HideInteraction();
    }

    private void StartCleaning()
    {
        if (cleaningManager == null)
            return;

        if (currentTarget == null)
            return;

        cleaningManager.StartCleaning(currentTarget);

        HideInteraction();
    }

    private void ShowInteraction()
    {
        if (interactionUI != null)
            interactionUI.SetActive(true);

        if (interactionText != null)
        {
            int dirtPercent =
                Mathf.RoundToInt(currentTarget.GetDirtPercent() * 100f);

            interactionText.text =
                "Press F to Clean Camera\nDirt: " + dirtPercent + "%";
        }
    }

    private void HideInteraction()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
}