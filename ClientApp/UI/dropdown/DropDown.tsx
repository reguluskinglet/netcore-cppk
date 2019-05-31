import './dropdown.scss'
import * as React from 'react'
import * as classnames from 'classnames'
//import { Input } from '../input'
import * as find from "lodash/find"
import { InputDisabled } from '../input'

interface DropDownProps {
    dataSource: SelectItem[]
    title?: string
    value: any
    name: string
    isClear?: boolean
    width?: number
    handleChange: (newValue, field) => void
    isError?: boolean
    readonly?: boolean
    emptyText?: string
}

interface DropDownState {
    isOpen: boolean
    value: any
}

export default class DropDown extends React.Component<DropDownProps, DropDownState> {

    static defaultProps = { isClear: true }

    id: any

    constructor(props) {
        super(props)

        this.state = {
            isOpen: false,
            value: props.value
        };

        this.id = Math.random();
    }

    _node: any


    componentWillReceiveProps(nextProps) {

        var newState = {
            value: nextProps.value,

        }

        if (!nextProps.value)
            newState['dataSource'] = nextProps.dataSource;

        this.setState(newState)
    }

    //componentWillMount = () => {
    //    document.addEventListener('click', this.handleClick, false);
    //}

    //componentWillUnmount = () => {
    //    document.removeEventListener('click', this.handleClick, false);
    //}

    //handleClick = (e) => {
    //    if (!this._node.contains(e.target)) {

    //        this.setState({ isOpen: false })
    //    }
    //}

    changeItem = (item: SelectItem) => {

        this.setState({
            value: item.value,
            isOpen: false
        })

        this.props.handleChange(item.value, this.props.name);
    }

    open = (open) => {
        this.setState({
            isOpen: open
        })
    }

    onOpen = (e) => {
        e.preventDefault()

        this.open(!this.state.isOpen)
    }

    get selectedItem(): SelectItem {

        var value = find(this.props.dataSource, x => x.value === this.state.value)

        if (this.props.emptyText)
            return value ? value : { value: '', text: this.props.emptyText };

        return value;
    }

    get selectedText(): string {
        var selectedItem = this.selectedItem;

        if (selectedItem)
            return selectedItem.text;

        return "";
    }

    onClear = () => {
        this.setState({ value: '' });

        this.props.handleChange('', this.props.name);
    }

    getWidth(): any {
        if (this.props.width)
            return { style: { width: this.props.width } }

        return null;
    }

    onFocusOut = (e) => {

        var target = e.relatedTarget || document.activeElement

        if (this.state.isOpen && !this._node.contains(target)) {
            this.setState({ isOpen: false })
        }
    }

    render() {

        if (this.props.readonly)
            return <InputDisabled title={this.props.title} value={this.selectedText} style={this.getWidth()} />

        var disabled = !this.props.dataSource || !this.props.dataSource.length;

        const className = classnames('main-drop-down', {
            'open': this.state.isOpen,
            'error': this.props.isError,
            'disabled': disabled
        })

        return (
            <div ref={(node) => { this._node = node; }} className={className}>
                {disabled ? <div className="drop-down-overlay" /> : null}
                <Input
                    title={this.props.title}
                    value={this.selectedText}
                    onFocus={(e) => this.open(true)}
                    onClear={this.onClear}
                    style={this.getWidth()}
                    name={this.id}
                    onBlurInput={this.onFocusOut}
                    isClear={this.props.isClear}
                >
                    <span className="drop-down-arrow" onClick={this.onOpen} />
                    {this.state.isOpen && this.props.dataSource && this.props.dataSource.length ?
                        <div className="drop-down">
                            <ul>
                                {this.props.emptyText && <li onClick={() => this.changeItem({ value: '', text: this.props.emptyText })}>{this.props.emptyText}</li>}
                                {this.props.dataSource.map((item,i) => <li key={'dd'+i} onClick={() => this.changeItem(item)}>{item.text}</li>)}
                            </ul>
                        </div>
                        : null
                    }
                </Input>
            </div>
        )
    }
}

const Input = ({title, value, name, onClear, onFocus, onBlurInput, children, isClear, style}) => {

    return (
        <div className="main-field" tabIndex={1} onBlur={onBlurInput}>
            <div className={classnames('field-label', { 'label-clear': isClear })}>
                {title && <label className="field-title" htmlFor={name}>{title}</label>} 
                <div className="field-input" {...style}>
                    <input
                        id={name}
                        className="select-none"
                        type="text"
                        value={value}
                        readOnly
                        onFocus={onFocus} />
                    {isClear && <span className="field-clear" onClick={onClear} />}
                    {children}
                </div>
            </div>
        </div>
    )
}