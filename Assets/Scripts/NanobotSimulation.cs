using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class NanobotSimulation : MonoBehaviour
{
    public BloodVesselSelector selector;
    public GameObject nanobotPrefab;
    public int nanobotCount = 100;
    public float moveSpeed = 2f;
    public float randomMovementRange = 1f;
    public float targetDetectionRadius = 0.5f;
    public float communicationRadius = 5f;
    
    private List<GameObject> nanobots = new List<GameObject>();
    private Vector3 injectionPoint;
    private Vector3 targetPoint;
    private bool simulationRunning = false;
    private int nanobotsReachedTarget = 0;

    // A* Pathfinding
    public BloodVesselGenerator bloodVesselGenerator; // Assign in Editor or find
    private List<Vector3> masterOptimalPath = new List<Vector3>();
    
    // نانوبات‌هایی که هدف را پیدا کرده‌اند
    private List<GameObject> informedNanobots = new List<GameObject>();
    
    // سیستم انتخاب دستی مسیر
    private bool manualPathMode = false;
    private List<Vector3> manualPath = new List<Vector3>();
    private GameObject pathIndicatorPrefab;
    private List<GameObject> pathIndicators = new List<GameObject>();
    private Material pathMaterial;
    private Color normalPathColor = Color.yellow;
    private Color selectedPathColor = Color.green;
    
    void Awake()
    {
        // ایجاد پریفب برای نشانگرهای مسیر
        CreatePathIndicatorPrefab();
    }
    
    // ایجاد پریفب برای نشانگرهای مسیر
    private void CreatePathIndicatorPrefab()
    {
        pathIndicatorPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pathIndicatorPrefab.name = "PathIndicator";
        pathIndicatorPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        // ایجاد متریال برای نشانگرهای مسیر
        pathMaterial = new Material(Shader.Find("Standard"));
        pathMaterial.color = normalPathColor;
        
        // اضافه کردن متریال به نشانگر
        Renderer renderer = pathIndicatorPrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = pathMaterial;
        }
        
        // غیرفعال کردن کولایدر
        Collider collider = pathIndicatorPrefab.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // مخفی کردن پریفب
        pathIndicatorPrefab.SetActive(false);
    }
    
    public void StartSimulation()
    {
        if (selector == null)
        {
            Debug.LogError("Selector is not assigned in NanobotSimulation.");
            return;
        }

        if (bloodVesselGenerator == null)
        {
            bloodVesselGenerator = FindObjectOfType<BloodVesselGenerator>();
            if (bloodVesselGenerator == null)
            {
                Debug.LogError("BloodVesselGenerator not found in scene or assigned.");
                return;
            }
        }
        
        // دریافت نقاط تزریق و هدف
        injectionPoint = GetInjectionPoint();
        targetPoint = GetTargetPoint();
        
        if (injectionPoint == Vector3.zero || targetPoint == Vector3.zero)
        {
            Debug.LogWarning("نقاط تزریق یا هدف تنظیم نشده‌اند!");
            return;
        }

        masterOptimalPath.Clear();
        List<BloodVesselGenerator.Node> vesselGraph = bloodVesselGenerator.GetVesselGraph();

        if (vesselGraph == null || vesselGraph.Count == 0)
        {
            Debug.LogWarning("Vessel graph is empty. A* pathfinding cannot proceed.");
        }
        else
        {
            BloodVesselGenerator.Node startNode = bloodVesselGenerator.GetClosestNode(injectionPoint);
            BloodVesselGenerator.Node targetNode = bloodVesselGenerator.GetClosestNode(targetPoint);

            if (startNode == null || targetNode == null)
            {
                Debug.LogError("Start or target node for A* not found. Ensure vessels are generated and points are within reach.");
            }
            else
            {
                List<BloodVesselGenerator.Node> nodePath = AStarPathfinder.FindPath(startNode, targetNode);
                if (nodePath != null && nodePath.Count > 0)
                {
                    foreach (BloodVesselGenerator.Node node in nodePath)
                    {
                        masterOptimalPath.Add(node.position);
                    }
                    // Log masterOptimalPath details
                    if (masterOptimalPath.Count > 0)
                    {
                        string pathLog = $"Successfully calculated A* path with {masterOptimalPath.Count} points.\n";
                        if (masterOptimalPath.Count <= 10)
                        {
                            for(int i=0; i < masterOptimalPath.Count; i++) pathLog += $"P{i}: {masterOptimalPath[i]}\n";
                        }
                        else
                        {
                            for(int i=0; i < 5; i++) pathLog += $"P{i}: {masterOptimalPath[i]}\n";
                            pathLog += "...\n";
                            for(int i=masterOptimalPath.Count - 5; i < masterOptimalPath.Count; i++) pathLog += $"P{i}: {masterOptimalPath[i]}\n";
                        }
                        Debug.Log(pathLog);
                    }
                    else
                    {
                         Debug.LogWarning("A* pathfinding returned an empty path.");
                    }
                }
                else
                {
                    Debug.LogWarning("A* pathfinding failed to find a path (AStarPathfinder.FindPath returned null). Nanobots may revert to old behavior or fail.");
                }
            }
            else // Added specific logging for null start/target nodes
            {
                Debug.LogError("A* pathfinding failed: startNode or targetNode is null. Cannot calculate path.");
            }
        }
        else // Added specific logging for empty graph
        {
            Debug.LogWarning("A* pathfinding cannot proceed: Blood vessel graph is empty or not available.");
        }
        
        // پاکسازی نانوبات‌های قبلی
        ResetSimulation();
        
        // ایجاد نانوبات‌ها
        for (int i = 0; i < nanobotCount; i++)
        {
            // ایجاد نانوبات با کمی انحراف تصادفی از نقطه تزریق
            Vector3 spawnPosition = injectionPoint + Random.insideUnitSphere * 0.2f;
            GameObject nanobot = Instantiate(nanobotPrefab, spawnPosition, Quaternion.identity);
            nanobot.name = $"Nanobot_{i}";
            nanobot.transform.parent = transform;
            
            // اضافه کردن کامپوننت NanobotBehavior به نانوبات
            NanobotBehavior behavior = nanobot.AddComponent<NanobotBehavior>();
            behavior.simulation = this;
            behavior.targetPoint = targetPoint;
            behavior.moveSpeed = moveSpeed;
            behavior.randomMovementRange = randomMovementRange;
            
            if (masterOptimalPath.Count > 0)
            {
                behavior.LearnPathFromOther(new List<Vector3>(masterOptimalPath)); // Give a copy
                behavior.SetInformed(true); // Make them informed to follow the path
                Debug.Log($"Nanobot {nanobot.name} received masterOptimalPath with {masterOptimalPath.Count} points.");
            }
            else if (manualPath.Count > 0) // Fallback to manual path if A* failed but manual exists
            {
                behavior.LearnPathFromOther(manualPath);
                Debug.Log($"Nanobot {nanobot.name} received manualPath with {manualPath.Count} points as A* path was not available.");
                behavior.SetInformed(true);
            }
            
            nanobots.Add(nanobot);
        }
        
        simulationRunning = true;
        nanobotsReachedTarget = 0;
        informedNanobots.Clear(); // Cleared in ResetSimulation, but good to be explicit
        
        // شروع کورتین برای بررسی ارتباط بین نانوبات‌ها
        StartCoroutine(CommunicationCheck());
    }
    
    public void ResetSimulation()
    {
        StopAllCoroutines();
        simulationRunning = false;
        
        foreach (var nanobot in nanobots)
        {
            if (nanobot != null)
                Destroy(nanobot);
        }
        
        nanobots.Clear();
        informedNanobots.Clear();
        nanobotsReachedTarget = 0;
        
        // مسیر دستی را پاک نمی‌کنیم تا بتوان از آن در شبیه‌سازی بعدی استفاده کرد
    }
    
    // فعال/غیرفعال کردن حالت انتخاب دستی مسیر
    public void ToggleManualPathMode()
    {
        manualPathMode = !manualPathMode;
        
        if (manualPathMode)
        {
            Debug.Log("حالت انتخاب دستی مسیر فعال شد. روی نقاط مسیر کلیک کنید.");
            // پاک کردن مسیر قبلی
            ClearManualPath();
        }
        else
        {
            Debug.Log("حالت انتخاب دستی مسیر غیرفعال شد.");
        }
    }
    
    // پاک کردن مسیر دستی
    public void ClearManualPath()
    {
        manualPath.Clear();
        
        // حذف نشانگرهای مسیر
        foreach (var indicator in pathIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        
        pathIndicators.Clear();
    }
    
    // اضافه کردن نقطه به مسیر دستی
    public void AddPointToManualPath(Vector3 point)
    {
        if (!manualPathMode) return;
        
        // اضافه کردن نقطه به مسیر
        manualPath.Add(point);
        
        // ایجاد نشانگر برای نقطه
        GameObject indicator = Instantiate(pathIndicatorPrefab, point, Quaternion.identity);
        indicator.SetActive(true);
        indicator.transform.parent = transform;
        
        // تنظیم رنگ نشانگر
        Renderer renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(pathMaterial);
            renderer.material.color = normalPathColor;
        }
        
        pathIndicators.Add(indicator);
        
        Debug.Log($"نقطه {manualPath.Count} به مسیر دستی اضافه شد.");
    }
    
    // آیا در حالت انتخاب دستی مسیر هستیم
    public bool IsInManualPathMode()
    {
        return manualPathMode;
    }
    
    // دریافت مسیر دستی
    public List<Vector3> GetManualPath()
    {
        return manualPath;
    }
    
    // کورتین برای بررسی ارتباط بین نانوبات‌ها
    private IEnumerator CommunicationCheck()
    {
        while (simulationRunning)
        {
            // لیست موقت برای ذخیره نانوبات‌های جدید آگاه شده
            List<GameObject> newInformedNanobots = new List<GameObject>();
            
            // بررسی ارتباط بین نانوبات‌های آگاه و ناآگاه
            foreach (var informedNanobot in informedNanobots)
            {
                if (informedNanobot == null) continue;
                
                foreach (var nanobot in nanobots)
                {
                    if (nanobot == null || informedNanobots.Contains(nanobot) || newInformedNanobots.Contains(nanobot)) continue;
                    
                    // اگر فاصله کمتر از شعاع ارتباطی باشد، نانوبات جدید را آگاه کن
                    float distance = Vector3.Distance(informedNanobot.transform.position, nanobot.transform.position);
                    if (distance <= communicationRadius)
                    {
                        NanobotBehavior behavior = nanobot.GetComponent<NanobotBehavior>();
                        if (behavior != null)
                        {
                            behavior.SetInformed(true);
                            ShareMasterOptimalPathWithNanobot(behavior); // Share A* path
                            newInformedNanobots.Add(nanobot);
                        }
                    }
                }
            }
            
            // اضافه کردن نانوبات‌های جدید آگاه شده به لیست اصلی
            // informedNanobots.AddRange(newInformedNanobots); // Already added to informedNanobots if SetInformed is true and they reach target. This might be redundant or handled by NanobotReachedTarget.
            // For now, newInformedNanobots ensures we don't try to inform them multiple times within one CommunicationCheck cycle.
            // The main `informedNanobots` list is populated by `NanobotReachedTarget`.

            
            yield return new WaitForSeconds(0.5f); // بررسی هر نیم ثانیه
        }
    }
    
    // اعلام رسیدن یک نانوبات به هدف
    public void NanobotReachedTarget(GameObject nanobot)
    {
        // Check if it's already in informedNanobots to prevent processing multiple times
        // NanobotBehavior's SetInformed might be a better place to add to this list,
        // but this method is specifically for "reached target".
        if (!informedNanobots.Contains(nanobot))
        {
            informedNanobots.Add(nanobot); // Add to general informed list
            nanobotsReachedTarget++;
            Debug.Log($"نانوبات {nanobot.name} به هدف رسید! تعداد کل: {nanobotsReachedTarget} از {nanobotCount}");

            // Path sharing from individual nanobots is now less critical due to A*
            // NanobotBehavior behavior = nanobot.GetComponent<NanobotBehavior>();
            // if (behavior != null)
            // {
            //     List<Vector3> successfulPath = behavior.GetLearnedPath();
            //     if (successfulPath.Count > 0 && masterOptimalPath.Count == 0) // Only share if A* failed
            //     {
            //         SharePathWithUninformedNanobots(successfulPath);
            //     }
            // }
        }
    }
    
    // Renamed and repurposed for master A* path or a given path if A* failed
    private void ShareMasterOptimalPathWithNanobot(NanobotBehavior behavior)
    {
        if (masterOptimalPath.Count > 0 && behavior != null && !behavior.HasLearnedPath() && !behavior.HasReachedTarget())
        {
            behavior.LearnPathFromOther(new List<Vector3>(masterOptimalPath)); // Give a copy
            behavior.SetInformed(true); // Ensure it's marked as informed
        }
        // Optional: Fallback to manual path if A* path is not available
        else if (manualPath.Count > 0 && behavior != null && !behavior.HasLearnedPath() && !behavior.HasReachedTarget())
        {
             behavior.LearnPathFromOther(new List<Vector3>(manualPath));
             behavior.SetInformed(true);
        }
    }

    // This method can be used if we need to propagate a path other than masterOptimalPath,
    // for example, if A* fails and a nanobot finds a path via random exploration.
    // For now, the primary mechanism is A* path distribution.
    private void SharePathWithUninformedNanobots(List<Vector3> path)
    {
        if (path == null || path.Count == 0) return;

        int sharedCount = 0;
        foreach (GameObject nb in nanobots)
        {
            if (nb == null) continue;
            NanobotBehavior behavior = nb.GetComponent<NanobotBehavior>();
            if (behavior != null && !behavior.HasLearnedPath() && !behavior.HasReachedTarget())
            {
                behavior.LearnPathFromOther(new List<Vector3>(path)); // Give a copy
                behavior.SetInformed(true);
                sharedCount++;
            }
        }
        if(sharedCount > 0)
            Debug.Log($"Shared a path with {sharedCount} uninformed nanobots.");
    }
    
    // دریافت نقطه تزریق از سلکتور
    private Vector3 GetInjectionPoint()
    {
        var field = selector.GetType().GetField("injectionPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (Vector3)field.GetValue(selector) : Vector3.zero;
    }
    
    // دریافت نقطه هدف از سلکتور
    private Vector3 GetTargetPoint()
    {
        var field = selector.GetType().GetField("targetPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (Vector3)field.GetValue(selector) : Vector3.zero;
    }
    
    // بررسی کلیک کاربر برای اضافه کردن نقطه به مسیر دستی
    void Update()
    {
        if (!manualPathMode) return;
        
        // بررسی کلیک ماوس
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // بررسی اینکه آیا روی یک رگ خونی کلیک شده است
                if (hit.collider.gameObject.CompareTag("BloodVessel") || hit.collider.gameObject.name.Contains("Vessel"))
                {
                    AddPointToManualPath(hit.point);
                }
                else
                {
                    // پیدا کردن نزدیک‌ترین رگ خونی به نقطه کلیک شده
                    BloodVesselGenerator generator = FindObjectOfType<BloodVesselGenerator>();
                    if (generator != null)
                    {
                        Vector3 closestPoint;
                        GameObject vessel = generator.GetClosestVessel(hit.point, out closestPoint);
                        if (vessel != null)
                        {
                            AddPointToManualPath(closestPoint);
                        }
                    }
                }
            }
        }
    }
}