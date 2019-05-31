import './toolbar.scss'
import * as React from 'react'


export default class ToolBar extends React.Component<any, any> {

    render() {
        return (
            <div className="r-tool-bar">
                <ul className="r-tool-bar-items">
                    {this.props.children}
                </ul>
                <div className="clear-x" />
            </div>
        )
    }
}