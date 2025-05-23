using UnityEngine;
using System.Collections.Generic;

public class NanobotBehavior : MonoBehaviour
{
    public NanobotSimulation simulation;
    public Vector3 targetPoint;
    public float moveSpeed = 2f;
    public float randomMovementRange = 1f;
    public float maxDistanceFromVessel = 0.1f; // حداکثر فاصله مجاز از رگ خونی
    
    private bool isInformed = false; // آیا این نانوبات از محل هدف آگاه است
    private Vector3 currentDirection;
    private float directionChangeTime = 2f;
    private float directionTimer = 0f;
    private bool hasReachedTarget = false;
    private GameObject currentVessel; // رگ خونی فعلی که نانوبات در آن قرار دارد
    private Vector3 lastValidPosition; // آخرین موقعیت معتبر روی رگ خونی
    
    // حافظه مسیر - لیستی از نقاط مسیر که نانوبات یاد گرفته است
    private List<Vector3> pathMemory = new List<Vector3>();
    private int currentPathIndex = 0;
    private bool hasLearnedPath = false; // آیا این نانوبات مسیر را یاد گرفته است
    
    // رنگ‌های مختلف برای نمایش وضعیت نانوبات
    private Color normalColor = Color.white;
    private Color informedColor = Color.yellow;
    private Color reachedColor = Color.green;
    
    void Start()
    {
        // تنظیم جهت اولیه حرکت به صورت تصادفی
        SetRandomDirection();
        
        // تنظیم رنگ اولیه
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = normalColor;
        }
        
