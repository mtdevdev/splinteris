# SPLINTERIS: Source Code

Este repositório contém o código-fonte do jogo **SPLINTERIS**, desenvolvido na **Unity Engine**. O objetivo deste projeto é demonstrar minhas competências em arquitetura de software, organização de projetos e padrões de desenvolvimento em C#.

---

## Tecnologias e Ferramentas
* **Engine:** [Unity](https://unity.com) (Versão: 6.1 (6000.1.15f1))
* **Linguagem:** C#
* **Versionamento:** Git (com [.gitignore para Unity](https://github.com/github/gitignore/blob/main/Unity.gitignore))

---

## Estrutura da pasta Scripts

A arquitetura do projeto foi dividida em domínios específicos para facilitar a manutenção, escalabilidade e o desacoplamento do código. Abaixo está a estrutura de pastas principal:

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
