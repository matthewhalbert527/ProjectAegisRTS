# Unity Setup Notes

Do not create a full Unity project in Stage 0 unless project files already exist. This folder is a placeholder for the later client.

Recommended future setup:

- Unity 6.1 or newer.
- Universal Render Pipeline.
- OpenXR.
- Meta XR Core SDK and Interaction SDK in a later Quest-focused stage.
- Android/Meta Quest build target.
- PC standalone target.
- `Rts.Core` remains UnityEngine-free and is consumed as a deterministic simulation library.
- Unity renders snapshots and submits commands; it does not own authoritative gameplay state.

Initial Stage 1 work should create a desktop board scene, load placeholder meshes, render `WorldSnapshot` actors, and send command DTOs to the simulation.
