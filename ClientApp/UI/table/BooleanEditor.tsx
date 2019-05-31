import * as React from 'react'
import { get } from 'lodash'

export default class BooleanEditor extends React.Component<any, any> {
  render() {
    const { row, column, value, editingEnabled, onValueChange, ...rest } = this.props
    return (
      <td {...{ colspan: '1' }} style={{ verticalAlign: 'middle', padding: '1px' }}>
        <div className="form-control">
          <input
            {...rest}
            disabled={!editingEnabled}
            type={'checkbox'}
            checked={get(value, 'props.checked')}
            onChange={e => {
              onValueChange(e.target.checked)
            }}
          />
        </div>
      </td>
    )
  }
}
