# Feature: <Name>

Status: Draft | Ready | In Progress | Done | Hold  
Owner/workstream: <one owner>  
Depends on: <contracts/features or none>  
Decision links: <production/DECISIONS.md entries or none>

## Player Outcome

Что игрок пытается сделать, зачем это полезно и какой feedback подтверждает
успех.

## Scope

- In: только обязательное поведение этой версии.
- Out: явно исключённое и post-MVP.

## Behavior

- Start/inputs:
- States and transitions:
- Success/failure/cancel:
- Rules and tunable intent:

## Contracts

- Data/config owner:
- Runtime inputs/outputs/events:
- Scene/prefab/UI/VFX/SFX requirements:
- Save/load implications:
- Other workstream handoffs:

## Edge Cases

Только реальные крайние случаи: недостаток ресурса, разрушение/отмена,
одновременные события, pause/scene reload и недостающие ссылки.

## Acceptance

- [ ] Наблюдаемое условие 1.
- [ ] Наблюдаемое условие 2.
- [ ] EditMode/PlayMode/manual checks определены пропорционально риску.
- [ ] Нет новых attributable Console errors.
- [ ] Документы/данные/scene wiring обновлены.

## Open Decisions

Только вопросы, которые меняют player-facing поведение, scope или дорогую
архитектуру. Если их нет, написать `None` и приступать.

