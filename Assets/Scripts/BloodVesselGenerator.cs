using UnityEngine;
using System.Collections.Generic;

public class BloodVesselGenerator : MonoBehaviour
{
    public int vesselCount = 10;
    public Vector3 areaSize = new Vector3(20, 20, 20);
    public float vesselRadius = 0.2f;
    public float vesselLengthMin = 2f;
    public float vesselLengthMax = 6f;
    public Material vesselMaterial;

    private List<GameObject> vessels = new List<GameObject>();

    private class Node
    {
        public Vector3 position;
        public Node(Vector3 pos) { position = pos; }
    }

    private List<Node> nodes = new List<Node>();

    public void CreateVessels()
    {
        ResetVessels();
        nodes.Clear();
        // اولین نود را تصادفی بساز
        Node firstNode = new Node(new Vector3(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            Random.Range(-areaSize.y / 2, areaSize.y / 2),
            Random.Range(-areaSize.z / 2, areaSize.z / 2)
        ));
        nodes.Add(firstNode);

        for (int i = 0; i < vesselCount; i++)
        {
            Node startNode;
            // با احتمال 30 درصد به یک نود قبلی وصل شو (تلاقی)
            if (nodes.Count > 1 && Random.value < 0.3f)
            {
                int idx = Random.Range(0, nodes.Count);
                startNode = nodes[idx];
            }
            else
            {
                startNode = nodes[nodes.Count - 1];
            }

            Vector3 endPos = startNode.position + Random.onUnitSphere * Random.Range(vesselLengthMin, vesselLengthMax);
            Node endNode = new Node(endPos);
            nodes.Add(endNode);

            // ساخت رگ بین startNode و endNode
            GameObject vessel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vessel.transform.position = (startNode.position + endNode.position) / 2;
            vessel.transform.up = (endNode.position - startNode.position).normalized;
            vessel.transform.localScale = new Vector3(vesselRadius, (endNode.position - startNode.position).magnitude / 2, vesselRadius);

            if (vesselMaterial != null)
                vessel.GetComponent<Renderer>().material = vesselMaterial;
            else
                vessel.GetComponent<Renderer>().material.color = Color.red;

            vessel.name = $"BloodVessel_{i}";
            vessel.transform.parent = this.transform;
            vessel.tag = "BloodVessel";
            vessels.Add(vessel);
        }
    }

    public void ResetVessels()
    {
        foreach (var v in vessels)
            Destroy(v);
        vessels.Clear();
    }

    public GameObject GetClosestVessel(Vector3 point, out Vector3 closestPoint)
    {
        float minDist = float.MaxValue;
        GameObject closestVessel = null;
        closestPoint = Vector3.zero;

        foreach (var vessel in vessels)
        {
            Vector3 center = vessel.transform.position;
            Vector3 dir = vessel.transform.up;
            float halfLen = vessel.transform.localScale.y;

            Vector3 start = center - dir * halfLen;
            Vector3 end = center + dir * halfLen;
            Vector3 proj = ProjectPointOnLineSegment(start, end, point);

            float dist = Vector3.Distance(proj, point);
            if (dist < minDist)
            {
                minDist = dist;
                closestVessel = vessel;
                closestPoint = proj;
            }
        }
        return closestVessel;
    }

    private Vector3 ProjectPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ap = p - a;
        Vector3 ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / ab.sqrMagnitude);
        return a + ab * t;
    }
}