import * as React from 'react'

interface ActivableRendererProps
{
    active: boolean
    children?: React.ReactNode
    delay?: number
    actions?: any
    onEscKeyDown?: (e) => void
    onOverlayClick?: (e) => void
    title?: string
    className?: string
}

const ActivableRendererFactory = (options = { delay: 500 }) =>
    ActivableComponent => class ActivableRenderer extends React.Component<ActivableRendererProps, any> {

        activateTimeout: any;
        unrenderTimeout: any;

        static defaultProps = {
            delay: options.delay,
        }

        state = {
            active: this.props.active,
            rendered: this.props.active,
        };

        componentWillReceiveProps(nextProps) {
            if (nextProps.active && !this.props.active) this.renderAndActivate();
            if (!nextProps.active && this.props.active) this.deactivateAndUnrender();
        }

        componentWillUnmount() {
            clearTimeout(this.activateTimeout);
            clearTimeout(this.unrenderTimeout);
        }

        renderAndActivate() {
            if (this.unrenderTimeout) clearTimeout(this.unrenderTimeout);
            this.setState({ rendered: true, active: false }, () => {
                this.activateTimeout = setTimeout(() => this.setState({ active: true }), 20);
            });
        }

        deactivateAndUnrender() {
            this.setState({ rendered: true, active: false }, () => {
                this.unrenderTimeout = setTimeout(() => {
                    this.setState({ rendered: false });
                    this.unrenderTimeout = null;
                }, this.props.delay);
            });
        }

        render() {
            const { delay, ...others } = this.props; // eslint-disable-line no-unused-vars
            const { active, rendered } = this.state;
            return rendered
                ? <ActivableComponent {...others} active={active} />
                : null;
        }
    };

export default ActivableRendererFactory;
