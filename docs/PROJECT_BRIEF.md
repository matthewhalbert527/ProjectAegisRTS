# Project Brief

ProjectAegisRTS is a modern mixed-reality RTS inspired by classic basebuilding, production queues, and command semantics, but with original code, names, assets, and presentation.

## Quest Mode

The Quest 3S version should let the player place a battlefield board in real space or play on a large adjustable virtual board. The board can be moved, scaled, rotated, and height-adjusted. The simulation remains deterministic and independent of controller poses or Unity physics.

## PC Mode

The PC version uses mouse and keyboard RTS controls. Production and building commands appear in a right-side panel with OpenRA-like categories and command flow, implemented as a new UI rather than a port of OpenRA chrome YAML.

## Interaction Direction

- Left hand/controller: build palette, production, selection management, and command context.
- Right hand/controller: tactical orders, movement and attack confirmation, board manipulation, and camera or board controls.
- PC: selection, move, attack, rally, stop, build, and power commands through conventional RTS mouse/keyboard patterns.

## Asset Direction

Stage 0 uses placeholders and concept registries only. Later stages should replace all placeholder visuals with modern, high-detail realistic models, materials, effects, and animation states. Buildings should visibly respond to power, production, damage, and offline states.
