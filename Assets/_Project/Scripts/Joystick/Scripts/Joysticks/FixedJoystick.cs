using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class FixedJoystick : Joystick
{
}

public class ActionGamePlay
{
    public static InputActions.IGameplayActions m_GameplayActionsCallbackInterface;
    public static NativeList<DeviceInputEvent<float2>> MoveInputs;
    public static NativeList<DeviceInputEvent<float2>> LookInputs;
    public static NativeList<DeviceInputEvent<float>> ShootInputs;
    public static NativeList<DeviceInputEvent<float>> MeleeInputs;
    public static NativeList<DeviceInputEvent<float>> ReturnInputs;
    public static NativeList<DeviceInputEvent<float>> ActionInputs;
    public static InputActions inputActions;
    public static void SetInput(InputActions input)
    {
        inputActions = input;
        MoveInputs = new NativeList<DeviceInputEvent<float2>>(Allocator.Persistent);
        LookInputs = new NativeList<DeviceInputEvent<float2>>(Allocator.Persistent);
        ShootInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);
        MeleeInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);
        ReturnInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);
        ActionInputs = new NativeList<DeviceInputEvent<float>>(Allocator.Persistent);

    }

}