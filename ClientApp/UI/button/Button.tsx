import './theme.scss'
import * as React from 'react'
import * as classnames from 'classnames';
import rippleFactory from '../ripple/Ripple';

interface ButtonProps {

    href?: string;

    label?: string;

    children?: React.ReactNode;

    disabled?: boolean;

    onMouseLeave?: Function;

    onMouseUp?: Function;

    ripple?: boolean;

    type?: string;

    className?: string

    red?: boolean
    green?: boolean
    grey?: boolean
}

const factory = (ripple) => {
    class Button extends React.Component<ButtonProps, undefined> {

        static defaultProps = {
            className: '',
            type: 'button'
        };

        buttonNode: any;

        handleMouseUp = (event) => {
            this.buttonNode.blur();

            if (this.props.onMouseUp)
                this.props.onMouseUp(event);
        };

        handleMouseLeave = (event) => {
            this.buttonNode.blur();

            if (this.props.onMouseLeave)
                this.props.onMouseLeave(event);
        };

        render() {
            const {
                children,
                className,
                href,
                label,
                type,
                red,
                green,
                grey,
                ...others
            } = this.props;

            const element: any = href ? 'a' : 'button';
         //   const shape = this.getShape();

            const classes = classnames('button', {
                'red': red,
                'green': green,
                'grey': grey

                //[theme[level]]: neutral,
                //[theme.mini]: mini,
                //[theme.inverse]: inverse,
            }, className);

            const props = {
                ...others,
                href,
                ref: (node) => { this.buttonNode = node; },
                className: classes,
                disabled: this.props.disabled,
                onMouseUp: this.handleMouseUp,
                onMouseLeave: this.handleMouseLeave,
                type: !href ? type : null,
            };

            return React.createElement(element, props, label,children);
        }
    }

    return ripple(Button);
};

const Button = factory(rippleFactory({ centered: false }));

export default Button;



//export default themr(BUTTON)(Button);
//export { factory as buttonFactory };
//export { Button };


