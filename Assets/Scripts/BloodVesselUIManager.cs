using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Required for LINQ operations

public class BloodVesselUIManager : MonoBehaviour
{
    public BloodVesselGenerator generator;
    public Button createButton;
    public Button resetButton;
    public TMP_InputField vesselCountInput;
    public Button injectionModeButton;
    public Button targetModeButton;
    public TMP_Text injectionText;
    public TMP_Text targetText;
    public BloodVesselSelector selector;
    
    // اضافه کردن دکمه‌ها و فیلدهای مربوط به شبیه‌سازی نانوبات
    public Button startSimulationButton;
    public Button stopSimulationButton;
    public TMP_InputField nanobotCountInput;
    public NanobotSimulation nanobotSimulation;
    
    // دکمه‌های مربوط به انتخاب مسیر دستی
    public Button manualPathButton;
    public Button clearPathButton;
    public TMP_Text pathModeText;

    // اضافه کردن فیلدها و دکمه‌های مربوط به اکشن‌های نانوبات
    public TMP_Dropdown nanobotActionDropdown;
    public Button executeActionButton;
    public NanobotActionManager nanobotActionManager; // Reference to the new action manager

    void Start()
    {
        createButton.onClick.AddListener(OnCreateClicked);
        resetButton.onClick.AddListener(OnResetClicked);
        if (injectionModeButton != null && selector != null)
            injectionModeButton.onClick.AddListener(() => selector.SetInjectionMode());
        if (targetModeButton != null && selector != null)
            targetModeButton.onClick.AddListener(() => selector.SetTargetMode());
            
        // اضافه کردن لیسنرهای دکمه‌های شبیه‌سازی
        if (startSimulationButton != null && nanobotSimulation != null)
            startSimulationButton.onClick.AddListener(OnStartSimulationClicked);
        if (stopSimulationButton != null && nanobotSimulation != null)
            stopSimulationButton.onClick.AddListener(OnStopSimulationClicked);
            
        // اضافه کردن لیسنرهای دکمه‌های مسیر دستی
        if (manualPathButton != null && nanobotSimulation != null)
            manualPathButton.onClick.AddListener(OnManualPathClicked);
        if (clearPathButton != null && nanobotSimulation != null)
            clearPathButton.onClick.AddListener(OnClearPathClicked);

        // اضافه کردن لیسنر دکمه اجرای اکشن و پر کردن دراپ‌دون
        if (executeActionButton != null && nanobotActionManager != null)
            executeActionButton.onClick.AddListener(OnExecuteActionClicked);

        if (nanobotActionDropdown != null)
        {
            PopulateActionDropdown();
        }
    }

    void OnCreateClicked()
    {
        if (vesselCountInput != null && int.TryParse(vesselCountInput.text, out int count))
            generator.vesselCount = count;
        generator.CreateVessels();
    }

    void OnResetClicked()
    {
        generator.ResetVessels();
    }
    
    // متدهای مربوط به شبیه‌سازی نانوبات
    void OnStartSimulationClicked()
    {
        if (nanobotSimulation != null)
        {
            // تنظیم تعداد نانوبات‌ها اگر فیلد ورودی وجود داشته باشد
            if (nanobotCountInput != null && int.TryParse(nanobotCountInput.text, out int count))
                nanobotSimulation.nanobotCount = count;
                
            nanobotSimulation.StartSimulation();
        }
    }
    
    void OnStopSimulationClicked()
    {
        if (nanobotSimulation != null)
        {
            nanobotSimulation.ResetSimulation();
        }
    }
    
    // متدهای مربوط به انتخاب مسیر دستی
    void OnManualPathClicked()
    {
        if (nanobotSimulation != null)
        {
            nanobotSimulation.ToggleManualPathMode();
            UpdatePathModeText();
        }
    }
    
    void OnClearPathClicked()
    {
        if (nanobotSimulation != null)
        {
            nanobotSimulation.ClearManualPath();
        }
    }
    
    // به‌روزرسانی متن وضعیت انتخاب مسیر
    void UpdatePathModeText()
    {
        if (pathModeText != null && nanobotSimulation != null)
        {
            if (nanobotSimulation.IsInManualPathMode())
                pathModeText.text = "حالت انتخاب مسیر: فعال";
            else
                pathModeText.text = "حالت انتخاب مسیر: غیرفعال";
        }
    }

    // متد مربوط به اجرای اکشن نانوبات
    void OnExecuteActionClicked()
    {
        if (nanobotActionDropdown != null && nanobotActionManager != null)
        {
            NanobotActionManager.NanobotActionType selectedAction = (NanobotActionManager.NanobotActionType)nanobotActionDropdown.value;
            nanobotActionManager.ExecuteAction(selectedAction);
        }
    }

    // متد برای پر کردن دراپ‌دون اکشن‌ها
    void PopulateActionDropdown()
    {
        nanobotActionDropdown.ClearOptions();
        List<string> options = System.Enum.GetNames(typeof(NanobotActionManager.NanobotActionType)).ToList();
        nanobotActionDropdown.AddOptions(options);
    }
}