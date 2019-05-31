import * as React from 'react'

const ReadOnlyField = ({ title, value }) => {
    return (
        <div className="g-field">
            <span className="g-title">{title}</span>
            <span className="g-value">{value}</span>
        </div>
    )
}

export default ReadOnlyField