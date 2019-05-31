import * as React from 'react'
import { DocumentType } from '../../../common'
import classnames from 'classnames'
import { dataSourceSelect } from '../../../resource'
import moment from 'moment'
import api from '../../../services/rest'
import * as remove from 'lodash/remove'

const folders = ['project/docs/', 'project/images/', 'project/sounds/']
const brigadeTypes = dataSourceSelect.brigadeTypeToObject();
const status = dataSourceSelect.statusToObject();
const inspections = dataSourceSelect.typeToObject();

export const FileBlock = ({ file, onDelete }: { file: JournalTaskHistoryFile, onDelete?: (id) => void}) => {

    var path = folders[file.documentType];

    var iconClass = classnames('g-icon', {
        'icon-audio': file.documentType == DocumentType.Sound,
        'icon-doc': file.documentType == DocumentType.Other
    })

    const open = (e) => {

        if (!$(e.target).hasClass('icon-close'))
            window.open(path + file.name, '_blank')
    }

    return <div className="g-file" onClick={open}>
        {file.documentType == DocumentType.Image
            ? <img src={path + 'thumbs/' + file.name} />
            : <div className="main-g-icon">
                <span className={iconClass} />
                <span className="g-name">{file.name}</span>
            </div>}
        {onDelete && <span className="icon-close" onClick={() => onDelete(file.id)} />}
    </div>
}

export const HistoryBlock = ({ history }: { history: JournalTaskHistory}) => {

    var text,
        type;

    const getText = (sources: Object, oldValue: number, newValue: number) => {

        var oldValueText,
            newValueText = sources[newValue];

        if (oldValue)
            oldValueText = sources[oldValue];

        return oldValueText ? `${oldValueText} → ${newValueText}` : newValueText
    }

    switch (history.type) {
        case 'Executor':
            type = 'Смена исполнителя'
            text = getText(brigadeTypes, history.oldExecutorBrigadeType, history.newExecutorBrigadeType);
            break;
        case 'Status':
            type = 'Смена статуса'
            text = getText(status, history.oldStatus, history.newStatus);
            break;
        case 'Comment':
            type = 'Сообщение'
            text = history.text;
            break;
    }

    return <div className="task-history-block">
        <div className="main-g-user">
            <span className="g-date">{moment(history.date, 'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY HH:mm')}</span> | <span className="g-user">{history.user}</span>
        </div>
        <div className="main-g-type">
            <div><b>{type}:</b><span>{text}</span></div>
            {history.files && history.files.length ? <div className="g-main-file history">{history.files.map((x, i) => <FileBlock key={i} file={x} />)}</div> : null}
        </div>
    </div>
}

export const FaultBlock = ({ fault }: { fault: JournalTaskFault}) => {

    return <div className="task-history-block">
        <div className="main-g-user">
            <span className="g-date">{moment(fault.date, 'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY HH:mm')}</span> | <span className="g-user">{fault.user}</span>
        </div>
        <div className="main-g-type">
            <div>{fault.text}</div>
        </div>
    </div>
}

export const InspectionBlock = ({ inspection }: { inspection: JournalTaskInspection}) => {

    var dateStart = moment(inspection.dateStart, 'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY HH:mm');
    var date = inspection.dateEnd ? `${moment(inspection.dateEnd, 'YYYY-MM-DDTHH:mm:ss').format('DD.MM.YYYY HH:mm')} - ${dateStart}` : dateStart,
        type = inspections[inspection.type],
        brigade = brigadeTypes[inspection.brigadeType];


    return <div className="task-history-block">
        <div className="main-g-user">
            <b className="g-ins-type">{type}</b>
            <p className="g-user">{date}</p>
            <p><b className="g-date">{brigade}</b> | <span className="g-user">{inspection.user}</span></p>
        </div>
        <div className="main-g-type">
            <div>{inspection.texts.map((x, i) => <p key={i}>{x}</p>)}</div>
        </div>
    </div>
}

export const UploadFile = ({ files, onChange }: { files: JournalTaskFile[], onChange: (files: JournalTaskFile[]) => void }) => {

    const onDeleteFile = (id) => {

        api.delete(`Document/Delete`, { id }).then(result => {
            remove(files, x => x.id === id);

            onChange(files);
        })
    }

    const handleFileChange = () => {
        const input = document.createElement('input')
        input.setAttribute('type', 'file')
        input.setAttribute('multiple', 'true')
        input.click()
        input.onchange = (e: any) => {
            var inputFiles: File[] = e.target.files,
                formData = new FormData(),
                currentFiles = files,
                count = currentFiles.length,
                totalCount = 6 - count;

            for (var i = 0; i < inputFiles.length; i++) {

                if (i > totalCount)
                    break;

                formData.append(`files`, inputFiles[i]);
            }

            api.post('Document/Add', formData, true).then(result => {

                var newFiles = JSON.parse(result).files;

                currentFiles.push(...newFiles);

                onChange(currentFiles);
            })
        }
    };

    return <div className="g-main-file">
        {files.map((x, i) => <FileBlock key={i} file={x} onDelete={onDeleteFile} />)}
        <span className="icon-attach" onClick={handleFileChange} />
    </div>
}