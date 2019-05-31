import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../../../../store';
import * as DialogStore from '../../../../../store/DialogStore';
import * as GridStore from '../../../../../store/GridStore';
import { DialogForm } from '../../../../../UI/dialog'
import { Button } from '../../../../../UI/button'
import * as remove from 'lodash/remove'
import { InputNumber, Input } from '../../../../../UI/input'
import * as store from '../store';

interface Data
{
    mileage?: number,
    name?: string,
    routeId?: number
}

class AddRoute extends React.Component<Props, any> {

    data: Data = {}

    constructor(props) {
        super(props)

        if (this.props.data)
            this.data = this.props.data;
    }

    handleChange = (newValue, field) => {

        this.data[field] = newValue;
    }

    render() {
        var data = this.data;

        return (
            <div>
                <div className="dialog-body clear-x" style={{ width: 400 }}>
                    <div className="g-dialog-form">
                        <Input title="Номер" name="name" value={data.name} handleChange={this.handleChange} />
                        <Input title="Пробег" type="number" name="mileage" value={data.mileage} handleChange={this.handleChange} />
                    </div>
                </div>
                <nav className="dialog-actions">
                    <Button label="Сохранить" onClick={() => this.props.addRoute(data, this.props.action)} />
                    <Button label="Закрыть" onClick={() => this.props.toggleDialog(false)} />
                </nav>
            </div>
        )
    }
}

const provider = provide(
    (state: ApplicationState) => ({ data: (state.dialog.responseMessage as Data), action:state.dialog.action}),
    { ...DialogStore.actionCreators, ...store.actionCreators }
);

type Props = typeof provider.allProps;

export default provider.connect(AddRoute);
