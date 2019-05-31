import './theme.scss'
import * as React from 'react'
import * as classnames from 'classnames';
import checkFactory from './Check';
import rippleFactory from '../ripple/Ripple';

const defaults = {
    checked: false,
    className: '',
    disabled: false,
    theme: {
        field: 'field',
        disabled: 'disabled',
        text: 'text',
        ripple:'ripple'
    }
}


const factory = (Check) => {
    class Checkbox extends React.Component<any, any> {

        static defaultProps = {
            ...defaults
        };

        handleToggle = (event) => {
            if (event.pageX !== 0 && event.pageY !== 0) this.blur();
            if (!this.props.disabled && this.props.onChange) {
                this.props.onChange(!this.props.checked, event);
            }
        };

        inputNode: any;

        blur() {
            if (this.inputNode) {
                this.inputNode.blur();
            }
        }

        focus() {
            if (this.inputNode) {
                this.inputNode.focus();
            }
        }

        render() {
            const {
                checked,
                children,
                disabled,
                label,
                name,
                style,
                onChange,
                onMouseEnter,
                onMouseLeave,
                theme,
                ...others
            } = this.props;

            const className = classnames(theme.field, {
                [theme.disabled]: this.props.disabled,
            }, this.props.className);

            return (
                <label
                    data-react-toolbox="checkbox"
                    htmlFor={name}
                    className={className}
                    onMouseEnter={onMouseEnter}
                    onMouseLeave={onMouseLeave}
                >
                    <input
                        {...others}
                        checked={checked}
                        className={theme.input}
                        disabled={disabled}
                        name={name}
                        onChange={() => { }}
                        onClick={this.handleToggle}
                        ref={(node) => { this.inputNode = node; }}
                        type="checkbox"
                    />
                    <Check
                        checked={checked}
                        disabled={disabled}
                        rippleClassName={theme.ripple}
                        style={style}
                        theme={theme}
                    />
                    {label ? <span data-react-toolbox="label" className={theme.text}>{label}</span> : null}
                    {children}
                </label>
            );
        }
    }

    return Checkbox;
};

const Check = checkFactory(rippleFactory({ centered: true, spread: 2.6 }));
const Checkbox = factory(Check);

export default Checkbox;
