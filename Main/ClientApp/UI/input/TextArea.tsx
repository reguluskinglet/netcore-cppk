import './input.scss'
import * as React from 'react'
import * as classnames from 'classnames';
//import * as mixin from 'lodash/mixin'
//import * as LinkedStateMixin from 'react-addons-linked-state-mixin'

interface InputProps {
    value: any
    name: string
    title: string
    height: number

    handleChange: (newValue, field) => void

    placeholder?: string
    disabled?: boolean
}


export default class TextArea extends React.Component<InputProps, any> {

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
        if (this.props.handleChange)
            this.props.handleChange(newValue, this.props.name)
    }

    onChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
        var value = e.target.value;

        this.setState({
            value: value
        })

        this.handleChange(value)
    }

    render() {
        return <div className="main-field main-field-area" style={{ height: this.props.height }}>
            <label className="field-label-area">
                <span className="field-title">{this.props.title}</span>
                <div className="field-input-area">
                    <textarea value={this.state.value} onChange={this.onChange} placeholder={this.props.placeholder} disabled={this.props.disabled} />
                </div>
            </label>
        </div>

    }
}