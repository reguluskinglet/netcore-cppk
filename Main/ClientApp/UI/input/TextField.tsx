import * as React from 'react'

export const TextField = ({title, text}) => {
    return <div className="main-field">
        <label className="field-label">
            <span className="field-title">{title}</span>
            <div className="field-input">
                <div className="field-text">{text}</div>
            </div>
        </label>
    </div>
}