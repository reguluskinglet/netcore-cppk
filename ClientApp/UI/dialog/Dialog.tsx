import './theme.scss'
import * as React from 'react'
import * as classnames from 'classnames';
import Portal from '../hoc/Portal';
import ActivableRenderer from '../hoc/ActivableRenderer';
import InjectButton from '../button/Button';
import InjectOverlay from '../overlay/Overlay';

interface DialogProps
{
    actions?: any[]
    active?: boolean
    children?: React.ReactNode
    className?: string
    onEscKeyDown?: (e) => void
    onOverlayClick?: (e) => void
    onOverlayMouseDown?: (e) => void
    onOverlayMouseMove?: (e) => void
    onOverlayMouseUp?: (e) => void
    title?: string
    type?: string
    theme: DialogPropsTheme
}

interface DialogPropsActions {
    className?: string
    label?: string
    children?: React.ReactNode
}

interface DialogPropsTheme
{
    active?: string
    body?: string
    button?: string
    dialog?: string
    navigation?: string
    overlay?: string
    title?: string
    wrapper?: string


}

const defaults = {
    actions: [],
    active: false,
    type: 'normal',
    theme: {
        active: 'active',
        body: 'body',
        button: 'button',
        dialog: 'dialog',
        navigation: 'navigation',
        overlay: 'backdrop',
        title: 'title',
        wrapper:'overlay'
    }
}

const factory = (Overlay, Button) => {
    class Dialog extends React.Component<DialogProps, any> {

        static defaultProps = {
            ...defaults
        };

        render() {

            const { ...props } = this.props;

            const actions = props.actions.map((action, idx) => {
                const className = classnames(props.theme.button, { [action.className]: action.className });
                return <Button key={idx} {...action} className={className} />; // eslint-disable-line
            });

            const className = classnames([props.theme.dialog, props.theme[props.type]], {
                [props.theme.active]: props.active,
            }, props.className);

            return (
                <Portal className={props.theme.wrapper}>
                    <Overlay
                        active={props.active}
                        onClick={props.onOverlayClick}
                        onEscKeyDown={props.onEscKeyDown}
                        onMouseDown={props.onOverlayMouseDown}
                        onMouseMove={props.onOverlayMouseMove}
                        onMouseUp={props.onOverlayMouseUp}
                        theme={props.theme}
                        lockScroll={false}
                    />
                    <div data-react-toolbox="dialog" className={className}>
                        <section role="body" className={props.theme.body}>
                            {props.title ? <h6 className={props.theme.title}>{props.title}</h6> : null}
                            {props.children}
                        </section>
                        {actions.length
                            ? <nav role="navigation" className={props.theme.navigation}>
                                {actions}
                            </nav>
                            : null
                        }
                    </div>
                </Portal>
            )
        }
    };

    return ActivableRenderer()(Dialog);
};

const Dialog = factory(InjectOverlay, InjectButton);

export default Dialog;
