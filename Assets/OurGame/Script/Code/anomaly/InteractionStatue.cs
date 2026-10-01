using UnityEngine;

public class InteractionStatue : MonoBehaviour
{
    [Header("Anomaly Test")]
    [SerializeField] private float anomalyDelay = 5f;
    [SerializeField] private float anomalyRotation = 180f;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float holdDuration = 1f;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 180f;

    private Transform player;

    private Quaternion normalRotation;
    private Quaternion anomalyRotationTarget;

    private bool anomalyActive = false;
    private bool fixedAnomaly = false;

    private float holdTimer = 0f;

    private void Start()
    {
        // จำตำแหน่งปกติของรูปปั้น
        normalRotation = transform.rotation;

        // สร้างตำแหน่งผิดปกติ
        anomalyRotationTarget =
            normalRotation * Quaternion.Euler(0f, anomalyRotation, 0f);

        // หา Player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // เริ่มนับเวลา
        Invoke(nameof(ActivateAnomaly), anomalyDelay);
    }

    private void Update()
    {
        // ถ้าไม่มี Player ให้ลองหาใหม่
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            return;
        }

        // ถ้ายังไม่เกิด Anomaly หรือแก้ไปแล้ว
        if (!anomalyActive || fixedAnomaly)
            return;

        float distance =
            Vector3.Distance(player.position, transform.position);

        // ถ้าอยู่นอกระยะ
        if (distance > interactionDistance)
        {
            holdTimer = 0f;
            return;
        }

        // กด E ค้าง
        if (Input.GetKey(interactionKey))
        {
            holdTimer += Time.deltaTime;

            // หมุนกลับอย่างต่อเนื่อง
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                normalRotation,
                rotateSpeed * Time.deltaTime
            );

            // กดค้างครบเวลาที่กำหนด
            if (holdTimer >= holdDuration)
            {
                FixAnomaly();
            }
        }
        else
        {
            // ปล่อย E ก่อนเวลา
            holdTimer = 0f;
        }
    }

    private void ActivateAnomaly()
    {
        if (fixedAnomaly)
            return;

        anomalyActive = true;

        // หมุนรูปปั้นไปตำแหน่งผิดปกติทันที
        transform.rotation = anomalyRotationTarget;

        Debug.Log(
            gameObject.name +
            " : Interaction Anomaly Activated!"
        );
    }

    private void FixAnomaly()
    {
        fixedAnomaly = true;
        anomalyActive = false;
        holdTimer = 0f;

        // ล็อกกลับตำแหน่งปกติ
        transform.rotation = normalRotation;

        Debug.Log(
            gameObject.name +
            " : Interaction Anomaly Fixed!"
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );
    }
}