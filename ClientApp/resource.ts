const convertToObject=(items: SelectItem[])=>{
    var result = {};

    for (var i = 0; i < items.length; i++) {
        var item = items[i];

        result[item.value] = item.text;
    }

    return result;
}

export const dataSourceSelect = {
    type: [
        { value: 99, text: 'Инцидент' },
        { value: 0, text: 'ТО-1' },
        { value: 1, text: 'ТО-2' },
        { value: 2, text: 'Приемка поезда' },
        { value: 3, text: 'Сдача поезда' },
    ],
    typeToObject: (): Object => {
        return convertToObject(dataSourceSelect.type)
    },
    brigadeType: [
        { value: 0, text: 'Локомотивная бригада' },
        { value: 1, text: 'Бригада Депо' },
        { value: 2, text: 'Приемщики' }
    ],
    brigadeTypeToObject: (): Object => {
        return convertToObject(dataSourceSelect.brigadeType)
    },
    status: [
        { value: 0, text: 'Новая' },
        { value: 1, text: 'В журнале' },
        { value: 2, text: 'В работе' },
        { value: 3, text: 'Просмотрено' },
        { value: 4, text: 'Принято к исполнению' },
        { value: 5, text: 'Не подтверждено' },
        { value: 6, text: 'Выполнено' },
        { value: 7, text: 'К проверке' },
        { value: 8, text: 'Не прошла проверку' },
        { value: 9, text: 'К подтверждению' },
        { value: 99, text: 'Закрыто' },
    ],
    statusToObject: (): Object => {
        return convertToObject(dataSourceSelect.status)
    },
    inspectionStatus: [
        { value: 0, text: 'Новая' },
        { value: 1, text: 'Отмененно' },
        { value: 2, text: 'Завершенно' },
    ],
    hasInspection: [
        { value: true, text: 'Мероприятия' },
        { value: false, text: 'Инцидент' },
    ],

    taskLevel:[
        { text: 'Низкий', value: 1 },
        { text: 'Нормальный', value: 2 },
        { text: 'Высокий', value: 3 }
    ],
    taskType: [
        { text: 'Техническая', value: 0 },
        { text: 'Санитарная', value: 1 }
    ]
}