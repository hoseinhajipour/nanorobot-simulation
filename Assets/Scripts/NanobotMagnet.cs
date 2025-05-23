
using UnityEngine;

public class NanobotMagnet : MonoBehaviour
{
    public Transform magnetTarget;
    public float magneticForce = 5f;
    public float magneticRange = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 3f;
    }

    void FixedUpdate()
    {
        if (magnetTarget == null) return;

        Vector3 direction = magnetTarget.position - transform.position;
        float distance = direction.magnitude;

        if (distance < magneticRange)
        {
            Vector3 force = direction.normalized * magneticForce * (1f - distance / magneticRange);
            rb.AddForce(force);
        }
    }
}
