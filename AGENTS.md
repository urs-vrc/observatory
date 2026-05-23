# AI Agent Guidelines for Observatory

This document provides context and instructions for AI agents contributing to the Observatory project. This is a VRChat world project built with Unity and UdonSharp.

## Project Overview

Observatory is a modular VRChat world. It utilizes a central management system to handle global states (like time and weather) and specific "Experiences" that contain their own game logic.

## Technical Stack

- **Engine:** Unity 2022.3.22f1
- **Scripting:** UdonSharp (C# wrapper for Udon VM)
- **SDK:** VRChat SDK3 (Base/Worlds)
- **Render Pipeline:** Built-in Render Pipeline
- **Target Framework:** .NET 4.7.1 / C# 9.0

## Architecture Patterns

### 1. World Management

- **WorldManager.cs**: The central authority for global environment variables.
  - Handles synchronized time-of-day and weather.
  - Uses `[UdonSynced]` variables to maintain state across network clients.
  - Focuses on normalized time (0.0 to 1.0) for easier synchronization.

### 2. Experience Modularization

Experiences are located in `Assets/Code/Experiences/`. Each experience (e.g., `Redshift`, `Stargazer`) should follow this pattern:

- **Manager Class**: A primary manager (e.g., `RedshiftGameManager`) that coordinates the experience's state.
- **Asset-Based Config**: Use `.asset` files (ScriptableObjects or UdonSharp equivalents) for configuration to avoid hardcoding values in scripts.
- **Structures**: Logic-specific data structures should be placed in the `Structures/` subfolder of the experience.

## Coding Standards & Constraints

### UdonSharp Specifics

- **Networking**: Always use `VRC.SDKBase.Networking` for ownership and synchronization.
- **Synced Variables**: Use `[UdonSynced]` for variables that must be consistent across all players. Remember that only the owner of the object can change a synced variable.
- **Performance**: Avoid heavy computations in `Update()`. Prefer event-driven logic or timers where possible, as Udon VM is more restrictive than standard C#.

### General C# Guidelines

- **Naming**: Use `PascalCase` for methods and public variables, and `_camelCase` for private fields.
- **Documentation**: Provide clear comments for complex networking logic, specifically regarding who owns an object and when synchronization occurs.

## Contribution Workflow

1. **Analyze Dependency**: Before adding a new feature, check if it belongs in `WorldManager` (global) or a specific `Experience` (local).
2. **Network Validation**: If the feature involves player interaction, explicitly define the networking model (e.g., "Owner-authoritative" or "Manual sync").
3. **Asset Integration**: Ensure any new scripts are associated with a `.meta` file and properly placed in the `Assets/Code/` hierarchy.
