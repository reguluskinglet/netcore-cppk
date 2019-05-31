import * as React from 'react'
import Checkbox from './Checkbox'
import * as classnames from 'classnames'

interface CheckBoxFieldProps {
    name: any
    title: string
    checked: boolean
    handleChange?: (checked, field) => void
    disabled?: boolean,

    invert?: boolean
    className?: string
    fieldTitleWidth?: number
}


export default class CheckBoxField extends React.Component<CheckBoxFieldProps, any> {

    constructor(props) {
        super(props)

        this.state = {
            checked: props.checked
        }
    }

    componentWillReceiveProps(nextProps) {

        this.setState({
            checked: nextProps.checked
        })
    }

    handleChange = (e) => {

        var checked = !this.state.checked;

        this.setState({
            checked: checked
        })

        if (this.props.handleChange)
            this.props.handleChange(checked, this.props.name)
    }

    render() {

        const className = classnames('field-label field', {
            'disabled': this.props.disabled,
            'invert': this.props.invert
        });

        const classNameCheck = classnames('check', {
            'checked': this.state.checked,
        });

        const title = <span className="field-title" style={{ width: this.props.fieldTitleWidth }}>{this.props.title}</span>;
        const checkBox = (
            <div className="field-check-box">
                <input type="checkbox" checked={this.state.checked} disabled={this.props.disabled} onChange={this.handleChange} />
                <div className={classNameCheck}></div>
            </div>)

        const classField = classnames('main-field', {
            [this.props.className]: this.props.className != undefined
        })

        return (
            <div className={classField}>
                {this.props.invert ?
                    <label className={className}>
                        {checkBox}
                        {title}
                    </label>
                    :
                    <label className={className}>
                        {title}
                        {checkBox}
                    </label>
                }
            </div>
          )
    }
}