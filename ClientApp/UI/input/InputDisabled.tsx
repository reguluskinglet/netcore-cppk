import * as React from 'react'

export const InputDisabled = ({title, value = '', style = null}) => {

    return (
        <div className="main-field">
            <div className="field-label field-disabled">
                <label className="field-title">{title}</label>
                <div className="field-input" {...style}>
                    <input type="text" value={value} disabled />
                </div>
            </div>
        </div>
    )
}