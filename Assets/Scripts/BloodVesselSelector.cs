using UnityEngine;
using TMPro;

public class BloodVesselSelector : MonoBehaviour
{
    public BloodVesselGenerator generator;
    public Camera mainCamera;
    public TMP_Text injectionText;
    public TMP_Text targetText;

    private enum SelectMode { None, Injection, Target }
    private SelectMode mode = SelectMode.None;

    private GameObject injectionVessel;
    private Vector3 injectionPoint;
    private GameObject targetVessel;
    private Vector3 targetPoint;

    public void SetInjectionMode() => mode = SelectMode.Injection;
    public void SetTargetMode() => mode = SelectMode.Target;

    void Update()
    {
        if (mode == SelectMode.None) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 clickPoint = hit.point;
                GameObject vessel = generator.GetClosestVessel(clickPoint, out Vector3 vesselPoint);
                if (vessel != null)
                {
                    if (mode == SelectMode.Injection)
                    {
                        if (injectionVessel != null)
                            injectionVessel.GetComponent<Renderer>().material.color = Color.red;
                        injectionVessel = vessel;
                        injectionPoint = vesselPoint;
                        injectionVessel.GetComponent<Renderer>().material.color = Color.green;
                        if (injectionText != null)
                            injectionText.text = $"Injection: {injectionPoint}";
                    }
                    else if (mode == SelectMode.Target)
                    {
                        if (targetVessel != null)
                            targetVessel.GetComponent<Renderer>().material.color = Color.red;
                        targetVessel = vessel;
                        targetPoint = vesselPoint;
                        targetVessel.GetComponent<Renderer>().material.color = Color.blue;
                        if (targetText != null)
                            targetText.text = $"Target: {targetPoint}";
                    }
                    mode = SelectMode.None;
                }
            }
        }
    }
} 