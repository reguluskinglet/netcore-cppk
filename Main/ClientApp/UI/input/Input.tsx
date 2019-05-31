import './input.scss'
import * as React from 'react'
import * as classnames from 'classnames';
import { EventHandler, ChangeEventHandler } from 'react';
//import * as mixin from 'lodash/mixin'
//import * as LinkedStateMixin from 'react-addons-linked-state-mixin'
import TimeField from 'react-simple-timefield';

interface InputProps {
    value: any
    name: string
    title?: string
    isClear?: boolean
    handleChange?: (newValue, field) => void
    onClear?: () => void

    width?: number
    disabled?: boolean

    type?: string
    hide?: boolean
    isError?: boolean

    className?: string
    time?: boolean
}


export default class Input extends React.Component<InputProps, any> {

    linkState: any

    static defaultProps = { isClear: true, type: 'text' }

    constructor(props) {
        super(props)

        this.state = {
            value: props.value,
        }
    }

    componentWillReceiveProps(nextProps) {

        this.setState({
            value: nextProps.value
        })
    }

    handleChange = (newValue) => {
       // this.setState({ value: newValue });
        // console.log(newValue)
        this.props.handleChange(newValue, this.props.name)
    }

    onClear = () => {
        this.setState({ value: '' });

        this.props.handleChange('', this.props.name)
    }

    onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        var value = e.target.value;

        this.setState({
            value: value
        })

        this.handleChange(value)
    }

    onChangeTime = (time) => {

        this.setState({
            value: time
        })

        this.handleChange(time)
    }

    render() {

        if (this.props.hide)
            return null;

        const className = classnames('field-label', {
            'label-clear': this.props.isClear,
            'g-time': this.props.time,
            [this.props.className]: this.props.className != undefined
        })

        //var valueLink = {
        //    value: this.state.value,
        //    requestChange: this.handleChange
        //};

        var style = null;

        if (this.props.width)
            style = { width: this.props.width }

        var fieldClass = classnames('field-title', {
            'error': this.props.isError
        })

        return <div className={classnames('main-field', { 'main-field-time': this.props.time})}>
            <label className={className}>
                {this.props.title ? <span className={fieldClass}>{this.props.title}</span> : null}
                <div className="field-input" style={style}>
                    {this.props.time
                        ? <TimeField disabled={this.props.disabled} value={this.state.value} onChange={this.onChangeTime} />
                        : <input type={this.props.type} onChange={this.onChange} value={this.state.value} disabled={this.props.disabled} />}
                    {this.props.isClear && !this.props.disabled ? <span className="field-clear" onClick={this.onClear} /> : null}
                    {this.props.time && <span className="icon-time" />}
                    {this.props.children}
                </div>
            </label>
        </div>

    }
}