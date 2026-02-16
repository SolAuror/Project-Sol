\## Flow Diagram



```mermaid

flowchart TD

&nbsp;   %% Subsystems

&nbsp;   subgraph Manager\[CharacterSystemManager.cs]

&nbsp;     M\_Awake\[Awake()]

&nbsp;     M\_InitLVL\[LevelSystem = GetComponent<LVLSystem>]

&nbsp;     M\_InitAttr\[AttributeSystem = GetComponent<AttributeSystem>]

&nbsp;     M\_InitSheet\[Sheet = GetComponent<CharacterSheet>]

&nbsp;     M\_Wire\[LVLSystem.Initialize(this)\\nAttributeSystem.Initialize(this)]

&nbsp;   end



&nbsp;   subgraph LVL\[LVLSystem.cs]

&nbsp;     L\_Initialize\[Initialize(CharacterSystemManager)]

&nbsp;     L\_Start\[Start()]

&nbsp;     L\_AddXP\[AddXP(amount)]

&nbsp;     L\_LevelUp\[LevelUp()]

&nbsp;     L\_UpdateUI\[UpdateUI()]

&nbsp;     L\_OnLvlEvt\[OnCharacterLevelUpdate (event<int>)]

&nbsp;     L\_OnLifetimeEvt\[OnLifetimeXpUpdate (event<int>)]

&nbsp;   end



&nbsp;   subgraph ATTR\[AttributeSystem.cs]

&nbsp;     A\_Initialize\[Initialize(CharacterSystemManager)]

&nbsp;     A\_Start\[Start()]

&nbsp;     A\_NotifyAll\[NotifyAllAttributeValues()]

&nbsp;     A\_HandleLvlUp\[HandleLevelUp(newLevel)]

&nbsp;     A\_UpdatePts\[UpdateAttributePoints(newPoints)]

&nbsp;     A\_AddPoint\[AddAttributePoint(attributeName)]

&nbsp;     A\_OnAttrEvt\[OnAttributeUpdate (event<string,int>)]

&nbsp;     A\_OnPtsEvt\[OnAttributePointUpdate (event<int>)]

&nbsp;   end



&nbsp;   subgraph SHEET\[CharSheet.cs]

&nbsp;     S\_Awake\[Awake()]

&nbsp;     S\_SubLvl\[Susbcribe: OnCharacterLevelUpdate]

&nbsp;     S\_SubLifetime\[Susbcribe: OnLifetimeXpUpdate]

&nbsp;     S\_SubAttr\[Susbcribe: OnAttributeUpdate]

&nbsp;     S\_SubPts\[Susbcribe: OnAttributePointUpdate]

&nbsp;     S\_HandleLvl\[HandleLevelUpdate(newLevel)]

&nbsp;     S\_HandleLifetime\[HandleLifetimeXpUpdate(newLifetimeXp)]

&nbsp;     S\_HandleAttr\[HandleAttributeUpdate(name,value)]

&nbsp;     S\_HandlePts\[HandleAttributePointUpdate(newPoints)]

&nbsp;   end



&nbsp;   %% Initialization wiring

&nbsp;   M\_Awake --> M\_InitLVL --> M\_InitAttr --> M\_InitSheet --> M\_Wire

&nbsp;   M\_Wire --> L\_Initialize

&nbsp;   M\_Wire --> A\_Initialize



&nbsp;   %% LVLSystem Initialize references to other systems

&nbsp;   L\_Initialize -->|refs set: AttributeSystem, CharacterSheet| L\_Start



&nbsp;   %% CharacterSheet initialization and subscriptions

&nbsp;   S\_Awake -->|GetComponent| S\_SubLvl

&nbsp;   S\_Awake --> S\_SubLifetime

&nbsp;   S\_Awake --> S\_SubAttr

&nbsp;   S\_Awake --> S\_SubPts

&nbsp;   S\_Awake -->|sync local copies via getters| S\_HandleLvl

&nbsp;   S\_Awake --> S\_HandleLifetime



&nbsp;   %% AttributeSystem subscribes to level events and notifies initial attributes

&nbsp;   A\_Initialize -->|Subscribe to LVLSystem.OnCharacterLevelUpdate| A\_Start

&nbsp;   A\_Start --> A\_NotifyAll --> A\_OnAttrEvt --> S\_HandleAttr



&nbsp;   %% LVLSystem Start initializes name/UI and seeds XP if level is 0

&nbsp;   L\_Start -->|if GetCharacterLevel()==0| L\_AddXP



&nbsp;   %% XP Gain Flow

&nbsp;   L\_AddXP -->|update \_currentXP, \_lifetimeXp| L\_OnLifetimeEvt --> S\_HandleLifetime

&nbsp;   L\_AddXP -->|if \_currentXP >= \_xpToNextLVL| L\_LevelUp

&nbsp;   L\_AddXP --> L\_UpdateUI



&nbsp;   %% Level Up Flow

&nbsp;   L\_LevelUp -->|compute overflow, carry to next level| L\_OnLvlEvt

&nbsp;   L\_OnLvlEvt --> A\_HandleLvlUp --> A\_UpdatePts --> A\_OnPtsEvt --> S\_HandlePts

&nbsp;   L\_LevelUp -->|recurse if overflow still >= next threshold| L\_LevelUp

&nbsp;   L\_LevelUp --> L\_UpdateUI



&nbsp;   %% Attribute Allocation Flow (player action)

&nbsp;   A\_AddPoint -->|validate points + max| A\_UpdatePts --> A\_OnPtsEvt --> S\_HandlePts

&nbsp;   A\_AddPoint --> A\_OnAttrEvt --> S\_HandleAttr



&nbsp;   %% UI Updates

&nbsp;   L\_UpdateUI -->|update nameText, lvlText, xpText, xpSlider| LVL\_UIDone\[(UI updated)]

&nbsp;   S\_HandleAttr --> SHEET\_UI\[(Sheet attributes updated)]

&nbsp;   S\_HandlePts --> SHEET\_UI

&nbsp;   S\_HandleLvl --> SHEET\_UI

&nbsp;   S\_HandleLifetime --> SHEET\_UI



&nbsp;   %% Loop continues with further AddXP calls via gameplay

&nbsp;   LVL\_UIDone --> L\_AddXP

```



