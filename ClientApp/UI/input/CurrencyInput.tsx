import './input.scss'
import * as React from 'react'
import * as classnames from 'classnames';
//import * as mixin from 'lodash/mixin'
//import * as LinkedStateMixin from 'react-addons-linked-state-mixin'

interface Props {
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
}


export default class CurrencyInput extends React.Component<Props, any> {

    static defaultProps = { isClear: true, type: 'text' }

    constructor(props) {
        super(props)

        this.state = {
            value: this.getCurencyValue(props.value)
        }
    }

    componentWillReceiveProps(nextProps) {

        this.setState({
            value: this.getCurencyValue(nextProps.value)
        })
    }

    splice = (value, idx, str)=> {
        return value.slice(0, idx) + str + value.slice(idx);
    };

    handleChange = (e: React.FormEvent<HTMLInputElement>) => {

        var residueIndex = this.state.value.indexOf(','),
            start = e.currentTarget.selectionStart;

        if (this.keyCode == 1 || (this.keyCode == 2 && residueIndex === start)) {
            this.setValueWithPosition(this.state.value, start, 0)
            return;
        }
        else if (this.keyCode === 3 && !/^[0-9,\s.]+$/g.test(e.currentTarget.value)) {
            console.log(e.currentTarget.value)
            return;
        }

        var inputValue = e.currentTarget.value;
        var  residue = inputValue.split(',');

        if (residue.length > 1)
        {
            var rt = residue[1];

            if (rt.length == 1)
                inputValue += '0';
            else if (rt.length > 2)
                inputValue = inputValue.slice(0, -1)
        }

        var inputValueLength = inputValue.length,
            currentValue = Globalize.parseFloat(this.state.value),
            newValue = !inputValue ? 0 : Globalize.parseFloat(inputValue),
            newValue = isNaN(newValue) ? currentValue : newValue,
            newValue = currentValue === 0 && newValue > 9 ? newValue / 10 : newValue;

        var value = this.getCurencyValue(newValue);

        var count = newValue == 0 ? 0 : inputValueLength > 1 && value.length > inputValueLength ? 1 : inputValueLength > 1 && (this.state.value.length - value.length)===2?-1 : 0;

        this.setValueWithPosition(value, start, count)
    }

    setValueWithPosition = (value, start, count, end?) => {
        this.setState({ value: value }, () => this.setCursorPosition(start, count, end));

        this.props.handleChange(Globalize.parseFloat(value), this.props.name)
    }

    onClear = () => {
        this.setValueWithPosition('0,00', 0, 0)

        this.props.handleChange('', this.props.name)
    }

    spaceCode = (keyCode) => {
        return (keyCode === 0 || keyCode === 32)
    }

    numberCode = (keyCode) => {
        return (keyCode >= 48 && keyCode <= 57) || (keyCode >= 96 && keyCode <= 105);
    }

    arrowLeftRightCode = (keyCode) => {
        return (keyCode === 37 || keyCode === 39)
    }

    arrowUpCode = (keyCode) => {
        return keyCode === 38;
    }

    arrowDownCode = (keyCode) => {
        return keyCode === 40;
    }

    deleteCode = (keyCode) => {
        return (keyCode === 8 || keyCode === 46)
    }

    tabCode = (keyCode) => {
        return keyCode === 9
    }

    keyCode: number

    onUpDownValue = (increment) => {
        var start = this._input.selectionStart,
            inputValue = this.state.value,
            value = Globalize.parseFloat(inputValue),
            residueIndex = inputValue.indexOf(',');

        var count;
        var startPosition = 0;
        var newValue;

        if (start <= residueIndex) {
            value += 1 * increment;
            newValue = this.getCurencyValue(value);
            count = newValue.split(',')[0].length;
        }
        else
        {
            startPosition = residueIndex + 1;
            value += 0.01 * increment;
            newValue = this.getCurencyValue(value);
            count = newValue.length;
        }

        this.setValueWithPosition(newValue, startPosition, 0, count);
    }

    copyPasteCode = (ctrlKey, keyCode) => {
        return ctrlKey && keyCode == 86 || ctrlKey && keyCode == 67
    }

    getCurencyValue = (value: number): string => {
        return Globalize.format(value < 0 ? 0 : value, "n2");
    }

    onKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {

        this.keyCode = undefined

        if (this.numberCode(e.keyCode) || this.tabCode(e.keyCode) || this.arrowLeftRightCode(e.keyCode))
            return;

        if (this.arrowUpCode(e.keyCode))
            this.onUpDownValue(1);
        else if (this.arrowDownCode(e.keyCode))
            this.onUpDownValue(-1);
        else if (this.deleteCode(e.keyCode)) {
            this.keyCode = 2;
            return;
        }
        else if (this.spaceCode(e.keyCode)) {
            this.keyCode = 1
            return;
        }
        else if (this.copyPasteCode(e.ctrlKey, e.keyCode)) {
            this.keyCode = 3;
            return;
        }

        e.preventDefault();
    }

    setCursorPosition = (start, position, end?) => {

        if (end)
        {
            this._input.selectionStart = start;             
            this._input.selectionEnd = end;
        }
        else
            this._input.selectionStart = this._input.selectionEnd = start + position
    }


    _input: HTMLInputElement

    render() {

        if (this.props.hide)
            return null;

        const className = classnames('field-label', {
            'label-clear': this.props.isClear
        })

        var style = null;

        if (this.props.width)
            style = { width: this.props.width }

        return <div className="main-field">
            <label className={className}>
                {this.props.title ? <span className="field-title">{this.props.title}</span> : null}
                <div className="field-input" style={style}>
                    <input ref={(node) => { this._input = node; }} type={this.props.type} onKeyDown={this.onKeyPress} onChange={this.handleChange} value={this.state.value} disabled={this.props.disabled} />
                    {this.props.isClear && !this.props.disabled ? <span className="field-clear" onClick={this.onClear} /> : null}
                    {this.props.children}
                </div>
            </label>
        </div>

    }
}
