import * as React from 'react'
import DatePicker from './DatePicker'

const DatePickerRange = ({valueStart, nameStart, valueEnd, nameEnd, title, handleChange}) => {

    return (
        <div className="main-field-range">
            <div className="field-title">{title}</div>
            <div className="field-body">
                <DatePicker value={valueStart} name={nameStart} handleChange={handleChange} />
                <span className="range-title">-</span>
                <DatePicker value={valueEnd} name={nameEnd} handleChange={handleChange} />
            </div>
        </div>
    )
}

export default DatePickerRange