\## Start-to-End Explanation



1\. Manager setup

&nbsp;  - CharacterSystemManager.Awake gets LVLSystem, AttributeSystem, CharacterSheet on the same GameObject and calls `LVLSystem.Initialize(this)` and `AttributeSystem.Initialize(this)`.

&nbsp;  - LVLSystem.Initialize stores references to AttributeSystem and CharacterSheet.



2\. Subsystem initialization

&nbsp;  - CharacterSheet.Awake:

&nbsp;    - Gets components for LVLSystem and AttributeSystem.

&nbsp;    - Initializes local copies of level and lifetime XP via LVLSystem getters.

&nbsp;    - Subscribes to LVLSystem events: `OnCharacterLevelUpdate` and `OnLifetimeXpUpdate`.

&nbsp;    - Subscribes to AttributeSystem events: `OnAttributeUpdate` and `OnAttributePointUpdate`.

&nbsp;  - AttributeSystem.Initialize:

&nbsp;    - Subscribes to `LVLSystem.OnCharacterLevelUpdate` so it can grant attribute points on level-up.

&nbsp;  - AttributeSystem.Start:

&nbsp;    - Calls `NotifyAllAttributeValues()` which emits current Strength/Intelligence/Agility via `OnAttributeUpdate`, allowing CharacterSheet to sync its display.



3\. LVLSystem start

&nbsp;  - LVLSystem.Start syncs the character name from CharacterSheet for the UI.

&nbsp;  - If the character level is 0, seeds the system by calling `AddXP(100)`.



4\. XP gain

&nbsp;  - `LVLSystem.AddXP(amount)` updates `\_currentXP` and `\_lifetimeXp`, and emits `OnLifetimeXpUpdate(lifetimeXp)` to CharacterSheet.

&nbsp;  - If `\_currentXP >= \_xpToNextLVL`, it triggers `LevelUp()`.

&nbsp;  - Calls `UpdateUI()` to refresh name, level text, xp text, and xp slider fill.



5\. Level up

&nbsp;  - `LVLSystem.LevelUp()`:

&nbsp;    - Computes overflow XP and carries it forward to the next level.

&nbsp;    - Increments `\_lvl`, recomputes `\_xpToNextLVL`.

&nbsp;    - Emits `OnCharacterLevelUpdate(\_lvl)` to notify downstream systems.

&nbsp;    - If overflow still meets the next threshold, it recurses to chain multiple level-ups in one XP grant.

&nbsp;    - Calls `UpdateUI()` to reflect the new level and XP threshold.

&nbsp;  - `AttributeSystem.HandleLevelUp(newLevel)`:

&nbsp;    - Adds attribute points via `UpdateAttributePoints(pool + pointsPerLevel)`, which emits `OnAttributePointUpdate(newPool)` and updates CharacterSheet’s unspent points.



6\. Attribute allocation (player-driven)

&nbsp;  - `AttributeSystem.AddAttributePoint(attributeName)`:

&nbsp;    - Validates unspent points and max caps.

&nbsp;    - Increments the attribute’s base value.

&nbsp;    - Decrements the point pool via `UpdateAttributePoints(pool - 1)` and emits both `OnAttributePointUpdate` and `OnAttributeUpdate(attributeName, newValue)`.

&nbsp;  - CharacterSheet handlers update local Strength/Intelligence/Agility and unspent points and can log or display changes.



7\. UI updates

&nbsp;  - LVLSystem.UpdateUI updates runtime HUD elements (nameText, lvlText, xpText, xpSlider).

&nbsp;  - CharacterSheet updates its internal display state for level, lifetime XP, attributes, and unspent points based on event handlers.



8\. Loop continues

&nbsp;  - As gameplay grants more XP, `AddXP` repeats the flow: UI refresh, threshold checks, level-ups, and attribute point grants, keeping all displays and pools synchronized.



\## References (permalinks)



\- LVLSystem.cs:

&nbsp; - \[Assets/LVLSystems/LVLSystem.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/LVLSystem.cs)

\- AttributeSystem.cs:

&nbsp; - \[Assets/LVLSystems/AttributeSystem.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/AttributeSystem.cs)

\- CharSheet.cs:

&nbsp; - \[Assets/LVLSystems/CharSheet.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/CharSheet.cs)

\- CharacterSystemManager.cs (requested as LevelSystemManager.cs):

&nbsp; - \[Assets/LVLSystems/CharacterSystemManager.cs](https://github.com/SolAuror/Project-Sol/blob/6047473f1e2ae746f84ec6076f7bab1871ca4d89/Assets/LVLSystems/CharacterSystemManager.cs)

