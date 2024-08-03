# Тестовое задание на C# Unity Developer

Разработать маленький сервис, который будет принимать и отправлять события на сервер аналитики.

## Примеры событий

- Старт уровня
- Получение награды
- Трата монеток

## Платформы проекта

- Android
- WebGL

## Ориентировочное время на выполнение

2-3 часа (время не фиксированное, вполне можно потратить и больше, если есть желание).

## Формат события

Событие - это объект, включающий в себя поля:

- `type` - тип события, строка
- `data` - данные события, строка

## Формат запроса на сервер

Сервер принимает несколько событий в одном POST запросе в формате JSON. Пример запроса:

```json
{
    "events": [
        {
            "type": "levelStart",
            "data": "level:3"
        }
        // ...
    ]
}
```

URL для отправки задаётся внешним параметром сервиса `serverUrl`.

## Интерфейс сервиса

Сервис принимает события через метод `TrackEvent` с аргументами `string type` и `string data`.

Сам сервис достаточно оформить в класс наследник MonoBehaviour, например:
```csharp
public class EventService : MonoBehaviour
{
    public void TrackEvent(string type, string data)
    {
    }
}
```

## Кулдаун и гарантированная доставка

События приходят очень неравномерно. За одну секунду может прилететь 10 событий, но потом минуту не будет ни одного события. Чтобы сократить количество запросов к серверу, вводим понятие кулдауна `cooldownBeforeSend` (обычно это 1-3 секунды). Кулдаун работает так:

1. Первое событие запускает кулдаун.
2. Пока длится кулдаун, мы копим поступающие события.
3. По истечении кулдауна делаем отправку накопившихся событий (даже если там одно событие). После этого кулдаун сбрасывается и мы возвращаемся на шаг 1.

Также важно обеспечить гарантированную доставку событий до сервера. События считаются доставленными, только если в ответ на сообщение сервер вернул `200 OK`.

Сервер аналитики не всегда может быть доступен (например, отсутствие сети на телефоне), поэтому успешная отправка может произойти через неопределённое время. Если приложение завершилось (или крашилось по ошибке), то недоставленные события должны быть отправлены при следующем запуске приложения (считаем, что сервис стартует вместе с приложением). Таким образом, события не должны теряться.

## Дополнительно

- Описывать обёртки вокруг сервиса, бутстрапы, UI - не требуется и только усложняет ревью тестового задания. Нам важен только сервис и логика его работы.
- Для проекта используем Unity 2021 LTS.
- Можно использовать дополнительные библиотеки.
