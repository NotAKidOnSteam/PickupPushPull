using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Valve.VR;
using NAK.Melons.PickupPushPull.InputModules;

namespace NAK.Melons.PickupPushPull;

public class PickupPushPull : MelonMod
{
    private static MelonPreferences_Category Category_PickupPushPull;
    private static MelonPreferences_Entry<float> Setting_PushPullSpeed, Setting_RotateSpeed;
    private static MelonPreferences_Entry<bool> Setting_EnableRotation, Setting_Desktop_UseZoomForRotate;

    public override void OnInitializeMelon()
    {
        Category_PickupPushPull = MelonPreferences.CreateCategory(nameof(PickupPushPull));

        //Global settings
        Setting_PushPullSpeed = Category_PickupPushPull.CreateEntry("Push Pull Speed", 2f, description: "Up/down on right joystick for VR. Left buSettingr + Up/down on right joystick for Gamepad.");
        Setting_RotateSpeed = Category_PickupPushPull.CreateEntry<float>("Rotate Speed", 6f);
        Setting_EnableRotation = Category_PickupPushPull.CreateEntry<bool>("Enable Rotation", false, description: "Hold left trigger in VR or right buSettingr on Gamepad.");

        //Desktop settings
        Setting_Desktop_UseZoomForRotate = Category_PickupPushPull.CreateEntry<bool>("Desktop Use Zoom For Rotate", true, description: "Use zoom bind for rotation while a prop is held.");

        //bruh
        foreach (var setting in Category_PickupPushPull.Entries)
        {
            setting.OnEntryValueChangedUntyped.Subscribe(OnUpdateSettings);
        }

        MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
    }


    System.Collections.IEnumerator WaitForLocalPlayer()
    {
        while (CVRInputManager.Instance == null)
            yield return null;

        CVRInputManager.Instance.gameObject.AddComponent<PickupPushPull_Module>();

        //update BlackoutController settings after it initializes
        while (PickupPushPull_Module.Instance == null)
            yield return null;

        UpdateAllSettings();
    }

    private void OnUpdateSettings(object arg1, object arg2) => UpdateAllSettings();

    private void UpdateAllSettings()
    {
        if (!PickupPushPull_Module.Instance) return;

        //Global settings
        PickupPushPull_Module.Instance.Setting_PushPullSpeed = Setting_PushPullSpeed.Value * 50;
        PickupPushPull_Module.Instance.Setting_RotationSpeed = Setting_RotateSpeed.Value * 50;
        PickupPushPull_Module.Instance.Setting_EnableRotation = Setting_EnableRotation.Value;
        //Desktop settings
        PickupPushPull_Module.Instance.Desktop_UseZoomForRotate = Setting_Desktop_UseZoomForRotate.Value;
    }
}