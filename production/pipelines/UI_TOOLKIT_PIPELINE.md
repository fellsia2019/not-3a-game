# Runtime UI Pipeline (Conditional)

Использовать только если проект отдельно выбирает UI Toolkit. Для мелкой правки
в утверждённой дизайн-системе HTML-прототип не обязателен.

## 1. Brief And References

Определить экран, действия, состояния, copy, target viewport/responsive rules и
acceptance. Взять из арт-направления только применимые цвет, типографику, форму,
иконки и feedback. Не добавлять состояния/controls «для полноты».

## 2. Layout And States

Утвердить wireframe и необходимые default/hover/selected/disabled,
valid/invalid, drag/cancel и error states. Для сложного или нового визуального
языка собрать финализированный HTML/CSS/JS prototype и получить approval до
переноса. Для простого экрана достаточно утверждённого mockup.

## 3. Unity Implementation

Перенести structure в UXML, visuals в USS, behavior/bindings в C#. Setup делать
в `OnEnable`, cleanup в `OnDisable`; не строить UI и не polling-ить его в
`Update`. Изолировать pointer/keyboard events от gameplay и корректно завершать
capture/drag/cancel. Объяснить неизбежные отличия от утверждённого layout.

## 4. Verification And Approval

- все заявленные функции и состояния работают;
- gameplay не получает UI input;
- viewport/responsive behavior проверены;
- нет новых Console errors;
- Unity-результат визуально сравнен с утверждённым mockup/prototype;
- человек утверждает готовый экран в Unity.

Создавать дополнительные mapping/QA документы только для реального workaround
или сложного повторяемого процесса.

