import * as React from 'react'

const InputNumber = ({value,title, handleChange, min=1, max=50}) => {
    //return <Input type="number" name="count" value={1} title="Количество добавляемых ОУ:" handleChange={handleChange} />

    var valueLink = {
        value: value,
        requestChange: handleChange
    };

    return <div className="main-field">
        <label className="field-label">
            <span className="field-title">{title}</span>
            <div className="field-input">
                <input type="number" min={min} max={max} valueLink={valueLink} style={{ width: '320px' }} />
            </div>
        </label>
    </div>
}

export default InputNumber