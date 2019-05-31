import './dropdown.scss'
import * as React from 'react'
import * as classnames from 'classnames'
import * as find from "lodash/find"
import * as escapeRegExp from "lodash/escapeRegExp"
import { InputDisabled } from '../input'

interface DropDownProps {
    dataSource: SelectItem[]
    title?: string
   // text: any
    value: any
    name: string
    isClear?: boolean
    handleChange?: (newValue: SelectItem, field) => void,
    disabled?: boolean
    hide?: boolean

    inputRef?: any

    isFullname?: boolean
    readonly?: boolean
    isError?: boolean
}

interface DropDownState {
    isOpen: boolean
    dataSource: SelectItem[] //| SelectItemDependent[]
    value?: any
    text: string
    activeIndex?: number
}

export default class AutocompliteDataSource extends React.Component<DropDownProps, DropDownState> {

    static defaultProps = { isClear: true }

    id: any

    constructor(props: DropDownProps) {
        super(props)

        this.state = {
            isOpen: false,
            dataSource: this.props.dataSource,
            value: props.value,
            text: '',
            activeIndex: null
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

    changeItem = (item: SelectItem) => {

        var source = this.getDataSource(item.text)

        this.setState({
            value: item.value,
            isOpen: false,
            dataSource: source,
            activeIndex: null
        })

        this.props.handleChange(item, this.props.name);
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

        return find(this.props.dataSource, x => x.value === this.state.value)
    }

    get selectedText(): string {
        var selectedItem = this.selectedItem;

        if (selectedItem)
            return selectedItem.text;
        //else if (!this.state.text)
        //    return '';

        return this.state.text;
    }

    onClear = () => {

        this.setState({
            value: '',
            text: '',
            activeIndex: null,
            dataSource: this.props.dataSource
        });

        var emptyItem: SelectItem = { value: '', text: '' }

        this.props.handleChange(emptyItem, this.props.name);
    }

    handleChange = (newValue) => {
        var source = this.getDataSource(newValue)

        var item = find(source, x => x.text == newValue)

        this.setState({
            value: item ? item.value : '',
            dataSource: source,
            text: newValue,
            activeIndex: null
        })

        if (item)
            this.props.handleChange(item, this.props.name);
    }

    getDataSource = (text) => {

        var textMatches = text.replace(/^─*\s/, '')

        var matcher = new RegExp(escapeRegExp(textMatches), "i");

        var source: SelectItem[] = this.props.dataSource

        return source.filter(x => x.text && matcher.test(x.text.replace(/^─*\s/, '')))
    }

    onFocusOut = (e) => {

        var target = e.relatedTarget || document.activeElement

        if (this.state.isOpen && !this._node.contains(target)) {
            var dataSource = this.state.value ? this.state.dataSource : this.props.dataSource

            this.setState({
                isOpen: false,
                text: '',
              //  activeIndex: null,
                dataSource: dataSource
            })
        }
    }

    //getValueItem = (item): string => {
    //    return this.props.isFullname ? item.fullName : item.text;
    //}

    onKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {

        var hasUp = e.keyCode == 38;
        var activeIndex = this.state.activeIndex;

        if (e.keyCode == 38 && activeIndex)
        {
            activeIndex--;
            this.updateListSelection();
        }
        else if (e.keyCode == 40) {
            var sourceCount = this.state.dataSource.length;
            activeIndex = activeIndex == null ? 0 : activeIndex + 1;

            if (activeIndex >= sourceCount)
                return;
            else
                this.updateListSelection();
        }
        else if (e.keyCode == 13 && this.state.activeIndex != null) {

            var item = this.state.dataSource[this.state.activeIndex];

            this.changeItem(item);

            e.preventDefault();

            return;
        }

        this.setState({
            activeIndex: activeIndex
        });
    }

    updateListSelection = () => {

    };

    render() {

        if (this.props.hide)
            return null;

        if (this.props.readonly)
            return <InputDisabled title={this.props.title} value={this.selectedText} />

        var disabled = this.props.disabled || !this.props.dataSource || !this.props.dataSource.length;

        const className = classnames('main-drop-down', {
            'open': this.state.isOpen,
            'disabled': disabled
        })

       

        return (
            <div ref={(node) => { this._node = node; }} className={className}>
                {disabled? <div className="drop-down-overlay" /> : null}
                <Input
                    onKeyDown={this.onKeyPress}
                    id={this.id}
                    title={this.props.title}
                    value={this.selectedText}
                    onFocus={(e) => {this.open(true)}}
                    onClear={this.onClear}
                    onChange={this.handleChange}
                    onBlurInput={this.onFocusOut}
                    disabled={disabled}
                    isClear={this.props.isClear}
                    inputRef={this.props.inputRef}
                    isError={this.props.isError}
                >
                    <span className="drop-down-arrow" onClick={this.onOpen} />
                    {this.state.isOpen && this.state.dataSource && this.state.dataSource.length ?
                        <div className="drop-down">
                            <ul id="ul-drop-down">
                                {this.state.dataSource.map((item, index) => <li className={this.state.activeIndex == index ? "active" : null} key={item.value} onClick={() => this.changeItem(item)}>{item.text}</li>)}
                            </ul>
                        </div>
                        : null
                    }
                </Input>
            </div>
        )
    }
}

const Input = ({ title, value, onKeyDown, onChange, onClear, onFocus, onBlurInput, id,disabled, children, isClear, inputRef,isError}) => {

    var fieldClass = classnames('field-title', {
        'error': isError
    })

    return (
        <div className="main-field" tabIndex={1} onBlur={onBlurInput}>
            <div className={classnames('field-label', { 'label-clear': isClear })}>
                {title ? <label className={fieldClass} htmlFor={id}>{title}</label> : null}
                <div className="field-input">
                    <input
                        autoComplete="off"
                        onKeyDown={onKeyDown}
                        id={id}
                        type="text"
                        value={value}
                        onChange={(e) => onChange(e.target.value)}
                        disabled={disabled}
                        onFocus={onFocus}
                        ref={inputRef}
                    />
                    {isClear && <span className="field-clear" onClick={onClear} />}
                    {children}
                </div>
            </div>
        </div>
    )
}