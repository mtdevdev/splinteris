# SPLINTERIS: Source Code

This repository contains the source code for **SPLINTERIS**, a game built with the **Unity Engine**. The goal of this project is to demonstrate my skills in software architecture, project organization, and C# development patterns.

---

## Technologies and Tools
* **Engine:** [Unity](https://unity.com) (Version: 6.1 (6000.1.15f1))
* **Language:** C#
* **Version Control:** Git (with [Unity's .gitignore](https://github.com/github/gitignore/blob/main/Unity.gitignore))

---

## Scripts Folder Structure

The project's architecture is split into specific domains to ease maintenance, scalability, and decoupling. Below is the main folder structure:

```text
Assets/_Project/Scripts/
├── Combat/
│   ├── GunController.cs
│   └── Projectile.cs
├── Entities/
│   ├── Enemy/
│   │   └── Enemy.cs
│   └── Player/
│       ├── FootstepHandler.cs
│       └── Player.cs
└── System/
    ├── Audio/
    │   └── AudioManager.cs
    ├── Camera/
    │   ├── CameraController.cs
    │   └── CameraShaker.cs
    └── Core/
        ├── GameManager.cs
        ├── TimeHandler.cs
        └── UIManager.cs
```
