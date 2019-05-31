import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/ScheduleBrigadesStore'
import { Table, HeaderRow, HeaderCell, Row, Cell } from '../../UI/grid'
import { InputField, TextareaField, DropdownField } from '../../UI/fields'
import { GreenButton, BlueButton } from '../../UI/buttons'
import Paginator from '../../UI/paginator/Paginator'

export const days = ['Воскресение', 'Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота']

class ScheduleBrigades extends React.Component<Props, any> {
  componentWillMount() {
    this.setState({
      skip: 0,
      limit: 10,
      loading: false,
      name: null,
      type: null,
      reload: false,
      filter1: null,
      tripId: null,
      inputId: null,
      outputId: null,
      brigadeId: null
    })
    this.props.get(0, 10)
    this.props.getLinks('routes')
  }

  componentWillReceiveProps(nextProps) {
    const { skip, limit } = this.state
    nextProps.reload && this.props.get(skip, limit)
  }

  add = () => {
    const { tripId, inputId, outputId, brigadeId } = this.state
    if (tripId && inputId && outputId && brigadeId) {
      this.props.add({
        tripId,
        inputId,
        outputId,
        brigadeId
      })

      this.setState({
        ...this.state,
        tripId: null,
        inputId: null,
        outputId: null,
        brigadeId: null,
        showAdd: false
      })
    }
  }

  setPage = (skip, l) => {
    const { limit, filter1, filter2 } = this.state
    this.props.get(skip, (l > 0 && l) || limit, filter1, filter2)
    this.setState({ skip })
  }

  render() {
    const { result = { data: [], total: 0 }, trips, inputs, outputs, brigades, routes } = this.props
    const { data, total } = result
    const { skip, limit, showAdd, tripId, inputId, outputId, brigadeId } = this.state
    const permissionsArray = []
    const labelWidth = '60px'
    return (
      <div>
        <div className="table-layout card  layout vertical margin-top">
          <div className="layout horizontal  margin-bottom">
            <GreenButton
              label="Добавить"
              onClick={() => {
                this.props.hideEditors()
                this.setState({ ...this.state, showAdd: true })
              }}
            />
          </div>

          <Table>
            <HeaderRow>
              <HeaderCell>Маршрут</HeaderCell>
              <HeaderCell>День</HeaderCell>
              <HeaderCell>Посадка</HeaderCell>
              <HeaderCell>Высадка</HeaderCell>
              <HeaderCell>Бригада</HeaderCell>
              <HeaderCell>Часов</HeaderCell>
              <HeaderCell className="last header-cell" />
            </HeaderRow>

            {showAdd && (
              <div
                className="layout horizontal center"
                style={{
                  borderLeft: '1px solid #666666',
                  borderTop: '1px solid #666666', borderBottom: '1px solid #666666',
                  borderRight: '1px solid #666666'
                }}
              >
                <div className="flex layout vertical margin-top margin-bottom">
                  <DropdownField
                    labelWidth={labelWidth}
                    label="Маршрут"
                    onChange={event => {
                      const value = parseInt(event.currentTarget.value)
                      this.setState({ ...this.state, tripId: value, inputId: null, outputId: null, brigadeId: null })
                      this.props.getLinks('inputs', value)
                    }}
                    value={tripId}
                    className="margin-left"
                    list={routes}
                    showNull
                  />
                </div>

                <div className="flex layout vertical margin-top margin-bottom">
                  <DropdownField
                    labelWidth={labelWidth}
                    label="Посадка"
                    onChange={event => {
                      const value = parseInt(event.currentTarget.value)
                      this.setState({ ...this.state, inputId: value, outputId: null, brigadeId: null })
                      this.props.getLinks('outputs', [tripId, value])
                    }}
                    value={inputId}
                    list={inputs}
                    className="margin-left"
                    showNull
                  />

                  <DropdownField
                    labelWidth={labelWidth}
                    label="Высадка"
                    onChange={event => {
                      const value = parseInt(event.currentTarget.value)
                      this.setState({ ...this.state, outputId: value, brigadeId: null })
                      this.props.getLinks('brigades', [tripId, inputId, value])
                    }}
                    value={outputId}
                    list={outputs}
                    className="margin-top margin-left"
                    showNull
                  />
                </div>

                <div className="flex layout vertical margin-top margin-bottom">
                  <DropdownField
                    labelWidth={labelWidth}
                    label="Бригада"
                    onChange={event => {
                      const value = parseInt(event.currentTarget.value)
                      this.setState({ ...this.state, brigadeId: value })
                    }}
                    value={brigadeId}
                    list={brigades}
                    className="margin-left"
                    showNull
                  />
                </div>

                <span
                  className="icon-save path1 path2 path3 margin-left margin-top margin-right margin-bottom"
                  onClick={this.add}
                  style={{ fontSize: '30px' }}
                >
                  <span className="path1" />
                  <span className="path2" />
                  <span className="path3" />
                </span>
              </div>
            )}

            {data &&
              data.map((row, index) => (
                <div key={`ct_${index}`}>
                  <Row className={row.expanded || row.showEdit ? 'expanded' : ''}>
                    <Cell>{row.routeName}</Cell>
                    <Cell>{days[row.day]}</Cell>
                    <Cell>{row.input}</Cell>
                    <Cell>{row.output}</Cell>
                    <Cell>{row.brigade && row.brigade.name}</Cell>
                    <Cell>{row.tripTime}</Cell>
                    <Cell className="last cell layout horizontal center-center">
                      <div
                        className="icon-delete"
                        onClick={() => {
                          this.props.del({
                            tripId: row.routeId,
                            inputId: row.inputStationTripId,
                            outputId: row.outputStationTripId
                          })
                        }}
                      />
                    </Cell>
                  </Row>
                </div>
              ))}
          </Table>

          <Paginator
            skip={skip}
            limit={limit}
            total={total}
            setPage={this.setPage}
            onLimitChange={l => {
              this.setState({ ...this.state, limit: l })
              l > 0 && this.setPage(0, l)
            }}
          />
        </div>
      </div>
    )
  }
}

const provider = provide((state: ApplicationState) => state.scheduleBrigades, Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(ScheduleBrigades)
