import * as React from 'react'
import moment from 'moment'
import { TimePicker, Button } from 'antd'

export default class DateEditor extends React.Component<any, any> {
  constructor(props: any) {
    super(props)
    this.state = {
      open: false,
      date: null
    }
  }

  componentDidMount() {
    const date = this.props.value
      ? moment(this.props.value, 'DD.MM.YYYY HH:mm')
      : null
    this.setState({ date })
  }

  componentWillReceiveProps(props) {
    if (props.value !== this.props.value) {
      const date = props.value ? moment(props.value, 'DD.MM.YYYY HH:mm') : null
      this.setState({ date })
    }
  }

  render() {
    const {
      row,
      column,
      value,
      editingEnabled,
      onValueChange,
      timeFormat,
      ...rest
    } = this.props
    const { date } = this.state
    return (
      <td
        {...{ colspan: '1' }}
        style={{ verticalAlign: 'middle', padding: '1px' }}
      >
        <input
          {...rest}
          className="form-control"
          disabled={!editingEnabled}
          type={'date'}
          value={(date && date.format('YYYY-MM-DD')) || null}
          onChange={e => {
            const date = moment(e.target.value)
            this.setState({ date })
          }}
        />
        <TimePicker
          disabled={!editingEnabled}
          open={this.state.open}
          onOpenChange={open => this.setState({ open })}
          value={date}
          format={timeFormat || 'HH:mm'}
          onChange={date => {
            this.setState({ date })
          }}
          addon={() => (
            <Button
              size="small"
              type="primary"
              onClick={() => {
                // console.log(date, ' => ', date.format('YYYY-MM-DDTHH:mm:ss'))

                onValueChange(date.format('YYYY-MM-DDTHH:mm:ss') + 'Z')
                this.setState({ open: false })
              }}
            >
              Ok
            </Button>
          )}
        />
      </td>
    )
  }
}
