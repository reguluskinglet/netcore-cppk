﻿import * as React from 'react'
import * as ReactDOM from "react-dom";

interface PortalProps {
    container?: any;
    lockBody?: boolean;
    children?: React.ReactNode
    className?: string
}

class Portal extends React.Component<PortalProps, any> {

    static defaultProps = {
        className: '',
    }

    componentDidMount() {
        this._renderOverlay();
    }

    _overlayInstance: any;
    _overlayTarget: any;
    _portalContainerNode: any;

    componentWillReceiveProps(nextProps) {
        if (this._overlayTarget && nextProps.container !== this.props.container) {
            this._portalContainerNode.removeChild(this._overlayTarget);
            this._portalContainerNode = getContainer(nextProps.container);
            this._portalContainerNode.appendChild(this._overlayTarget);
        }
    }

    componentDidUpdate() {
        this._renderOverlay();
    }

    componentWillUnmount() {
        this._unrenderOverlay();
        this._unmountOverlayTarget();
    }

    getMountNode() {
        return this._overlayTarget;
    }

    getOverlayDOMNode() {
        if (this._overlayInstance) {
            if (this._overlayInstance.getWrappedDOMNode) {
                return this._overlayInstance.getWrappedDOMNode();
            }
            return ReactDOM.findDOMNode(this._overlayInstance);
        }

        return null;
    }

    _getOverlay() {
        if (!this.props.children) return null;
        return <div className={this.props.className}>{this.props.children}</div>;
    }

    _renderOverlay() {
        const overlay = this._getOverlay();
        if (overlay !== null) {
            this._mountOverlayTarget();
            this._overlayInstance = ReactDOM.unstable_renderSubtreeIntoContainer(
                this, overlay, this._overlayTarget,
            );
        } else {
            this._unrenderOverlay();
            this._unmountOverlayTarget();
        }
    }

    _unrenderOverlay() {
        if (this._overlayTarget) {
            ReactDOM.unmountComponentAtNode(this._overlayTarget);
            this._overlayInstance = null;
        }
    }

    _mountOverlayTarget() {
        if (!this._overlayTarget) {
            this._overlayTarget = document.createElement('div');
            this._portalContainerNode = getContainer(this.props.container);
            this._portalContainerNode.appendChild(this._overlayTarget);
        }
    }

    _unmountOverlayTarget() {
        if (this._overlayTarget) {
            this._portalContainerNode.removeChild(this._overlayTarget);
            this._overlayTarget = null;
        }
        this._portalContainerNode = null;
    }

    render() {
        return null;
    }

}

function getContainer(container) {
    const _container = typeof container === 'function' ? container() : container;
    return ReactDOM.findDOMNode(_container) || document.body;
}

export default Portal;