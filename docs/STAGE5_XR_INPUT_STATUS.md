# Stage 5 XR Input Status

## Current Package Status

- XR Plug-in Management (`com.unity.xr.management`): present.
- OpenXR Plugin (`com.unity.xr.openxr`): present.
- Input System (`com.unity.inputsystem`): present.
- XR Interaction Toolkit (`com.unity.xr.interaction.toolkit`): not detected.
- Meta XR Core SDK (`com.meta.xr.sdk.core`): not detected.
- Meta XR Interaction SDK (`com.meta.xr.sdk.interaction`): not detected.

## Adapter Status

- Stage 5 runtime scripts are package-independent.
- `DesktopRightHandInputSource` provides the current automated input path.
- `XrRightHandInputAdapter` exposes a compile-safe right-hand ray boundary for future Quest bindings.
- `SimulatedRightHandRig` provides the visible placeholder controller, ray, and wrist anchor.

## Future Quest Binding Notes

Map right controller trigger/select to primary command, thumbstick/buttons to move/attack/force-attack modes, grip or palm action to board manipulation, thumbstick horizontal to rotate, and thumbstick vertical or pinch/axis input to scale/zoom. Keep these bindings behind `IRightHandInputSource` so Stage 2 PC controls and Stage 4 left-hand controls remain unchanged.
