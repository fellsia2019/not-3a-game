# Hero progression

## D-085 / D-086 — approved first playable slice

2026-10-19: the owner approved the fullscreen development page, HUD, Blink/XP
and the first three passives. The replacement level-up polygon_v3 was then
explicitly approved after shape, blur and text revisions. The first playable slice and approved visual implementation are complete.
Qualification is recorded in DEVELOPMENT_PLAN.md and WORKSTREAMS.md.
The extra attack spell from the older proposal is deferred; the automatic bow
remains single-target and dense matched-tier hordes still require towers.

Current contracts:

- Confirmed deaths in the shared hero/tower enemy population award XP once,
  globally while the hero explores. Arrival, despawn, reset and restoration
  award nothing. There is no XP pickup or last-hit contest.
- Hero starts at level1 with BlinkI; cap100. Later levels award hero points.
  Consecutive Blink/passive ranks spend available points and stop at their cap.
  XP thresholds, kill rewards, point costs, cooldown and personal bonuses are
  initial playable tuning in the Unity catalog, not a fixed balance inferred
  from concept numbers. Broader12/48/36 content budgets below remain proposals.
- Blink uses the confirmed300/500/800/1000 display ranges, converted by the
  catalog to current world units. Q casts toward the world cursor; clicking
  the first HUD slot arms targeting, then world LMB casts. RMB/Esc cancels.
  Distant cursor clamps to reach. Landing requires the full hero footprint
  inside world bounds and clear of resources/buildings; invalid landings
  consume no cooldown. There is no attack, splash or mana cost.
- Three personal passives have three ranks each: «Точный удар» adds integer
  damage to one bow target; «Быстрые руки» improves personal attack cadence;
  «Лёгкий шаг» improves walking speed independently of the F3 multiplier.
  Cards show current → next bonuses. They do not improve towers or tools.
- O is the existing fullscreen development page. The v4 dock keeps minimap,
  separate navigation and ten-cell capacity, with one Blink icon, portrait
  LVL, point badge and numeric orange XP. HP/MP remain inactive0/0. At cap the
  XP bar displays its maximum state rather than an inactive0/0 source.
- Blink has cyan valid landing/ghost/range, red blocked mark+X and brief local
  departure/arrival wisps. Marks render above terrain and obstacle art. Compact
  orange XP feedback stays at the live bar and yields to the level celebration.
- D-086 level-up is a brief spectral gold/ivory polygonal shield-seal: crisp
  varied angled central contour, soft diffuse energy waves to the sides. Text
  has only «НОВЫЙ УРОВЕНЬ», the level numeral and awarded points; no O/Развитие
  prompt. Multiple awards coalesce to the latest level and total earned points.
  The noninteractive overlay does not block movement/Blink or pause the world.
  Pause/modal UI freezes the same animation; reopening a page never replays it.
  Reset/retry/terminal state clears transient feedback.

Rules/data/lifetime: Stage2HeroProgression(.cs), Stage2HeroProgressionCatalog
and Stage2HeroProgressionDriver in the existing Integration assembly. Stage3
opts in through PlayerInterfaceBinding.Progression; no new global manager,
package, UI stack or scene migration. Data source is
Assets/Stage3/HeroProgression/Resources/Stage3HeroProgression.asset. Existing
Stage2 scenes do not receive the opt-in driver. Combat and mover apply personal
bonuses without mutating shared catalogs or tower stats.

D-062 checkpoint captures XP, level, points, all ranks and Blink cooldown at
BeforeWaveStarted. Retry restores these and derived stats without XP/level-up
reward events; failed-future kills and purchases do not survive restoration.

