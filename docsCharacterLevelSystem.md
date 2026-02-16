## Flow Diagram

```mermaid
flowchart TD
    %% Subsystems
    subgraph Manager[CharacterSystemManager.cs]
      M_Awake[Awake()]
      M_InitLVL[LevelSystem = GetComponent<LVLSystem>]
      M_InitAttr[AttributeSystem = GetComponent<AttributeSystem>]
      M_InitSheet[Sheet = GetComponent<CharacterSheet>]
      M_Wire[LVLSystem.Initialize(this)\nAttributeSystem.Initialize(this)]
    end

    subgraph LVL[LVLSystem.cs]
      L_Initialize[Initialize(CharacterSystemManager)]
      L_Start[Start()]
      L_AddXP[AddXP(amount)]
      L_LevelUp[LevelUp()]
      L_UpdateUI[UpdateUI()]
      L_OnLvlEvt[OnCharacterLevelUpdate (event<int>)]
      L_OnLifetimeEvt[OnLifetimeXpUpdate (event<int>)]
    end

    subgraph ATTR[AttributeSystem.cs]
      A_Initialize[Initialize(CharacterSystemManager)]
      A_Start[Start()]
      A_NotifyAll[NotifyAllAttributeValues()]
      A_HandleLvlUp[HandleLevelUp(newLevel)]
      A_UpdatePts[UpdateAttributePoints(newPoints)]
      A_AddPoint[AddAttributePoint(attributeName)]
      A_OnAttrEvt[OnAttributeUpdate (event<string,int>)]
      A_OnPtsEvt[OnAttributePointUpdate (event<int>)]
    end

    subgraph SHEET[CharSheet.cs]
      S_Awake[Awake()]
      S_SubLvl[Susbcribe: OnCharacterLevelUpdate]
      S_SubLifetime[Susbcribe: OnLifetimeXpUpdate]
      S_SubAttr[Susbcribe: OnAttributeUpdate]
      S_SubPts[Susbcribe: OnAttributePointUpdate]
      S_HandleLvl[HandleLevelUpdate(newLevel)]
      S_HandleLifetime[HandleLifetimeXpUpdate(newLifetimeXp)]
      S_HandleAttr[HandleAttributeUpdate(name,value)]
      S_HandlePts[HandleAttributePointUpdate(newPoints)]
    end

    %% Initialization wiring
    M_Awake --> M_InitLVL --> M_InitAttr --> M_InitSheet --> M_Wire
    M_Wire --> L_Initialize
    M_Wire --> A_Initialize

    %% LVLSystem Initialize references to other systems
    L_Initialize -->|refs set: AttributeSystem, CharacterSheet| L_Start

    %% CharacterSheet initialization and subscriptions
    S_Awake -->|GetComponent| S_SubLvl
    S_Awake --> S_SubLifetime
    S_Awake --> S_SubAttr
    S_Awake --> S_SubPts
    S_Awake -->|sync local copies via getters| S_HandleLvl
    S_Awake --> S_HandleLifetime

    %% AttributeSystem subscribes to level events and notifies initial attributes
    A_Initialize -->|Subscribe to LVLSystem.OnCharacterLevelUpdate| A_Start
    A_Start --> A_NotifyAll --> A_OnAttrEvt --> S_HandleAttr

    %% LVLSystem Start initializes name/UI and seeds XP if level is 0
    L_Start -->|if GetCharacterLevel()==0| L_AddXP

    %% XP Gain Flow
    L_AddXP -->|update _currentXP, _lifetimeXp| L_OnLifetimeEvt --> S_HandleLifetime
    L_AddXP -->|if _currentXP >= _xpToNextLVL| L_LevelUp
    L_AddXP --> L_UpdateUI

    %% Level Up Flow
    L_LevelUp -->|compute overflow, carry to next level| L_OnLvlEvt
    L_OnLvlEvt --> A_HandleLvlUp --> A_UpdatePts --> A_OnPtsEvt --> S_HandlePts
    L_LevelUp -->|recurse if overflow still >= next threshold| L_LevelUp
    L_LevelUp --> L_UpdateUI

    %% Attribute Allocation Flow (player action)
    A_AddPoint -->|validate points + max| A_UpdatePts --> A_OnPtsEvt --> S_HandlePts
    A_AddPoint --> A_OnAttrEvt --> S_HandleAttr

    %% UI Updates
    L_UpdateUI -->|update nameText, lvlText, xpText, xpSlider| LVL_UIDone[(UI updated)]
    S_HandleAttr --> SHEET_UI[(Sheet attributes updated)]
    S_HandlePts --> SHEET_UI
    S_HandleLvl --> SHEET_UI
    S_HandleLifetime --> SHEET_UI

    %% Loop continues with further AddXP calls via gameplay
    LVL_UIDone --> L_AddXP
```

