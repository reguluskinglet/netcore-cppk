import './theme.scss'
import * as React from 'react'
import * as classnames from 'classnames';
import Portal from '../hoc/Portal';
import ActivableRenderer from '../hoc/ActivableRenderer';
import InjectButton from '../button/Button';
import InjectOverlay from '../overlay/Overlay';

interface DialogProps {
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

    classBody?: string

    loadingText?: string
    isLoaded: boolean
    fixed: boolean

    width?: number
}

interface DialogPropsActions {
    className?: string
    label?: string
    children?: React.ReactNode
}

interface DialogPropsTheme {
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
    isLoaded: true,
    theme: {
        active: 'active',
        body: 'body',
        button: 'button',
        dialog: 'dialog',
        navigation: 'navigation',
        overlay: 'backdrop fix',
        title: 'title',
        wrapper: 'overlay'
    }
}

const factory = (Overlay, Button) => {
    class DialogForm extends React.Component<DialogProps, any> {

        topPosition: number;


        componentDidMount() {

            var doc = document.documentElement;
            this.topPosition = ((window.pageYOffset || doc.scrollTop) - (doc.clientTop || 0)) + 54;
        }

        scrollBody = (e) => {
            var doc = document.documentElement;
            var left = (window.pageXOffset || doc.scrollLeft) - (doc.clientLeft || 0);
            var top = (window.pageYOffset || doc.scrollTop) - (doc.clientTop || 0);

            console.log(top)
        }

        static defaultProps = {
            ...defaults
        };

        render() {

            const { ...props } = this.props;

            const actions = props.actions.map((action, idx) => {
                return <Button key={idx} {...action}>{action.loading ? <div className="loading" /> : null}</Button>; // eslint-disable-line
            });

            const className = classnames('dialog-form', {
                'active': props.active,
                'is-loaded': props.isLoaded === false,
                'fixed': props.fixed
            });

            const classNamePortal = classnames('overlay-form', {
                [props.classBody]: props.classBody != undefined
            });

            return (
                <Portal className={classNamePortal}>
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
                       <div className={className} style={{ top: this.topPosition }}>
                            <section role="body" className="body clear-x">
                                {props.title ? <h6 className={props.theme.title}>{props.title}</h6> : null}
                                {props.children}
                            </section>
                            {actions.length
                                ? <nav className="dialog-actions">
                                    {actions}
                                </nav>
                                : null
                            }
                       </div>
                </Portal>
            )
        }
    };

    return ActivableRenderer()(DialogForm);
};

const DialogForm:any = factory(InjectOverlay, InjectButton);

export default DialogForm;