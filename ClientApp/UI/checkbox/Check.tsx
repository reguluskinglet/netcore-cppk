import * as React from 'react'
import * as classnames from 'classnames';

interface CheckProps
{
    checked?: boolean
    children?: React.ReactNode
    onMouseDown?: (e) => void
    style?: any
    theme?: CheckPropsTheme
}

interface CheckPropsTheme
{
    check?: string
    checked?: string
}

const defaults = {
    checked: 'checked',
    check: 'check',
}

const factory = (ripple) => {
    const Check = ({ checked, children, onMouseDown, style }) => (
        <div
            data-react-toolbox="check"
            className={classnames(defaults.check, { [defaults.checked]: checked })}
            onMouseDown={onMouseDown}
            style={style}
        >
            {children}
        </div>
    );

    return ripple(Check);
};

export default factory;
