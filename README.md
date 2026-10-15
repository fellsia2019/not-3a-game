# Not 3A Game

Изометрическая tower defense игра: герой добывает дерево, камень и металл,
строит башни и защищает трон. Целевая платформа первого выпуска — Windows;
разработку и просмотр проекта можно продолжить в Unity Editor на Mac. Сейчас
цель — один короткий понятный забег. Долгосрочный замысел и граница первого
играбельного билда описаны в [Game Vision](gamedesign/GAME_VISION.md).

## Состояние

Unity `6000.6.2f1`, URP, Input System. Stage 2 завершён. Текущая играбельная
визуальная проба Stage 3 — [Stage3Rapid3DGamePilot](Assets/Scenes/Stage3Rapid3DGamePilot.unity):
диагональная карта 100×100, мультяшные спрайты скелета, портала, цели, башен,
трава и брусчатка по подборке B. Название сцены историческое: в ней сейчас
используются и 2D-спрайты. Проверка вида игроком и Stage 3 gate ещё открыты;
Stage 4 не начат. Точное состояние — в [плане](gamedesign/DEVELOPMENT_PLAN.md)
и верхнем handoff [Workstreams](production/WORKSTREAMS.md).

## Быстрый вход

1. [Правила проекта](AGENTS.md) и [текущий путь разработки](gamedesign/DEVELOPMENT_PLAN.md).
2. [Текущий геймдизайн](reports/game-design.html) — визуальное представление vision.
3. [Roadmap и рекомендации по моделям](reports/development-roadmap.html).
4. [Art Direction](gamedesign/ART_DIRECTION.md) и [решения](production/DECISIONS.md).
5. [Workstreams](production/WORKSTREAMS.md) — только текущий блок и handoff
   затрагиваемой системы; старые claims остаются историей.

## Продолжение на Mac

1. На Mac клонировать актуальную ветку `main` из публичного репозитория
   `https://github.com/fellsia2019/not-3a-game` (например,
   `git clone https://github.com/fellsia2019/not-3a-game.git`). В Git сохранены
   `Assets/` с `.meta`, `Packages/`, `ProjectSettings/`, `art/`, вся проектная
   документация и `concepts_visual_scene/`. Для дальнейших обновлений в
   этой папке использовать обычный `git pull`.
2. Установить Unity Editor **6000.6.2f1** через Unity Hub и открыть клонированную папку
   проекта. Unity восстановит `Library/` и пакеты из `Packages/manifest.json`
   и `Packages/packages-lock.json`; первый импорт может занять время.
3. Открыть сцену `Assets/Scenes/Stage3Rapid3DGamePilot.unity` и запустить
   Play. Для привычного обзора есть меню `Tools > Stage 3 > Cartoon World >
   Open Live Review`. Если меню ещё не появилось, дождаться импорта и
   компиляции скриптов.
4. Проверить Game view и Console. Сборки в `Builds/Windows/` и журналы в
   `Artifacts/` локальны и в перенос не входят. Разработка на Mac не меняет
   принятую целевую платформу Windows; выпуск или QA сборки macOS отдельно
   не утверждались и не проверялись.

Оригинальная подборка B —
`art/concepts/sprite_style_review_2026-10-15/b_cartoon_2d.png`;
приложенный позднее вариант имеет те же пиксели. Остальные концепты,
промпты, файлы происхождения генераций и кадры из игры — в `art/`.
`art/source/*provenance.json` может содержать старые абсолютные Windows-пути
к файлам генератора; используемые PNG уже находятся в `Assets/`, а исходные
концепты — в `art/`, поэтому эти пути на Mac не требуются. `art/review/`
содержит статичные кадры проверок; ненужные анимированные записи удалены по
запросу владельца.

Исторические сторонние референсы в `concepts_visual_scene/` находятся вне
`Assets/` и не являются игровыми ассетами. Их источники/лицензии не подтверждены;
список каждого файла и его статус — в [индексе](concepts_visual_scene/README.md).
То же относится к присланному референсу в `art/references/`. По явному решению
владельца D-044 эти материалы сохранены в публичном GitHub вместе с проектом,
но для включения в игру нужна отдельная проверка прав. Генерируемые `Library/`, `Temp/`,
`Logs/`, `Builds/` и `Artifacts/` восстанавливаются или не нужны на другом
компьютере.
