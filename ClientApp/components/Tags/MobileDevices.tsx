import * as React from 'react'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from './store'
import Table from '../../UI/table'
import { map, get, find, concat, isEmpty } from 'lodash'
import { Select } from 'antd'
import PrintSettings from './PrintSettings'
const Option = Select.Option

class CLS extends React.Component<any, any> {
  constructor(props) {
    super(props)
    this.state = {}
  }

  componentDidMount() {
    this.props.getLinks(undefined)
  }

  render() {
    const { md } = this.props
    const { columns = [], rows = [], total = 0 } = md
    const {} = this.state
    return (
      <div>
        <PrintSettings
          templates={this.props.templates}
          printers={this.props.printers}
          procent={this.props.procent}
          print1={this.props.print1}
          print={this.props.print}
          enabledPrintButton={!isEmpty(this.state.selected) && this.props.isStartPrint === undefined}
          rows={this.state.selected}
          getPrintState={this.props.getPrintState}
          t="md"
          stopPrint={this.props.stopPrint}
        />

        <Table
          data={{ columns, rows, total }}
          widthOffset={320}
          reload={({ currentPage, pageSize, sortings, filters }) => {
            this.props.getMdList({
              paging: { skip: currentPage * pageSize, limit: pageSize },
              sortings,
              filters
            })
          }}
          selectable
          onSelect={selected => {this.setState({selected})}}
        />
      </div>
    )
  }
}

const provider = provide(
  (state: any) => state.tags,
  Store.actionCreators
)

type Props = typeof provider.allProps

export default provider.connect(CLS)