        // پیدا کردن نزدیک‌ترین رگ خونی به موقعیت فعلی
        FindClosestVessel();
        lastValidPosition = transform.position;
    }
    
    void Update()
    {
        if (hasReachedTarget) return;
        
        // بررسی رسیدن به هدف
        float distanceToTarget = Vector3.Distance(transform.position, targetPoint);
        if (distanceToTarget < 0.5f)
        {
            ReachTarget();
            return;
        }
        
        // حرکت نانوبات
        if (isInformed)
        {
            // اگر از هدف آگاه است، مستقیم به سمت هدف حرکت کند
            MoveTowardsTarget();
        }
        else
        {
            // حرکت تصادفی
            RandomMovement();
        }
        
        // محدود کردن حرکت به مسیر رگ خونی
        ConstrainToVessel();
    }
    
    // حرکت به سمت هدف
    private void MoveTowardsTarget()
    {
        // اگر مسیر را یاد گرفته باشد، از حافظه مسیر استفاده کن
        if (hasLearnedPath && pathMemory.Count > 0)
        {
            FollowLearnedPath();
            return;
        }
        
        Vector3 direction = (targetPoint - transform.position).normalized;
        
        // اضافه کردن کمی حرکت تصادفی حتی در حالت آگاهی
        direction += Random.insideUnitSphere * 0.1f;
        direction.Normalize();
        
        // محاسبه موقعیت جدید
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        
        // بررسی اینکه آیا موقعیت جدید در مسیر رگ خونی است
        if (IsPositionValidOnVessel(newPosition))
        {
            transform.position = newPosition;
            lastValidPosition = newPosition;
            
            // Path recording is removed here as A* path is now provided externally
            // and should not be modified by individual nanobot's movement attempts.
            // if (pathMemory.Count == 0 || Vector3.Distance(pathMemory[pathMemory.Count - 1], transform.position) > 0.5f)
            // {
            //     pathMemory.Add(transform.position);
            // }
        }
        else
        {
            // اگر موقعیت جدید خارج از رگ خونی است، به دنبال رگ خونی جدید بگرد
            FindClosestVessel();
            // اگر رگ خونی جدیدی پیدا شد، به آن منتقل شو
            if (currentVessel != null)
            {
                transform.position = lastValidPosition;
            }
        }
        
        transform.forward = direction; // چرخش به سمت جهت حرکت
    }
    
    // دنبال کردن مسیر یاد گرفته شده
    private void FollowLearnedPath()
    {
        // اگر به انتهای مسیر رسیده‌ایم
        if (currentPathIndex >= pathMemory.Count)
        {
            // اگر به هدف نزدیک هستیم، مستقیم به سمت هدف حرکت کن
            if (Vector3.Distance(transform.position, targetPoint) < 2f)
            {
                Vector3 direction = (targetPoint - transform.position).normalized;
                transform.position += direction * moveSpeed * 1.5f * Time.deltaTime; // سرعت بیشتر
                transform.forward = direction;
            }
            return;
        }
        
        // حرکت به سمت نقطه بعدی در مسیر
        Vector3 targetDir = (pathMemory[currentPathIndex] - transform.position).normalized;
        transform.position += targetDir * moveSpeed * 1.5f * Time.deltaTime; // سرعت بیشتر برای مسیر یاد گرفته شده
        transform.forward = targetDir;
        
        // اگر به نقطه رسیدیم، به نقطه بعدی برو
        if (Vector3.Distance(transform.position, pathMemory[currentPathIndex]) < 0.2f)
        {
            currentPathIndex++;
            if (currentPathIndex < pathMemory.Count)
            {
                Debug.Log($"Nanobot {gameObject.name} reached path point. Next target: {pathMemory[currentPathIndex]} at index {currentPathIndex}. Current Pos: {transform.position}");
            }
            else
            {
                Debug.Log($"Nanobot {gameObject.name} reached end of its path memory. Current Pos: {transform.position}");
            }
        }
        // Log current movement state periodically (e.g. every 30th frame, approx every 0.5s)
        else if (Time.frameCount % 30 == 0 && currentPathIndex < pathMemory.Count) 
        {
            Debug.Log($"Nanobot {gameObject.name} moving towards {pathMemory[currentPathIndex]} (idx {currentPathIndex}). Current Pos: {transform.position}");
        }
    }
    
    // حرکت تصادفی
    private void RandomMovement()
    {
        // تغییر جهت هر چند ثانیه
        directionTimer += Time.deltaTime;
        if (directionTimer >= directionChangeTime)
        {
            SetRandomDirection();
            directionTimer = 0f;
        }
        
        // محاسبه موقعیت جدید
        Vector3 newPosition = transform.position + currentDirection * moveSpeed * Time.deltaTime;
        
        // بررسی اینکه آیا موقعیت جدید در مسیر رگ خونی است
        if (IsPositionValidOnVessel(newPosition))
        {
            transform.position = newPosition;
            lastValidPosition = newPosition;
        }
        else
        {
            // اگر موقعیت جدید خارج از رگ خونی است، جهت را تغییر بده
            SetRandomDirection();
            directionTimer = 0f;
        }
        
        transform.forward = currentDirection; // چرخش به سمت جهت حرکت
    }
    
    // تنظیم یک جهت تصادفی جدید
    private void SetRandomDirection()
    {
        if (currentVessel != null)
        {
            // اگر در رگ خونی هستیم، جهت را در امتداد رگ خونی تنظیم کن
            Vector3 vesselDirection = currentVessel.transform.up;
            
            // اضافه کردن کمی انحراف تصادفی
            Vector3 randomOffset = Random.insideUnitSphere * 0.3f;
            currentDirection = (vesselDirection + randomOffset).normalized;
            
            // تصادفی کردن جهت (به سمت بالا یا پایین رگ)
            if (Random.value > 0.5f)
                currentDirection = -currentDirection;
        }
        else
        {
            // اگر در رگ خونی نیستیم، جهت کاملاً تصادفی
            currentDirection = Random.onUnitSphere.normalized;
        }
    }
    
    // تنظیم وضعیت آگاهی نانوبات
    public void SetInformed(bool informed)
    {
        if (isInformed == informed) return;
        
        isInformed = informed;
        
        // تغییر رنگ نانوبات بر اساس وضعیت
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = isInformed ? informedColor : normalColor;
        }
    }
    
    // دریافت مسیر از نانوبات دیگر
    public void LearnPathFromOther(List<Vector3> path)
    {
        if (path == null || path.Count == 0 || hasLearnedPath) return;
        
        // کپی کردن مسیر
        pathMemory = new List<Vector3>(path);
        currentPathIndex = 0;
        hasLearnedPath = true;
        
        // تغییر رنگ به حالت آگاه
        SetInformed(true);
    }
    
    // ارسال مسیر به نانوبات دیگر
    public List<Vector3> GetLearnedPath()
    {
        return new List<Vector3>(pathMemory);
    }
    
    // آیا این نانوبات مسیر را یاد گرفته است
    public bool HasLearnedPath()
    {
        return hasLearnedPath;
    }
    
    // آیا این نانوبات به هدف رسیده است
    public bool HasReachedTarget()
    {
        return hasReachedTarget;
    }
    
    // رسیدن به هدف
    private void ReachTarget()
    {
        hasReachedTarget = true;
        
        // تغییر رنگ به سبز
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = reachedColor;
        }
        
        // اطلاع به سیستم شبیه‌سازی
        if (simulation != null)
        {
            simulation.NanobotReachedTarget(gameObject);
            Debug.Log($"Nanobot {gameObject.name} reached targetPoint: {targetPoint} at Time: {Time.timeSinceLevelLoad}.");
        }
        else
        {
            Debug.LogWarning($"Nanobot {gameObject.name} reached targetPoint: {targetPoint} but simulation reference is null.");
        }
    }
    
    // پیدا کردن نزدیک‌ترین رگ خونی
    private void FindClosestVessel()
    {
        BloodVesselGenerator generator = FindObjectOfType<BloodVesselGenerator>();
        if (generator == null) return;
        
        Vector3 closestPoint;
        currentVessel = generator.GetClosestVessel(transform.position, out closestPoint);
        
        if (currentVessel != null)
        {
            // اگر فاصله تا رگ خونی بیشتر از حد مجاز است، به نزدیک‌ترین نقطه روی رگ منتقل شو
            float distance = Vector3.Distance(transform.position, closestPoint);
            if (distance > maxDistanceFromVessel)
            {
                transform.position = closestPoint;
                lastValidPosition = closestPoint;
            }
        }
    }
    
    // بررسی اینکه آیا موقعیت داده شده در مسیر رگ خونی است
    private bool IsPositionValidOnVessel(Vector3 position)
    {
        if (currentVessel == null) return false;
        
        BloodVesselGenerator generator = FindObjectOfType<BloodVesselGenerator>();
        if (generator == null) return false;
        
        Vector3 closestPoint;
        GameObject vessel = generator.GetClosestVessel(position, out closestPoint);
        
        // اگر نزدیک‌ترین رگ خونی همان رگ فعلی است و فاصله کمتر از حد مجاز است
        if (vessel == currentVessel && Vector3.Distance(position, closestPoint) <= maxDistanceFromVessel)
            return true;
            
        // اگر رگ خونی دیگری نزدیک‌تر است و فاصله کمتر از حد مجاز است
        if (vessel != null && vessel != currentVessel && Vector3.Distance(position, closestPoint) <= maxDistanceFromVessel)
        {
            currentVessel = vessel;
            return true;
        }
        
        return false;
    }
    
    // محدود کردن موقعیت به مسیر رگ خونی
    private void ConstrainToVessel()
    {
        if (currentVessel == null)
        {
            FindClosestVessel();
            return;
        }
        
        BloodVesselGenerator generator = FindObjectOfType<BloodVesselGenerator>();
        if (generator == null) return;
        
        Vector3 closestPoint;
        generator.GetClosestVessel(transform.position, out closestPoint);
        
        float distance = Vector3.Distance(transform.position, closestPoint);
        if (distance > maxDistanceFromVessel)
        {
            // اگر از رگ خونی خارج شده‌ایم، به نزدیک‌ترین نقطه روی رگ برگرد
            transform.position = Vector3.Lerp(transform.position, closestPoint, 0.5f);
            lastValidPosition = closestPoint;
        }
    }
}