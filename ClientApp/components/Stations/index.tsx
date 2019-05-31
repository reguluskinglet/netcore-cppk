import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import Table from '../../UI/table'
import { map, get, find } from 'lodash'
import { Select } from 'antd'
const Option = Select.Option

export const TYPES = [{ label: 'Депо', value: 0 }, { label: 'Вокзал', value: 1 }]

class Stations extends React.Component<Props, any> {
  constructor(props) {
    super(props)
    this.state = {
      paging: {
        skip: 0,
        limit: 10
      },
      filters: [],
      sortings: []
    }
  }

  componentDidMount() {
    this.props.getList({ ...this.state })
  }

  componentWillReceiveProps(nextProps) {
    nextProps.reload && this.props.getList({ ...this.state })
  }

  render() {
    const { data } = this.props
    const {} = this.state
    return (
      <div>
        <Table
          data={data}
          widthOffset={320}
          reload={({ currentPage, pageSize, sortings, filters }) => {
            this.props.getList({
              paging: { skip: currentPage * pageSize, limit: pageSize },
              sortings,
              filters
            })
          }}
          editing
          add
          edit
          del
          commitChanges={(changes, { currentPage, pageSize, sortings, filters }) => {
            map(get(changes, 'changed'), row => {
              return this.props.add({
                stantion: {
                  Name: row.col0,
                  Description: '',
                  StantionType: get(find(TYPES, { label: row.col1 }), 'value'),
                  id: get(row, 'id.id') || undefined
                }
              })
            })
            map(get(changes, 'added'), row => {
              return this.props.add({
                stantion: {
                  Name: row.col0,
                  Description: '',
                  StantionType: get(find(TYPES, { label: row.col1 }), 'value'),
                  id: get(row, 'id.id') || undefined
                }
              })
            })
            map(get(changes, 'deleted'), row => {
              return this.props.del(row.id.id)
            })
          }}
          editors={[
            {
              columnName: 'col1',
              editor: ({ row, column, value, editingEnabled, onValueChange }) => {
                return (
                  <td {...{ colspan: '1' }} style={{ verticalAlign: 'middle', padding: '1px' }}>
                    <Select
                      value={value}
                      disabled={!editingEnabled}
                      onChange={(e: any) => {
                        onValueChange(get(find(TYPES, { label: e }), 'label'))
                      }}
                      style={{ width: '100%' }}
                    >
                      <Option key={-1} />
                      {map(TYPES, day => (
                        <Option key={day.label} value={day.label}>
                          {day.label}
                        </Option>
                      ))}
                    </Select>
                  </td>
                )
              }
            }
          ]}
        />
      </div>
    )
  }
}

const provider = provide((state: ApplicationState) => state.stations, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(Stations)
