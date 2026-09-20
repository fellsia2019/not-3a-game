# Not 3A Game

Небольшая одиночная игра для Windows/Steam: исследование и добыча ресурсов в
изометрическом мире, строительство башен и защита трона от волн врагов.

Статус: **Stage 0 concept approved; Stage 1 graybox specification ready**.
Геймплейного кода пока нет.

## Технологии

- Unity `6000.6.2f1`;
- Universal Render Pipeline `17.6.0`;
- Input System `1.20.0`;
- целевая платформа MVP: Windows desktop, Steam;
- рабочее визуальное направление: 2D-изометрия, pixel/near-pixel, фиксированная
  ортографическая камера без вращения.

## С чего начать

1. Прочитать [AGENTS.md](AGENTS.md).
2. Сверить замысел в [gamedesign/GAME_VISION.md](gamedesign/GAME_VISION.md).
3. Взять ближайший незакрытый gate из
   [gamedesign/DEVELOPMENT_PLAN.md](gamedesign/DEVELOPMENT_PLAN.md).
4. Перед реализацией крупной механики создать её документ по
   [gamedesign/features/FEATURE_TEMPLATE.md](gamedesign/features/FEATURE_TEMPLATE.md).
5. Для отдельного чата выбрать поток работ в
   [production/WORKSTREAMS.md](production/WORKSTREAMS.md).

Визуальные референсы находятся вне `Assets/` в `concepts_visual_scene/` и не
являются игровыми ассетами. Их назначение и ограничения описаны в
[gamedesign/ART_DIRECTION.md](gamedesign/ART_DIRECTION.md). Изображения с
неподтверждёнными правами распространения намеренно не публикуются в Git.

## Открытие проекта

Открыть корень проекта через Unity Hub указанной версией Unity. Текущая сцена
`Assets/Scenes/SampleScene.unity` — стандартная тестовая сцена и не считается
архитектурой игры.

Папки `Library/`, `Temp/`, `Logs/` и сгенерированные IDE-файлы не редактируются
и не должны попадать в систему контроля версий.
