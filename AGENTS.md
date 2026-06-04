# Repository Guidelines

## Project Structure & Module Organization

This is a Unity 2022.3.62f3 project. Runtime code lives in `Assets/Scripts`, with feature areas split into `AutoGain`, `CD Gain`, `Data`, `Managers`, `2D`, and `TestScene`. Scenes are in `Assets/Scenes`, including `Lobby.unity`, `3D Auto Gain.unity`, `3D Fitts Test.unity`, and `2D.unity`. Project packages are declared in `Packages/manifest.json`; renderer and pipeline assets are under `Assets/Settings` and `ProjectSettings`. Experiment configuration is stored in `Assets/Json/session_config.json`. Do not commit generated Unity folders such as `Library`, `Temp`, `Logs`, `UserSettings`, or local references in `_local_refs`.

## Build, Test, and Development Commands

Open the project with Unity Editor `2022.3.62f3` from Unity Hub or by passing this repository path to the editor.

Useful batch commands:

```powershell
Unity.exe -batchmode -quit -projectPath . -buildWindows64Player Builds/3d-fps-autogain.exe
Unity.exe -batchmode -quit -projectPath . -runTests -testPlatform EditMode -testResults TestResults/EditMode.xml
Unity.exe -batchmode -quit -projectPath . -runTests -testPlatform PlayMode -testResults TestResults/PlayMode.xml
```

Use the first command for a Windows player build. Use the test commands when EditMode or PlayMode tests are present.

## Coding Style & Naming Conventions

Follow existing C# Unity style: four-space indentation, PascalCase for classes and public methods, camelCase for fields and locals, and descriptive MonoBehaviour names such as `AGManager`, `AGTargetGenerator`, and `GameManager2D`. Prefer `[SerializeField]` fields for Inspector wiring over public mutable fields. Keep scene-specific scripts in the nearest feature folder. Preserve `.meta` files when moving assets so Unity references remain intact.

## Testing Guidelines

The Unity Test Framework package is installed, but no dedicated `Assets/Tests` assembly is currently committed. Add EditMode tests for pure data/model logic, especially classes under `Assets/Scripts/Data`, and PlayMode tests for scene workflows, input, spawning, and logging behavior. Name test files after the unit under test, for example `SessionConfigurationTests.cs`.

## Commit & Pull Request Guidelines

Recent history uses concise Conventional Commit prefixes: `feat:`, `fix:`, `chore:`, and `build:`. Keep commit messages focused, for example `fix: Resolve mouse movement logging issue`. Pull requests should describe the gameplay or experiment behavior changed, list tested scenes, include test/build results, and attach screenshots or short captures for UI or scene changes. Link related issues when available.

## Security & Configuration Tips

Logs are written to `Assets/Log` in the editor and `Application.persistentDataPath` in Windows builds. Avoid committing generated logs, participant data, private references, or machine-specific Unity settings.

# User-Added Instructions

## Language

Use Korean by default when communicating with the user. However, it is acceptable to include English terms within Korean sentences for academic terms and programming terminology. When writing code comments, use English unless the user explicitly requests otherwise.

## Additional Project Context

- This project is an undergraduate thesis project. Its goal is to apply existing research and produce an original project outcome based on it.
- The `_local_refs` directory contains research papers and related software source code that the user referenced while developing this project. Reviewing these materials together with the Unity project source code will help you understand the project.
- Do not edit any files in `_local_refs`.