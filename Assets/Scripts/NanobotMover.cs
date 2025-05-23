using UnityEngine;
using UnityEngine.UI;

// این کلاس برای حرکت یک نانوبات منفرد استفاده می‌شود
// برای شبیه‌سازی چندین نانوبات، از کلاس NanobotSimulation استفاده کنید
public class NanobotMover : MonoBehaviour
{
    public BloodVesselSelector selector;
    public GameObject nanobotPrefab;
    public float moveSpeed = 2f;
    public Button startMoveButton;
    public Button resetMoveButton;

    private GameObject nanobotInstance;
    private bool isMoving = false;
    private Vector3 startPoint;
    private Vector3 endPoint;

    void Start()
    {
        if (startMoveButton != null)
            startMoveButton.onClick.AddListener(StartMove);
        if (resetMoveButton != null)
            resetMoveButton.onClick.AddListener(ResetMove);
    }
    
    void OnDestroy()
    {
        // پاکسازی لیسنرها
        if (startMoveButton != null)
            startMoveButton.onClick.RemoveListener(StartMove);
        if (resetMoveButton != null)
            resetMoveButton.onClick.RemoveListener(ResetMove);
    }

    void Update()
    {
        if (isMoving && nanobotInstance != null)
        {
            float step = moveSpeed * Time.deltaTime;
            nanobotInstance.transform.position = Vector3.MoveTowards(nanobotInstance.transform.position, endPoint, step);

            if (Vector3.Distance(nanobotInstance.transform.position, endPoint) < 0.01f)
                isMoving = false;
        }
    }

    public void StartMove()
    {
        if (selector == null) return;
        Vector3 injection = selectorInjectionPoint();
        Vector3 target = selectorTargetPoint();
        if (injection == Vector3.zero || target == Vector3.zero) return;

        if (nanobotInstance == null)
            nanobotInstance = Instantiate(nanobotPrefab, injection, Quaternion.identity);
        else
            nanobotInstance.transform.position = injection;

        startPoint = injection;
        endPoint = target;
        isMoving = true;
    }

    public void ResetMove()
    {
        isMoving = false;
        if (nanobotInstance != null)
            Destroy(nanobotInstance);
    }

    // Helper methods to access private points in selector
    private Vector3 selectorInjectionPoint()
    {
        var field = selector.GetType().GetField("injectionPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (Vector3)field.GetValue(selector) : Vector3.zero;
    }
    private Vector3 selectorTargetPoint()
    {
        var field = selector.GetType().GetField("targetPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (Vector3)field.GetValue(selector) : Vector3.zero;
    }
}