import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import Table from '../../UI/table'
import { get } from 'lodash'
import { RouteComponentProps } from 'react-router-dom'
import { history } from '../../main'

class CLS extends React.Component<Props, any> {
  constructor(props) {
    super(props)
    this.state = {
      currentPage: 0,
      pageSize: 10,
      sortings: [],
      filters: []
    }
  }

  componentDidMount() {
    const { currentPage, pageSize, sortings, filters } = this.state
    this.props.getList({
      paging: {
        skip: currentPage * pageSize,
        limit: pageSize
      },
      filters,
      sortings
    })
  }

  render() {
    const { data } = this.props
    const { columns = [], rows = [], total = 0 } = data

    const {} = this.state

    columns.length &&
      columns[0].type !== 'link' &&
      columns.splice(0, 0, {
        name: '',
        title: '',
        type: 'link',
        getLink: (row, col) => {
          return `/administration/service/${row.id.id}`
        }
      })
    return (
      <div>
        <Table
          reload={({ currentPage, pageSize, sortings, filters }) => {
            this.props.getList({
              paging: {
                skip: currentPage * pageSize,
                limit: pageSize
              },
              filters,
              sortings
            })
          }}
          data={{ columns, rows, total }}
          widthOffset={320}
          editing
          add
          addComponent={() => (
            <th
              {...{
                colspan: '1',
                style: {
                  position: 'relative',
                  userSelect: 'none',
                  cursor: 'pointer'
                }
              }}
            >
              <a
                onClick={() => {
                  history.push('/administration/service/new')
                }}
              >
                Создать
              </a>
            </th>
          )}
        />
      </div>
    )
  }
}

const provider = provide(
  (state: ApplicationState) => state.serv,
  Store.actionCreators
)

type Props = typeof provider.allProps

export default provider.connect(CLS)
