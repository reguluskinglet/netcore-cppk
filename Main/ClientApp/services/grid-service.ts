import api from './rest'
import { CommandType } from '../common'


export const getGrid = (gridName: string, data?, command?: CommandType): JQueryPromise<any> => {
    return createGridCommand(gridName, data, command)
}

export const getChild = (gridName: string, data?): JQueryPromise<any> => {
    let url = 'grid/' + gridName +'GetChild';

    return api.request('POST', url, data, false);
}

const createGridCommand = (urlgrid, data?, command?: CommandType): JQueryPromise<any> => {

    let url = 'grid/' + urlgrid;

    if (command != undefined || data != undefined) {

        var query = url.indexOf('?') > 0 ? '&' : '?';

        if (command != undefined)
            url += query + 'command=' + command

        return api.request('POST', url, data, true, '/api/', false, false, true);
    }

    return api.request('GET', url, null, true);
}