Audio: Resources/Stage3HeroLevelUpSound currently contains only the accepted
three-part female chord (bone666138, Freesound274121, CC0) and stereo echoes.
The owner explicitly postponed the hammer and kept the reward cue without it.
All four impact experiments were rejected and are absent from runtime. The
owner then selected the third supplied reference: Elden Ring Player Level Up
(https://www.youtube.com/watch?v=k1avFgP63Ng). Original candidate v5 was also
rejected as entirely unlike that reference; it was never imported. The prior
technical waveform checks did not include an auditory comparison and do not
establish similarity. Latest owner confirmation keeps the accepted voice/echo without an impact.
The impact task is deferred, not an open requirement of this playable slice.
Any future attempt requires actual auditory comparison; technical WAV checks
alone do not qualify similarity. No reference-game audio
is downloaded, sampled or shipped. Source bytes, license, reproducible numpy mix script and previews
are preserved in art/source/hero_levelup_audio_2026-10-19/.
PCM is3.2s, stereo44.1kHz16-bit, peak−1.51dBFS, without clipped samples.
SetAudioClip remains available for later production replacement.
Original prompts, references, rights, approval and PNG hashes:
art/source/hero_progression_2026-10-19_generation.json.

## Prior broader design and proposed budget

Status: design revision D-074 with D-075 campaign/equipment clarification,
2026-10-19; replaces the D-073 proposed spell
roster. Confirmed: level cap 100, cursor Blink, low-splash exploration hero and
towers responsible for dense hordes. Counts and point budgets below are
proposals, not implemented mechanics or approved art. Owner: current Stage 3
Game Design chat. Dependencies: HERO_COMBAT.md, PORTAL_CONTROL.md,
EQUIPMENT_AND_RUNES.md, STAGE2_ECONOMY.md, STAGE2_RUN_STATE.md and future hero
health/open-world enemies.

## Role and scope

Герой исследует мир, добывает ресурсы и зачищает небольшие группы в заброшенных
местах. Башни уничтожают массовые ночные орды. Магия героя преимущественно
одиночная: точный умеренный урон, короткий контроль одной цели, усиление личной
атаки и выживание. Небольшой огненный сплеш не превращается с рангами в средство
зачистки дороги.
По D-075 развитие готовит к трём сильным боссам, после победы над всеми
кампания заканчивается. Их место/режим боя ещё открыты: BOSSES_AND_VICTORY.md.

Предел — **100 уровней героя**, расширяемый позже. Это не 100 обязательных
ночей и не обещание сотен часов контента. Уровень героя, освоенной орды,
календарный день, мастерство профессии и уровень предмета — разные величины.
Уровень героя не повышает текущих врагов автоматически.

Проверять роль нужно при сопоставимом развитии героя, базы и ночной угрозы:
сочетание доступных навыков, скорости атаки, экипировки и рун не позволяет
герою одному стабильно зачищать соответствующую плотную орду. Подготовленная
оборона имеет путь к победе. Переросший старую угрозу герой может легко
побеждать старых врагов; искусственная неуязвимость к герою не нужна.
Первый лёгкий бой по D-064 остаётся доступен стартовому луку.

## Proposed complete budget

| Система | Планка |
| --- | --- |
| Уровень героя | 1–100; подтверждено владельцем |
| Библиотека активных умений | 12 умений, по 4 ранга |
| Одновременно доступные активные | Предложено 6 мест, одно занимает блинк; окончательный предел открыт |
| Общие пассивные таланты | 48 узлов: 3 ветки по 16, до 3 рангов у узла |
| Профессии | Рудокоп, лесоруб, исследователь; своё мастерство 1–100 |
| Таланты профессий | По 12 узлов с 3 рангами: всего 36 узлов |
| Очки героя | Блинк I бесплатно; по 1 очку за уровни 2–100, всего 99 |
| Очки профессии | По 1 за каждые 5 уровней мастерства; 20 на профессию |

Кроме подтверждённого предела героя, это рекомендуемый бюджет содержания,
а не требование изготовить всё до первого билда. Характеристики и пороги XP
при реализации хранятся в Unity data; таблица задаёт объём и правила выбора.

Активные ранги и общие пассивки расходуют очки героя. Возможны 47 платных
рангов активных и 144 ранга пассивок: **191 улучшение при 99 очках**.
Максимально изучить всё нельзя. В каждой профессии 36 возможных рангов при
20 очках. Расширение библиотеки не выдаёт бесплатные ранги или места панели.

D-078: интерфейс проектируется с возможностью расширить панель до десяти
ячеек (в концепте 2×5). Это пожелание к вместимости HUD, а не утверждение
десяти одновременно доступных умений, новых клавиш или изменения очков.
Число экипированных умений согласовать отдельно; библиотека/бюджеты выше
остаются предложениями. Числовые шкалы HP/MP/XP требуют реальных игровых
источников; значения на v4-картинках только демонстрируют оформление. В D-078
интерфейс уже показывает отключённое 0/0 + «Не открыто», без игрового баланса.

## Active abilities

Автоматическая базовая атака и кирка/топор мест на панели не занимают.
Герой универсален, стартовый лук не фиксирует класс. Названия рабочие;
клавиши, иконки и оформление требуют отдельного визуального согласования.

| Умение | Функция и предел |
| --- | --- |
| Блинк | Скачок к курсору; четыре ступени дальности |
| Чародейский снаряд | Точный умеренный удар по одной цели без взрыва |
| Огненный снаряд | Удар и горение основной цели; слабый побочный урон максимум двум соседним |
| Ледяная игла | Одна цель и краткое замедление; без ледяного поля |
| Метка охотника | Короткое усиление личных попаданий по одной цели |
| Сосредоточенность | Временное ускорение личной атаки с общим пределом скорости |
| Грозовой удар | Одиночный удар с прерыванием подходящего действия, без цепи |
| Попутный ветер | Короткое ускорение ходьбы для перемещения и отхода |
| Каменная кожа | Временное снижение входящего урона герою |
| Личный барьер | Краткое поглощение урона герою |
| Восстановление | Лечение героя; без лечения трона или башен |
| Поток маны | Личное восстановление маны; занимает место атакующего умения |

Первые шесть — начальный каталог, ещё шесть расширяют выбор позже. Защитные
умения вводятся вместе со здоровьем героя и врагами открытого мира. Старые
предложения «Ледяная печать», «Цепная молния» и рост площади вспышки заменены.
Полного контроля орды, цепей на десятки целей и взрывов от каждой смерти нет.

У каждого умения четыре ранга. Атакующие ранги усиливают основную цель или
эффективность; число побочных целей не растёт. Общий урон группе ограничен:
даже три цели не получают каждая полный основной удар. Скорость атаки,
перезарядки, предметы и руны проверяются вместе: одного запрета большого
сплеша недостаточно, если одиночные атаки становятся чрезмерно частыми.
Магия использует личную ману и перезарядки; исключение для базового блинка
предложено ниже. Не допускается каст нескольких взаимоисключающих действий
одним нажатием или обход темпа через смену набора на панели.

## Blink

Подтверждено: короткий скачок в точку курсора с первого уровня, четыре ранга
дальности **300/500/800/1000** по шкале спелла. Круги остаются для дальних
переходов. Рекомендация: первый ранг бесплатно, только перезарядка без маны.
Необходимое перемещение не конкурирует с первым боевым выбором.

Предлагаемый масштаб сетки — 100 единиц шкалы на клетку, то есть 3/5/8/10
клеток. Проверить по игровому кадру и времени ходьбы до фиксации в data.
Дальний курсор ограничивает скачок максимальной дальностью в его направлении.
Приземление проверяет полный радиус героя, границы и занятость земли.
Недопустимая точка не расходует перезарядку; пересечь небольшую преграду
при свободном приземлении допустимо. Предпросмотр показывает реальный предел.

За полноэкранным UI, в паузе и при поражении каст заблокирован. Скачок
прерывает незаконченный удар и размещение; продолжение добычи требует свежего
E. Один ввод не подтверждает спелл и постройку одновременно. Поздняя развилка
может дать второй заряд либо более частый одиночный скачок. Заряды входят
в общий бюджет силы и checkpoint; дальность блинка не усиливает прочую магию.

## Passive talents

| Ветка | 16 узлов; основные направления |
| --- | --- |
| Точная атака | Одиночный физический/магический урон, управляемая скорость атаки, горение основной цели, личная метка и эффективность против элиты |
| Выживание | Здоровье, личная защита, барьер и восстановление; после появления соответствующих механик |
| Подвижность и магия | Ходьба, блинк, мана, эффективность каста и одиночного контроля, альтернативные вершины перемещения |

Это бюджет трёх веток, а не 48 уже определённых процентных бонусов. Узлы
проектируются с работающими системами, короткими путями и несколькими
взаимоисключающими вершинами. Фиктивные характеристики и скрытый обход
ограничения площади не добавляются. Постоянные усиления башен относятся
к исследованиям и отдельным рунам обороны, не к личному урону или блинку.

## Professions and tools

Мастерство рудокопа/лесоруба растёт за фактически извлечённый материал,
исследователя — за первое обнаружение полезного места и завершённое открытие.
Бег по кругу, пустой удар и повторное открытие точки опыта не дают.

У каждой профессии три направления по четыре узла, до трёх рангов у каждого.
Для рудокопа/лесоруба — сила удара, темп работы и удобство похода за ресурсом.
Для исследователя — перемещение, обнаружение и польза первого исследования
места. Вес, ремонт и прочность инструментов не вводятся ради заполнения ветки.

Сила инструмента означает количество извлечённого запаса. При силе 3 удар
по остатку 35 сразу отдаёт 3 и оставляет 32; по остатку 2 отдаёт 2 и истощает
узел. Визуальная стадия и коллизия следуют остатку. Незавершённый удар
не платит. Улучшение не создаёт материал сверх запаса. Мастерство и качество
предмета-инструмента — разные источники эффективности, проверяемые в сумме.
По D-075 кузница теперь активирована как будущий путь создания/улучшения
самих кирок и топоров, отдельно от мастерства: EQUIPMENT_AND_RUNES.md.
Улучшения автоматических генераторов — отдельные технологии/вложения по
RESOURCE_PRODUCTION_BUILDINGS.md. Ремонт и производственные цепочки не выбраны.

## Level bands and experience

| Уровни героя | Ориентир наполнения |
| --- | --- |
| 1–20 | Блинк, базовая атака, выбор из первых шести активных, понятные пассивки, начало профессий |
| 21–40 | Часть альтернативных активных, сочетания талантов, небольшие руины и предметы |
| 41–60 | Вся библиотека из 12 активных; выбор шести, основа своего билда |
| 61–80 | Углубление специализации, трудные места, качество снаряжения/рун, рост базы |
| 81–100 | Завершение билда/обороны, серьёзная подготовка к боссам; без выдачи всех талантов разом |

Это ориентиры, не часы до уровня или фиксированный ранг предмета на пороге.
Точные XP требуют измерений реального цикла. Сто полос опыта не заменяют
содержательных мест, решений и развития базы.

Боевой опыт идёт за подтверждённые убийства: свои башни дают герою столько
же, сколько его атака, без перехвата последнего удара. Рекомендация:
оборона начисляет общий опыт и во время исследования мира, чтобы игрок
не стоял у портала постоянно. Добыча развивает прежде всего свою профессию;
полезные первые открытия и завершение мест могут давать опыт героя.
По D-075 основной боевой XP идёт через массовый TD; мир даёт меньше XP,
зато более вероятные ценные находки. Сравнивать нужно награду за время игры.

Дневной фарм повторяет освоенный уровень орды. Опыт сильно переросших врагов
рекомендуется постепенно снижать; кривая ещё не выбрана. Источник определяет
уровень награды. Старый лёгкий портал не выдаёт предметы 100 уровня только
из-за уровня героя.

## Checkpoints and first implementation

Уровни, очки, ранги, профессии, мана, заряды, перезарядки и временные эффекты
принадлежат текущему миру и входят в снимок D-062. Провал не сохраняет XP
проигранного будущего. Постоянная руна означает постоянство в текущем мире,
а не roguelite-метапрогрессию. Дневные врата и живые остатки врагов требуют
расширения checkpoint по PORTAL_CONTROL.md до реализации фарма.

По текущему D-085 первый срез использует каталог с пределом 100, блинк,
первые три предложенные пассивки и проверяемый XP. Дополнительный одиночный
атакующий спелл следует позже, отдельно от этого среза. Профессии
и следующий каталог добавляются по DEVELOPMENT_PLAN.md. Все 12/48/36 узлов
не нужны до первого игрового теста.

Приёмка: XP/очки без двойных наград; полезный выбор; допустимое приземление
блинка; откат всей прокачки; успех против небольшой группы и необходимость
обороны против сопоставимой ночи. Будущая внешность проходит ImageGen,
одобрение владельца и реализацию GPT-6 Astra. Здесь XP/UI/магия не внедряются.

## References

[Last Epoch Skill Specialization](https://support.lastepoch.com/hc/en-us/articles/46363203944859-Skill-Specialization)
— референс ограниченного набора и развития знакомых умений.
[New World Character Progression](https://www.newworld.com/en-gb/news/articles/character-progression)
— опубликованный разработчиком референс раздельного боевого и мирного развития.
Это источники принципов, а не наших количеств или текущего баланса этих игр.
WoW/Dota названы владельцем ориентиром управления блинком.
