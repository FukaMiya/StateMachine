## Overview
[English](README.md)
HybridStateMachine is a lightweight and flexible state machine library for Unity.
It seamlessly integrates "event-driven (push-based)" and "condition monitoring (pull-based)" and features intuitive description through method chaining and flexible support for DI.

## Features
- Hybrid Driven: Allows mixing condition monitoring (Pull) via `Update()` and event notification (Push) via `Fire()` within a single state machine. Events can be managed in a type-safe manner using Enums.

- Fluent Interface: Transition logic can be described in a way close to natural language, such as `idle.To(jump).On(Event.Jump).When(() => IsGrounded).Build();`.

- DI/Factory Support: Allows control over the generation of states with dependencies, enabling smooth integration with DI containers like VContainer or manual DI.

- Mermaid Visualization: Built state machines can be output in Mermaid notation for visualization as transition diagrams.

## Usage
- Place the `Assets/StateMachine` folder from the repository into any location in your Unity project.
- Add `using HybridStateMachine;` when using it.
- For specific usage, please check [GameEntryPoint.cs](Assets/GameEntryPoint.cs). You can run it from Demo.unity.

## License
[MIT License](LICENSE)