import * as React from 'react'
import * as classnames from 'classnames';
import * as ReactDOM from 'react-dom';

import { Button } from '../../UI/button'

interface Props {
    right?: boolean
    text: string
    children?: any
    disabled?: boolean
}

export default class ToolBarItemMenu extends React.Component<Props, any> {

    state = {
        isOpen: false,
    }

    toggleMenu = () => {

        if (this.props.disabled)
            return;

        this.setState({
            isOpen: !this.state.isOpen
        })
    }

    handleClick = (e) => {
        if (this._node && !this._node.contains(e.target)) {
            this.setState({ isOpen: false })
        }
    }

    componentWillMount(){
        document.addEventListener('click', this.handleClick, false);
    }

    componentWillUnmount(){
        document.removeEventListener('click', this.handleClick, false);
    }

    _node: any

    render() {

        const className = classnames('r-tool-bar-item', {
            'right': this.props.right,
        })

        const classNameMenu = classnames('menu-drop-down', {
            'open': this.state.isOpen
        })

        const classNameBarMenu = classnames('tool-bar-menu', {
            'disabled': this.props.disabled
        })

        return (
            <li className={className}  ref={(node) => { this._node = node; }}>
                <div className={classNameBarMenu} onClick={this.toggleMenu}>
                    <div className="tool-bar-text">{this.props.text}</div>
      
                    <ul className={classNameMenu} ref="menu">
                        {this.props.children.map(el => <li>{el}</li>)}
                    </ul>
                </div>
            </li>
        )
    }
}