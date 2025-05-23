using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float lookSpeed = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        // Mouse look
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = 0;
        if (Input.GetKey(KeyCode.E)) y += 1;
        if (Input.GetKey(KeyCode.Q)) y -= 1;
        Vector3 move = transform.right * x + transform.forward * z + transform.up * y;
        transform.position += move * movementSpeed * Time.deltaTime;
    }
} 