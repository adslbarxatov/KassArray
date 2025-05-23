_Изменения для v 6.8.2_:
- Добавлены новые модели ККТ (май 2025 года);
- Перестроено определение моделей ККТ: теперь запрос поддерживаемых ФФД доступен по названию независимо от того, известна ли программе сигнатура ЗН;
- Реализовано отображение моделей ККТ, исключённых из реестра;
- `ФН`: добавлено ограничение на максимальный размер ПФ в папке наблюдения (для исключения зависаний на случайных больших файлах);
- `ФН`: реализована поддержка кодировок `CP1251` и `UTF8` для файлов `.tlv` (приложение Блокнот по умолчанию сохраняет текст в `UTF8`, а не в `ANSI`);
- `ФН`: дополнена и скорректирована обработка состава выгрузок `JSON`;
- `ФН`: исправлен внешний вид окон выбора папок;
- `ФН`: отправка ПФ на печать теперь сопровождается сообщением-подтверждением;
- `ФН`: возвращена кнопка «О приложении»;
- `ФН`: реализовано сохранение настроек принтера. Если результат последней печати из-под программы соответствует требованиям пользователя, он может «зафиксировать» настройки, отключив запрос принтера. Настроенный принтер в этом случае будет использоваться для печати любого задания, сформированного программой, без дополнительного подтверждения;
- Исправлен адрес отправителя чеков для ОФД «Такском» (*благодарим Юрия О. за подсказку*);
- `ФН`: дополнена и скорректирована печатная форма чеков и БСО;
- `ФН`: исправлена ошибка прямого запроса документов;
- Добавлена серия ЗН ФН `73834408` для модели `Эв36-4`;
- Исправлены неточности в руководствах к ККТ `Атол 90Ф` и `Атол 91Ф/92Ф`
