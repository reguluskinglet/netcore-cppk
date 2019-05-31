export const getViewport = () => ({
    height: window.innerHeight || document.documentElement.offsetHeight,
    width: window.innerWidth || document.documentElement.offsetWidth,
});


export const checkToValue = (value)=> {
    return value !== undefined && value !== null && value !== '';
}

export const isValidData = (data) => {

    var isValidAll = true;

    for (var key in data) {

        var handler = checkToValue;

        var isValid = handler(data[key]);

        if (!isValid)
            isValidAll = false;
    }

    return isValidAll;
}

export const isValidForm = (component, data) => {
    var validate = { ...component.state.validate };

    var isValidAll = true;

    for (var key in validate) {

        var validator = validate[key]

        var handler = validator.handler || checkToValue;

        var isValid = handler(data[key]);

        validator.isError = !isValid;

        if (!isValid)
            isValidAll = false;
    }

    component.setState({
        validate: validate
    })

    return isValidAll;
}

export const changeArrayItem = (array: any[], id) => {

    var index = array.indexOf(id);

    if (index >= 0)
        array.splice(index, 1)
    else
        array.push(id)

    return array;
}
export const swapRow = (row, fromIndex, toIndex) => {
    var temp = row.item[fromIndex];
    row.item[fromIndex] = row.item[toIndex];
    row.item[toIndex] = temp;

    if (row.child) {
        for (var i = 0; i < row.child.length; i++)
            swapRow(row.child[i], fromIndex, toIndex)
    }
}

export const findRow = (row, id, newRow) => {

    if (row.id === id)
        return true;

    if (row.child) {
        for (var i = 0; i < row.child.length; i++) {
            if (findRow(row.child[i], id, newRow)) {
                row.child[i] = newRow;
                return false;
            }
        }
    }

    return null;
}

export const getRowById = (row, id) => {

    if (row.id === id)
        return row;

    if (row.child) {
        for (var i = 0; i < row.child.length; i++) {
            if (getRowById(row.child[i], id)) {
                return row.child[i];
            }
        }
    }

    return null;
}