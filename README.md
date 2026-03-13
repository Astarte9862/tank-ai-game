# Tank AI Game

A prototype tank battle game where players program tank AI using a node editor.

Inspired by Carnage Heart and Gladiabots.

## Overview

This project explores a programmable AI system for a tank battle game.

Players design tank behaviors using a custom node editor.
The AI graph is saved as JSON and executed during battle.

## Features

- Node-based AI programming system
- Custom node editor built with Unity UI Toolkit
- AI behaviors saved as JSON
- Separate AI programs for player and enemy tanks
- Planned: 5 vs 5 tank battles

## AI System

Players create tank behaviors using nodes such as:

- IfEnemyAhead
- IfTurretAimed
- IfWallAhead
- MoveForward
- TurnLeft / TurnRight
- Fire

The node graph is saved as JSON and executed at runtime.

Example logic:
IfEnemyAhead
→ IfTurretAimed
→ Fire

Else
→ IfWallAhead
→ TurnLeft

Else
→ MoveForward

## Screenshots

### Node Editor

![Node Editor](docs/node_editor.png)

### Battle

![Battle](docs/battle.png)

## Tech Stack

- Unity
- C#
- Unity UI Toolkit
- JSON serialization

## Status

🚧 Early Development

## Asset Credits

Tank sprites from  
WW2 Pixel Top-View Tanks by JimHatama  
https://jimhatama.itch.io/ww2-pixel-top-view-tanks
