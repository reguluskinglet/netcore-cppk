import * as React from 'react'
import { Button } from '../button'

interface InputMultipleProps {
    options: any[]
    handleChange: (selected: any[]) => void
    selectedIds: any[]
    title:string
    onRemove: () => void
}

const InputMultiple = ({options, handleChange, selectedIds, onRemove, title}: InputMultipleProps) => {

    const onChange = (e) => {

        var el = e.target;
        let selectedValues = []
        for (let i = 0, l = el.options.length; i < l; i++) {
            if (el.options[i].selected) {
                selectedValues.push(options[i].value)
            }
        }

        handleChange(selectedValues);
    }

    return (
        <div className="main-select-multi">
            <p className="field-title">{title}</p>
            <Button label="Удалить" red disabled={selectedIds.length == 0} onClick={onRemove} />
            <select multiple onChange={onChange}>
                {options.map(x => <option
                    key={x.value}
                    value={x.value}>{x.fullName ? x.fullName:x.text}</option>)}
            </select>
        </div>
    )
}

export default InputMultiple