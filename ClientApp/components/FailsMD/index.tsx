import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import Table from '../../UI/table'
import { map, get, find, concat } from 'lodash'
import { Select } from 'antd'
const Option = Select.Option

class CLS extends React.Component<Props, any> {
  constructor(props) {
    super(props)
    this.state = {}
  }

  componentDidMount() {
    this.props.getLinks()
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
          commitChanges={(
            changes,
            { currentPage, pageSize, sortings, filters }
          ) => {
            Promise.all(
              concat(
                map(get(changes, 'changed'), row => {
                  return this.props.add({
                    devicefault: {
                      Name: row.col0,
                      Description: row.col1,
                      id: get(row, 'id.id') || undefined
                    }
                  })
                }),
                map(get(changes, 'added'), row => {
                  return this.props.add({
                    devicefault: {
                      Name: row.col0,
                      Description: row.col1
                    }
                  })
                }),
                map(get(changes, 'deleted'), row => {
                  return this.props.del(row.id.id)
                })
              )
            ).then(res => {
              this.props.getList({
                paging: { skip: currentPage * pageSize, limit: pageSize },
                sortings,
                filters
              })
            })
          }}
        />
      </div>
    )
  }
}

const provider = provide(
  (state: ApplicationState) => state.failsmd,
  Store.actionCreators
)

type Props = typeof provider.allProps

export default provider.connect(CLS)
