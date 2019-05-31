import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../../store';
import * as DialogStore from '../../../../store/DialogStore';
import { Button } from '../../../../UI/button'
import { Input } from '../../../../UI/input'
import { DayOfWeek } from '../../../Common'
import { AutocompliteDataSource } from '../../../../UI/dropdown'
import { checkToValue } from '../../../../UI/utils/utils'
import * as store from '../store'

interface State {
    id?: number
    days?: number[]
    directionId?: number
    name?: string
}

class TurnoverCreate extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props)


        var result = props.data;

        this.state = {
            id: result.id,
            days: result.days,
            directionId: result.directionId,
            name: result.name
        }
    }


    handleChange = (newValue, field) => {

        var state = {}
        state[field] = newValue;

        this.setState(state);
    }

    selectedDay = (day: number) => {

        var days = [...this.state.days],
            index = days.indexOf(day);

        if (index >= 0)
            days.splice(index, 1)
        else
            days.push(day);

        this.setState({
            days: days
        })
    }

    get isValid() {

        const { days, name, directionId } = this.state

        return days.length > 0 && checkToValue(name) && checkToValue(directionId)
    }

    save = () => {

        this.props.saveTurnoverWithDays(this.state, this.props.action);
    }

    render() {

        var data = this.props.data;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 700 }}>
                    <div className="g-dialog-form">
                        <Input title="Наименование" name="name" value={this.state.name} handleChange={this.handleChange} />
                        <AutocompliteDataSource
                            name="directionId"
                            title="Направление"
                            dataSource={data.directions}
                            value={this.state.directionId}
                            handleChange={(n, f) => this.handleChange(n.value, f)} />
                        <div className="day-of-week-field clear-x g-turnover" style={{ marginBottom: 8 }}>
                            <b>Дни недели</b>
                            <DayOfWeek selectedDays={this.state.days} onSelectedDay={this.selectedDay} />
                        </div>
                    </div>
                </div>
                <nav className="dialog-actions">
                    <Button disabled={!this.isValid} label="Сохранить" onClick={this.save} />
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage as TurnoverUI), action: state.dialog.action }),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(TurnoverCreate);

