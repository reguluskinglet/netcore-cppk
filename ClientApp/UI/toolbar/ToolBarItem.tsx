import * as React from 'react'

const ToolBarItem = ({text, onClick, children = null, icon=''}) => {
    return (
        <li className="r-tool-bar-item" onClick={onClick}>
            <div className="tool-bar-text">
                {icon ? <span className={'bar-icon ' + icon} /> : null}
                <span>{text}</span>
            </div>
            {children}
        </li>
    )
} 

export default ToolBarItem