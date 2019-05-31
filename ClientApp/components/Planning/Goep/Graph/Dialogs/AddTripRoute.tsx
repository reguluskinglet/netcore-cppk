import * as React from 'react'
import { provide } from 'redux-typed';
import TimeField from 'react-simple-timefield';
import { ApplicationState } from '../../../../../store';
import { TimelineTypeEnum, RoutePaths } from '../../../../../common'
import * as DialogStore from '../../../../../store/DialogStore';
import * as GridStore from '../../../../../store/GridStore';
import { Button } from '../../../../../UI/button'
import { DayOfWeek } from '../../../../Common'
import * as remove from 'lodash/remove'
import moment from 'moment'
import Checkbox from '../../../../../UI/checkbox/Checkbox'
import * as store from '../store';
import * as classnames from 'classnames';
import { NavLink } from 'react-router-dom'

interface Data {
    checkListType: TimelineTypeEnum
    routeId: number
    hover: number
}

interface State {
    selectedTrips: TripsByTurnoverIdAndDays[]
}

class AddTripRoute extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)

        this.state = {
            selectedTrips:[]
        }
    }

    handleChange = (newValue, field) => {

        var state = {}
        state[field] = newValue;

        this.setState(state);
    }

    save = () => {

        this.props.addOrUpdateTripToRoute({
            routeId: this.props.response.routeId,
            tripIds: this.state.selectedTrips.map(x => x.id)
        }, this.props.action)
    }

    handleCheck = (event, checked, row) =>
    {
        var selectedRows = [...this.state.selectedTrips];
        var isSelected = selectedRows.indexOf(row);

        if (isSelected >= 0)
            remove(selectedRows, x => x.id == row.id);
        else
            selectedRows.push(row);

        this.setState({ selectedTrips: selectedRows })
    }

    //                    <Button label="Добавить рейс" onClick={() => this.props.toggleDialog(false)} style={{ marginBottom:11}} />
    render() {

        var data = this.props.response.data;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 980 }}>
                    <div className="g-dialog-form">
                        {data.length ?
                            <table className="grid">
                                <tr>{columns.map((name, i) => <th key={i}>{name}</th>)}</tr>
                                <tbody>
                                    {data.map((item, i) => <Row key={i} row={item} handleCheck={this.handleCheck} selectedTrips={this.state.selectedTrips} />)}
                                </tbody>
                            </table>
                            : <div>
                                <h6 style={{ fontSize: 15 }}>Все рейсы уже были добавлены на маршрут или отсутствуют рейсы на дни недели</h6>
                                <a onClick={() => {
                                    this.props.toggleDialog(false)
                                    this.props.goTo(RoutePaths.routes)
                                }}>Перейти к созданию рейса</a>
                            </div>
                            }
                    </div>
                </div>
                <nav className="dialog-actions">
                    <Button label="Сохранить" onClick={this.save} disabled={!this.state.selectedTrips.length} />
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ response: (state.dialog.responseMessage as { data: TripsByTurnoverIdAndDays[], routeId: number }), action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators, goTo: GridStore.actionCreators.goToDocument }
);

type Props = typeof provider.allProps;

export default provider.connect(AddTripRoute);



interface RowProps {
    row: TripsByTurnoverIdAndDays
    selectedTrips: TripsByTurnoverIdAndDays[]
    handleCheck: (event, checked, row) => void
}

const convertTime = (time: string) => {
    var times = time.split(':'),
        day = time


    var date = getDate(time);


    return {
        hour: times[0],
        minute: times[1]
    }
}

const getDate = (time: string, day = '01') =>{
    return moment(`0001-01-${day}T${time}:00+00:00`);
}

const Row = ({ row, selectedTrips, handleCheck}: RowProps) => {

    var checked = selectedTrips.indexOf(row) >= 0;

    var className = classnames({
        'checked': checked
    })
   
    return (
        <tr className={className}>
            <td className="fix row-check"><Checkbox checked={checked} onChange={(checked, event) => handleCheck(event, checked, row)} /></td>
            {keys.map((key, i) => <td key={i} className={key === 'days' ? 'days' : null}>{key === 'days' ? <DayOfWeek selectedDays={row[key]} /> : row[key]}</td>)}
        </tr>
    )
}

const columns = ['', 'Наименование', 'Станция отправления', 'Время', 'Станция прибытия', 'Время', 'Дни недели']
const keys = ['name', 'startStationName', 'startTime', 'endStationName', 'endTime','days'];
