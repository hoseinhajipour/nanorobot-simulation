# Nanobot Simulation Project

This project simulates the movement and behavior of nanobots within a blood vessel network.

## Features:

*   **Nanobot Navigation:** Nanobots can move and navigate through a generated blood vessel system.
*   **Pathfinding:** Nanobots can find and follow paths to a target location.
*   **Communication:** Nanobots can share learned paths with each other to improve efficiency.
*   **Manual Path Selection:** Users can manually select paths for nanobots.

## Key Scripts:

*   `NanobotBehavior.cs`: Handles individual nanobot movement, path memory, and state.
*   `NanobotSimulation.cs`: Manages the overall simulation, including nanobot communication and path sharing.
*   `BloodVesselGenerator.cs`: Generates the blood vessel network.

## Recent Updates:

*   Optimized pathfinding to allow nanobots to remember and share paths.
*   Implemented a manual path selection system with UI integration.
*   Fixed issues related to nanobot freezing after one finds the target by improving path sharing logic in `NanobotSimulation.cs`.
*   Added the `HasReachedTarget` method to `NanobotBehavior.cs` for proper status checks.

