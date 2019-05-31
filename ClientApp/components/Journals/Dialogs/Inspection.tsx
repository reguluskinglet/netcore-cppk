import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../store';
//import * as InvItemState from '../../store/InvItem';

import * as DialogStore from '../../../store/DialogStore';
import * as GridStore from '../../../store/GridStore';
import { DialogForm } from '../../../UI/dialog'
import { Button } from '../../../UI/button'
import { ReadOnlyField } from '../../Common'
import * as remove from 'lodash/remove'
import { InputNumber } from '../../../UI/input'
import Item from 'antd/lib/list/Item';
import classnames from 'classnames'
import { ScrollBarsVertical } from '../../../UI/scrollbars'

interface State {
    tab: Tabs
}

enum Tabs {
    label = 1,
    mileage = 2,
}

class Inspection extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        var labelsCount = props.data.labels.length;

        this.state = {
            tab: labelsCount > 0 ? Tabs.label : Tabs.mileage
        }
    }

    tab = (text: string, icon: string, tab: Tabs) => {

        var className = classnames('g-tab-menu-item', {
            'activ': tab === this.state.tab
        })

        return <div className={className} onClick={() => this.setState({ tab: tab })}>
            <div className="g-text">{text}</div>
        </div>
    }

    renderLabel = () => {
        var data: InspectionUI = this.props.data;

        return <div className="dialog-blok">
            <div className="flex-grid">
                <div className="flex-grid-header">
                    <div className="item">Вагон</div>
                    <div className="item">Оборудование</div>
                    <div className="item">Время</div>
                    <div className="item">ИД Метки</div>
                </div>
                <ScrollBarsVertical height={250} width={undefined}>
                    <div className="flex-container">
                        {data.labels.map(item => <Td item={item} />)}
                        </div>
                </ScrollBarsVertical>
            </div>
        </div>
    }

    renderInspectionData = () => {
        var data: InspectionUI = this.props.data;

        return <div className="dialog-blok">
                <div className="flex-grid">
                    <div className="flex-grid-header">
                        <div className="item">Вагон</div>
                        <div className="item">Квт часы</div>
                    </div>
                    <ScrollBarsVertical height={250} width={undefined}>
                    <div className="flex-container">
                        {data.inspectionDataCarriages.map(item => <Td item={item} />)}
                        </div>
                    </ScrollBarsVertical>
                </div>
        </div> 
    }

    render() {

        var data: InspectionUI = this.props.data,
            labelsCount = data.labels.length,
            inspectionDataCount = data.inspectionDataCarriages.length;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 700}}>
                    <div>
                        <div className="g-data">
                            <ReadOnlyField title="Статус" value={data.status} />
                            <ReadOnlyField title="Состав" value={data.trainName} />
                            <ReadOnlyField title="Исполнитель" value={data.author} />
                            <ReadOnlyField title="Бригада" value={data.brigadeName} />
                            <ReadOnlyField title="Дата" value={data.date} />
                            <ReadOnlyField title="Задач" value={data.taskCount} />
                            {data.inspectionDataUis.map((x, i) => <ReadOnlyField title={x.type} value={x.value} />)}
                            {data.signature && <div className="g-field" style={{ borderBottom:'none' }}>
                                <span className="g-title">Подпись</span>
                                <span className="g-value">
                                    <img height={50} src={data.signature} />
                                </span>
                            </div>}
                        </div>
                        {labelsCount>0 || inspectionDataCount>0 ? <div>
                            <div className="g-tab-menu g-tab-body">
                                {labelsCount > 0 && this.tab(`Считано меток ${labelsCount}`, 'icon-home', Tabs.label)}
                                {inspectionDataCount > 0 && this.tab('Информация по пробегу', 'icon-steam', Tabs.mileage)}
                            </div>
                            {this.state.tab === Tabs.label ? this.renderLabel() : this.renderInspectionData()}
                        </div>
                            : null}
                    </div>
                </div>
                <nav className="dialog-actions">
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ data: state.dialog.responseMessage }),
    { ...DialogStore.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(Inspection);

//Перенести в UI


interface RowProps {
    row: any
}

const Row = ({ row }: RowProps) => {

    var keys = Object.keys(row);

    return (
        <tr>
            {keys.map((key, i) => <td key={i}>{row[key]}</td>)}
        </tr>
        )
}

const labelColumns = ['Вагон', 'Оборудование', 'Время', 'ИД Метки']
const carriageColumns = ['Вагон', 'Квт часы']



const Th = ({ name }) => {
    return <th><span>{name}</span></th>
}

const Td = ({ item }: { item: Object }) => {

    var className = classnames('flex-row')

    var keys = Object.keys(item);

    return <div className={className}>
        {keys.map((x, i) => <div key={i} className="item">{item[x]}</div>)}
    </div>
}