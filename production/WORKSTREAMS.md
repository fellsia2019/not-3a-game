# Workstreams And Chat Handoffs

Этот файл — единая координация отдельных чатов. Один активный владелец на
систему/сцену/контракт; параллельные агенты не редактируют одни и те же файлы.

## Recommended Order

1. **Design coordinator:** vision, decisions, stage gates, feature briefs.
2. **Foundation:** isometric camera/movement, grid, sorting, input, scene shell.
3. **Economy:** resource nodes, gathering, inventory/cost contracts.
4. **Defense:** placement, towers, targeting, damage and throne health.
5. **Waves/AI:** route, spawn schedule, enemy movement, win/lose orchestration.
6. **UI/UX:** HUD, building flow, warnings, menus and accessibility.
7. **Art/content:** tile/sprite pipeline, animation, VFX/audio and content data.
8. **Platform/QA:** saves/settings, tests, Windows builds, Steam and release QA.

На Stage 1 Foundation владеет интеграционной сценой. Economy, Defense и
Waves/AI начинают параллельно только после согласования узких data contracts;
иначе работа последовательно интегрируется владельцем сцены. UI и art не должны
блокировать graybox.

## Generic Prompt For A New Chat

```text
Прочитай AGENTS.md, gamedesign/GAME_VISION.md,
gamedesign/DEVELOPMENT_PLAN.md, production/DECISIONS.md и этот файл.
Ты владеешь workstream <NAME> и задачей <TASK>. Сначала изучи проект и
соответствующий feature spec; не придумывай player-facing правила. Зафиксируй
goal, scope, contracts, acceptance и unresolved decisions. После готовности
реализуй минимальный coherent slice, проверь его и обнови canonical plan/spec и
Handoff Register. Не изменяй поверхности других активных workstreams без
явного handoff.
```

## Handoff Entry Format

```md
### YYYY-MM-DD — <source> -> <target/status>
- Outcome: <что теперь работает>.
- Contract/files: <что изменено и на что можно опираться>.
- Verification: <tests/build/manual; degraded reason if any>.
- Remaining: <явно вне завершённой задачи>.
```

## Handoff Register

### 2026-09-20 — Project setup -> All workstreams

- Outcome: создана базовая документация, scope MVP и правила работы чатов.
- Contract/files: `AGENTS.md`, `gamedesign/`, `production/`, `.cursor/rules/`.
- Verification: структура и ссылки проверяются до закрытия Stage 0.
- Remaining: настроить version control и реализовать Stage 1 graybox spec.

### 2026-09-20 — Design approval -> Foundation

- Outcome: GAME_VISION утверждён; герой не сражается, обучение и progression
  инструментов перенесены после MVP.
- Contract/files: `gamedesign/GAME_VISION.md`, `production/DECISIONS.md`,
  `gamedesign/features/GRAYBOX_CORE_LOOP.md`,
  `gamedesign/features/OBJECTIVES.md`.
- Verification: решения отражены в canonical vision/plan; Git baseline
  `0016bd5` опубликован в `origin/main`.
- Remaining: реализация интеграционного Unity graybox одним Foundation owner.