## Start-to-End Explanation

1. Manager setup
   - CharacterSystemManager.Awake gets LVLSystem, AttributeSystem, CharacterSheet on the same GameObject and calls `LVLSystem.Initialize(this)` and `AttributeSystem.Initialize(this)`.
   - LVLSystem.Initialize stores references to AttributeSystem and CharacterSheet.

2. Subsystem initialization
   - CharacterSheet.Awake:
     - Gets components for LVLSystem and AttributeSystem.
     - Initializes local copies of level and lifetime XP via LVLSystem getters.
     - Subscribes to LVLSystem events: `OnCharacterLevelUpdate` and `OnLifetimeXpUpdate`.
     - Subscribes to AttributeSystem events: `OnAttributeUpdate` and `OnAttributePointUpdate`.
   - AttributeSystem.Initialize:
     - Subscribes to `LVLSystem.OnCharacterLevelUpdate` so it can grant attribute points on level-up.
   - AttributeSystem.Start:
     - Calls `NotifyAllAttributeValues()` which emits current Strength/Intelligence/Agility via `OnAttributeUpdate`, allowing CharacterSheet to sync its display.

3. LVLSystem start
   - LVLSystem.Start syncs the character name from CharacterSheet for the UI.
   - If the character level is 0, seeds the system by calling `AddXP(100)`.

4. XP gain
   - `LVLSystem.AddXP(amount)` updates `_currentXP` and `_lifetimeXp`, and emits `OnLifetimeXpUpdate(lifetimeXp)` to CharacterSheet.
   - If `_currentXP >= _xpToNextLVL`, it triggers `LevelUp()`.
   - Calls `UpdateUI()` to refresh name, level text, xp text, and xp slider fill.

5. Level up
   - `LVLSystem.LevelUp()`:
     - Computes overflow XP and carries it forward to the next level.
     - Increments `_lvl`, recomputes `_xpToNextLVL`.
     - Emits `OnCharacterLevelUpdate(_lvl)` to notify downstream systems.
     - If overflow still meets the next threshold, it recurses to chain multiple level-ups in one XP grant.
     - Calls `UpdateUI()` to reflect the new level and XP threshold.
   - `AttributeSystem.HandleLevelUp(newLevel)`:
     - Adds attribute points via `UpdateAttributePoints(pool + pointsPerLevel)`, which emits `OnAttributePointUpdate(newPool)` and updates CharacterSheet’s unspent points.

6. Attribute allocation (player-driven)
   - `AttributeSystem.AddAttributePoint(attributeName)`:
     - Validates unspent points and max caps.
     - Increments the attribute’s base value.
     - Decrements the point pool via `UpdateAttributePoints(pool - 1)` and emits both `OnAttributePointUpdate` and `OnAttributeUpdate(attributeName, newValue)`.
   - CharacterSheet handlers update local Strength/Intelligence/Agility and unspent points and can log or display changes.

7. UI updates
   - LVLSystem.UpdateUI updates runtime HUD elements (nameText, lvlText, xpText, xpSlider).
   - CharacterSheet updates its internal display state for level, lifetime XP, attributes, and unspent points based on event handlers.

8. Loop continues
   - As gameplay grants more XP, `AddXP` repeats the flow: UI refresh, threshold checks, level-ups, and attribute point grants, keeping all displays and pools synchronized.

## References (permalinks)

- LVLSystem.cs:
  - [Assets/LVLSystems/LVLSystem.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/LVLSystem.cs)
- AttributeSystem.cs:
  - [Assets/LVLSystems/AttributeSystem.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/AttributeSystem.cs)
- CharSheet.cs:
  - [Assets/LVLSystems/CharSheet.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/CharSheet.cs)
- CharacterSystemManager.cs:
  - [Assets/LVLSystems/CharacterSystemManager.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/CharacterSystemManager.cs)
