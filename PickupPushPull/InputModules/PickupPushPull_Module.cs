using ABI.CCK.Components;
using ABI_RC.Core;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace NAK.Melons.PickupPushPull.InputModules;

public class PickupPushPull_Module : CVRInputModule
{
    //Reflection shit
    private static readonly FieldInfo _grabbedObject = typeof(ControllerRay).GetField("grabbedObject", BindingFlags.NonPublic | BindingFlags.Instance);

    //Global stuff
    public static PickupPushPull_Module Instance;
    public Vector2 objectRotation = Vector2.zero;

    //Global settings
    public float Setting_PushPullSpeed = 100f;
    public float Setting_RotationSpeed = 200f;
    public bool Setting_EnableRotation = false;

    //Desktop settings
    public bool Desktop_UseZoomForRotate = true;

    //Local stuff
    private CVRInputManager _inputManager;
    private ControllerRay desktopControllerRay;
    private bool controlGamepadEnabled;

    public new void Start()
    {
        _inputManager = CVRInputManager.Instance;
        Instance = this;
        base.Start();
        
        //Get desktop controller ray
        desktopControllerRay = PlayerSetup.Instance.desktopCamera.GetComponent<ControllerRay>();

        controlGamepadEnabled = (bool)MetaPort.Instance.settings.GetSettingsBool("ControlEnableGamepad", false);
        MetaPort.Instance.settings.settingBoolChanged.AddListener(new UnityAction<string, bool>(SettingsBoolChanged));
    }

    private void SettingsBoolChanged(string name, bool value)
    {
        if (name == "ControlEnableGamepad")
            controlGamepadEnabled = value;
    }

    //this will run while menu is being hovered
    public override void UpdateImportantInput()
    {
        objectRotation = Vector2.zero;
    }

    //this will only run outside of menus
    public override void UpdateInput()
    {
        objectRotation = Vector2.zero;

        CVRPickupObject desktopObject = (CVRPickupObject)_grabbedObject.GetValue(desktopControllerRay);
        if (desktopObject != null && desktopObject.gripType == CVRPickupObject.GripType.Free)
        {
            //Desktop Input
            DoDesktopInput();
            //Gamepad Input
            DoGamepadInput();
        }
    }

    private void DoDesktopInput()
    {
        if (!Desktop_UseZoomForRotate) return;

        //mouse rotation when zoomed
        if (Setting_EnableRotation && _inputManager.zoom)
        {
            objectRotation.x += Setting_RotationSpeed * _inputManager.rawLookVector.x;
            objectRotation.y += Setting_RotationSpeed * _inputManager.rawLookVector.y * -1;
            _inputManager.lookVector = Vector2.zero;
            _inputManager.zoom = false;
            return;
        }
    }

    private void DoGamepadInput()
    {
        if (!controlGamepadEnabled) return;

        //not sure how to make settings for this
        bool button1 = Input.GetButton("Controller Left Button");
        bool button2 = Input.GetButton("Controller Right Button");

        if (button1 || button2)
        {
            //Rotation
            if (Setting_EnableRotation && button2)
            {
                objectRotation.x += Setting_RotationSpeed * _inputManager.rawLookVector.x;
                objectRotation.y += Setting_RotationSpeed * _inputManager.rawLookVector.y * -1;
                _inputManager.lookVector = Vector2.zero;
                return;
            }

            _inputManager.objectPushPull += _inputManager.rawLookVector.y * Setting_PushPullSpeed * Time.deltaTime;
            _inputManager.lookVector = Vector2.zero;
        }
    }
}