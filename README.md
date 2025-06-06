# IMPERATOR_RPG

IMPERATOR_RPG est une application de bureau WPF pour Windows permettant de gérer des parties du jeu de rôle **Imperator**. Elle offre un système de chat et de lancer de dés en ligne ainsi qu'une feuille de personnage complète.

## Fonctionnalités principales

- **Serveur de chat** : le programme peut créer ou rejoindre un serveur TCP pour discuter entre joueurs.
- **Lancer de dés** : interface de configuration des jets (nombre de dés, difficulté, bonus). Les résultats apparaissent en couleur dans la discussion.
- **Feuille de personnage** : suivi des caractéristiques et compétences. Possibilité de sauvegarder/charger la feuille au format JSON.
- **Liste des joueurs connectés** avec rafraîchissement automatique.

## Prérequis

- Windows avec le **.NET Framework 4.8**.
- [Newtonsoft.Json](https://www.newtonsoft.com/json) (référencé via `packages.config`).
- Visual Studio 2019 (ou version ultérieure) ou MSBuild pour compiler.

## Compilation

Ouvrez `DiceRoller.csproj` avec Visual Studio puis reconstruisez la solution. Vous pouvez aussi utiliser MSBuild :

```bash
msbuild DiceRoller.csproj
```

Les binaires générés se trouvent dans `bin/Debug` ou `bin/Release`.

## Exécution

Lancez `DiceRoller.exe`. L’application présente d’abord un menu permettant de créer un serveur ou de rejoindre un serveur existant (en précisant IP, port et pseudonyme). Une fois connecté, la fenêtre principale permet :

1. d’échanger des messages dans le chat ;
2. de lancer des dés et partager les résultats ;
3. d’ouvrir la feuille de personnage pour la modifier, la sauvegarder ou la charger.

L’outil peut servir de base pour animer des parties à distance avec suivi de feuille de personnage et gestion des jets de dés.
