Globalize.addCultureInfo("ru-RU", "default", {
    name: "ru-RU",
    englishName: "Russian (Russia)",
    nativeName: "русский (Россия)",
    language: "ru",
    numberFormat: {
        ",": " ",
        ".": ",",
        negativeInfinity: "-бесконечность",
        positiveInfinity: "бесконечность",
        percent: {
            pattern: ["-n%", "n%"],
            ",": " ",
            ".": ","
        },
        currency: {
            pattern: ["-n$", "n$"],
            ",": " ",
            ".": ",",
            symbol: "р."
        }
    },
    calendars: {
        standard: {
            "/": "-",
            firstDay: 1,
            days: {
                names: ["воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота"],
                namesAbbr: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
                namesShort: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"]
            },
            months: {
                names: ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь",""],
                namesAbbr: ["Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек", ""]
            },
            monthsGenitive: {
                names: ["января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря",""],
                namesAbbr: ["янв", "фев", "мар", "апр", "май", "июн", "июл", "авг", "сен", "окт", "ноя", "дек",""]
            },
            AM: null,
            PM: null,
            patterns: {
                d: "dd.MM.yyyy",
                D: "d MMMM yyyy 'г.'",
                t: "H:mm",
                T: "H:mm:ss",
                f: "d MMMM yyyy 'г.' H:mm",
                F: "d MMMM yyyy 'г.' H:mm:ss",
                Y: "MMMM yyyy"
            }
        }
    }
});

Globalize.culture("ru-RU");

