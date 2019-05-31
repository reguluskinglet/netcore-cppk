import './loading.scss'
import * as React from 'react'
import * as classnames from 'classnames';

export const Loading = ({ className = "main-loading", text = 'Загрузка'}) => {
    return (
        <div className={className}>
            <div className="loader" />
            {text ? <span>{text}</span> : null}
        </div>
    )
}