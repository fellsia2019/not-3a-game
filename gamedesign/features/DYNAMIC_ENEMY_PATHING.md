# Feature: Player-Shaped Enemy Pathing

Status: Complete and human-accepted, 2026-09-21
Owner/workstream: Foundation / integration, with Defense and Waves/AI contracts
Depends on: D-002, D-006, D-009, D-010, D-011; grid, tower placement,
health/damage
Decision links: `production/DECISIONS.md` D-009–D-011

## Player Outcome

Игрок использует башни не только как оружие, но и как стены: короткий прямой
проход менее эффективен, чем длинный зигзагообразный лабиринт. Если игрок
полностью закрывает проход, волна не застревает и не проходит сквозь постройки —
враги разрушают мешающие башни и продолжают движение к трону.

## Scope

### In For Stage 1 Rework

- клеточная walkability map на компактном репрезентативном map slice;
- один entrance, один throne goal и один тип наземного врага;
- башня занимает клетку и меняет доступный маршрут;
- поиск детерминированного пути entrance -> throne и пересчёт после изменения
  occupied cells;
- разрешённая полная блокировка: враги выбирают мешающую башню, атакуют её,
  уничтожают и возобновляют движение;
- минимальное здоровье башни и enemy damage/attack interval;
- placement preview показывает buildable/occupied/outside-map состояние;
- герой остаётся non-combat и не является целью;
- одна волна и существующие win/lose/restart правила.

### Out

- NavMesh и свободное движение вне клеточной сетки;
- летающие/копающие/телепортирующиеся типы врагов;
- несколько entrances, развилки между несколькими тронами или cooperative lanes;
- repair, tower upgrades, walls как отдельный тип постройки;
- финальный размер 100x100/200x200, chunk streaming и production performance;
- vertical traversal, уступы и режим полёта героя;
- финальный баланс здоровья, урона и стоимости.

## Behavior

### Placement And Route Shaping

- Башню можно ставить на buildable свободную клетку серой или зелёной зоны,
  кроме throne/resource/outside-map cells.
- Существование текущего пути не является условием успешной постройки: игроку
  разрешено полностью перекрыть corridor.
- Если открытый путь существует, враги используют кратчайший доступный путь с
  детерминированным tie-break, чтобы одинаковое состояние сетки давало одинаковый
  маршрут.
- Более длинный зигзаг должен наблюдаемо увеличивать пройденную дистанцию и время
  подхода к трону.

### Blocked State

- Если открытого пути до трона нет, враг переходит из движения в siege behavior.
- Целью становится построенная башня, разрушение которой восстанавливает проход
  или продвигает врага к восстановлению прохода. Враг не выбирает героя.
- Враг подходит к доступной соседней клетке цели, атакует с фиксированным
  interval и наносит урон через общий health/damage contract.
- После разрушения башни occupied cell освобождается, путь пересчитывается, и
  враги продолжают движение. Волна не должна зависать в blocked state.
- Если несколько blockers эквивалентны, выбор стабилен и воспроизводим; точный
  tie-break является технической деталью, а не отдельной player-facing системой.

### Runtime Changes

- Изменение occupied cells инвалидирует route revision один раз; каждый враг не
  запускает полный поиск пути каждый кадр.
- Размещение во время волны следует P-002. Pathing contract обязан поддерживать
  безопасный пересчёт mid-wave, даже если UI временно разрешает строительство
  только в preparation.
- Restart очищает башни, их здоровье, navigation occupancy, route revisions и
  blocked targets.

## Contracts

- **Grid/navigation owner:** Foundation/Waves хранит buildable, walkable и
  occupied cell state и выдаёт path/blocker result.
- **Placement input:** Defense запрашивает cell validation, списывает ресурсы
  только после успешного confirm и затем публикует occupancy change.
- **Tower lifecycle:** Defense владеет tower health; spawn/destruction атомарно
  добавляет/удаляет occupied cell.
- **Enemy output:** Waves/AI следует path result либо атакует returned blocker;
  throne damage остаётся текущим loss contract.
- **Data:** costs, hit points, damage, intervals и grid dimensions являются
  prototype data, не дублируются как durable balance в Markdown.
- **Save/load:** Stage 1 не сохраняет незавершённый забег; будущий save обязан
  восстановить occupancy до запуска pathfinding.

## Edge Cases

- башня поставлена mid-wave на следующую клетку нескольких врагов;
- одновременно разрушены blocker и последний enemy;
- blocker исчез до завершения attack wind-up;
- несколько стен полностью перекрывают corridor;
- враг находится внутри клетки, ставшей occupied;
- placement отменён или не оплачен;
- throne/entrance случайно помечены occupied;
- restart во время movement и siege states.

## Acceptance

- [x] На открытой сетке враг находит путь entrance -> throne.
- [x] Одна и та же сетка всегда возвращает один и тот же путь.
- [x] Размещение башен зигзагом увеличивает длину пути относительно прямого.
- [x] Башни нельзя ставить на throne/resource/outside-map/occupied cells; отказ
  не списывает ресурсы.
- [x] Полная стена разрешается и переводит врагов в атаку на башню, а не в
  остановку или проход сквозь неё.
- [x] После разрушения blocker occupancy освобождается, путь восстанавливается и
  волна доходит до win/lose terminal state.
- [x] Герой не получает health/aggro/attack role.
- [x] EditMode покрывает pathfinding, deterministic tie-break, replanning и
  blocker selection.
- [x] PlayMode покрывает full block, tower destruction, resume, win/loss и
  restart; zigzag/path length покрыт детерминированным EditMode тестом.
- [x] Windows Development Build собран и запущен без новых startup errors;
  gameplay behavior дополнительно закрыт PlayMode.
- [x] Canonical plan, graybox spec и handoff обновлены.

## Open Decisions

- P-002: разрешено ли размещение новых башен во время активной волны.
- P-008: целевой production-размер карты после компактного Stage 1 slice.
