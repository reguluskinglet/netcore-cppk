import * as React from 'react'
import { DropdownField, InputField } from '../../UI/fields'
import { PrinterButton } from '../../UI/buttons'

const tagTypes = [
  { value: 1, label: 'Бумажные метки' },
  { value: 2, label: 'Корпусные метки' }
]

export default class PrintSettings extends React.Component<any, any> {
    state: any = {}


    get isValid(): boolean {
        const {
            type,
            temp,
            label,
            id,
            connectionType,
            printerName,
            comPort,
            modelName,
            power
        } = this.state
        //   RfidTagWriterDeviceType: printData.modelName || '0',
        //   TagWriterTxPower: printData.power || '0',
        //comPort

        if (type === 1)
            return connectionType && printerName && temp;

        return modelName != undefined && comPort;
    }

  render() {
    const {
      result = { data: [], total: 0 },
      templates,
      printers,
      print1,
      procent,
      enabledPrintButton
    } = this.props
    const { data } = result
    const {
      type,
      temp,
      label,
      id,
      connectionType,
      printerName,
      comPort,
      modelName,
      power
    } = this.state
    const rows1 = data && data.filter(row => row.selected)
    // const enabledPrintButton = (rows1 && rows1.length > 0) || false

    const connectionTypes = []
    printers &&
      printers.paperTag &&
      printers.paperTag.connectedTypes &&
      printers.paperTag.connectedTypes.forEach(element => {
        connectionTypes.push({
          value: parseInt(element.value),
          label: element.text
        })
      })

    const printerNames = []
    const a =
      printers &&
      printers.paperTag &&
      printers.paperTag.connectionTypeItems &&
      printers.paperTag.connectionTypeItems.find(
        e => e.connectionType === connectionType
      )

    a &&
      a.printers &&
      a.printers.forEach(element => {
        printerNames.push({ value: element.value, label: element.text })
      })

    const modelNames = []
    printers &&
      printers.corpTag &&
      printers.corpTag.deviceModels &&
      printers.corpTag.deviceModels.forEach(element => {
        modelNames.push({ value: parseInt(element.value), label: element.text })
      })

    const comPorts = []
    printers &&
        printers.corpTag &&
        printers.corpTag.comPorts.forEach(element => {
        comPorts.push({ value: element.value, label: element.text })
      })

    //if (print1 === true)
    //  // setTimeout(() => {
    //      this.props.getPrintState()


    return (
      <div>
        <div className="add-item card margin-top">
          <div className="layout vertical">
            <div className="layout horizontal">
              <div className="layout horizontal center">
                <DropdownField
                  label="Тип"
                  className="flex"
                  value={type}
                  onChange={event => {
                    const value = parseInt(event.currentTarget.value)
                    this.setState({ ...this.state, type: value })
                  }}
                  list={tagTypes}
                  showNull
                />

                {/* <InputField
                  label="Наименование"
                  className="flex margin-left"
                  value={label}
                  onChange={event => {
                    const value = event.currentTarget.value
                    this.setState({ ...this.state, label: value })
                  }}
                /> */}

                <DropdownField
                  label="Шаблон"
                  className="flex margin-left"
                  value={temp}
                  onChange={event => {
                    const value = parseInt(event.currentTarget.value)
                    this.setState({ ...this.state, temp: value })
                  }}
                  showNull
                  list={templates}
                />

                {/* <span
                  className={`${label
                    ? 'icon-save'
                    : 'icon-save-1'} path1 path2 path3 margin-left`}
                  onClick={() => {
                    label && this.props.save({ type, temp, label, id })
                  }}
                  style={{ fontSize: '30px' }}
                >
                  <span className="path1" />
                  <span className="path2" />
                  <span className="path3" />
                </span> */}
              </div>
            </div>
          </div>
        </div>
        {printers && (
          <div className="add-item card margin-top">
            <div className="layout vertical">
              <div className="layout horizontal">
                <div className="layout horizontal center">
                  <div className="label flex-none margin-right">
                    Настройки печати:
                  </div>

                  {type === 1 && (
                    <DropdownField
                      label="Тип соединения"
                      className="flex margin-left"
                      value={connectionType}
                      onChange={event => {
                        const value = parseInt(event.currentTarget.value)
                        this.setState({
                          ...this.state,
                          connectionType: value
                        })
                      }}
                      list={connectionTypes}
                      showNull
                    />
                  )}

                  {type === 1 &&
                    connectionType === 2 && (
                      <DropdownField
                        label="Имя принтера"
                        className="flex margin-left"
                        value={printerName}
                        onChange={event => {
                          const value = event.currentTarget.value
                          this.setState({ ...this.state, printerName: value })
                        }}
                        showNull
                        list={printerNames}
                      />
                    )}

                  {type === 1 &&
                    connectionType === 0 && (
                      <DropdownField
                        label="COM порт"
                        className="flex margin-left"
                        value={comPort}
                        onChange={event => {
                          const value = event.currentTarget.value
                          this.setState({ ...this.state, comPort: value })
                        }}
                                        showNull
                                        list={comPorts}
                      />
                    )}

                  {type === 2 && (
                    <DropdownField
                      label="Модель"
                      className="flex margin-left"
                      value={modelName}
                      onChange={event => {
                        const value = parseInt(event.currentTarget.value)
                        this.setState({ ...this.state, modelName: value })
                      }}
                      showNull
                      list={modelNames}
                    />
                  )}

                  {type === 2 && (
                    <DropdownField
                      label="COM порт"
                      className="flex margin-left"
                      value={comPort}
                      onChange={event => {
                        const value = event.currentTarget.value
                        this.setState({ ...this.state, comPort: value })
                      }}
                      showNull
                      list={comPorts}
                    />
                  )}

                  {type === 2 && (
                    <InputField
                      label="Мощность"
                      className="flex margin-left"
                      type="number"
                      value={power}
                      onChange={event => {
                        const value = parseInt(event.currentTarget.value)
                        this.setState({ ...this.state, power: value })
                      }}
                    />
                  )}

                  <PrinterButton
                                    label="Печать"
                                    enabled={enabledPrintButton && this.isValid}
                    onClick={() => {
                      // const rows = data && data.filter(row => row.selected)
                      
                      this.props.print(this.props.rows, {
                        connectionType,
                        printerName,
                        comPort,
                        power,
                        modelName,
                        paper: type === 1,
                        template:
                          templates.find(e => e.value === temp) &&
                          templates.find(e => e.value === temp).template
                      }, this.props.t)
                    }}
                    className="margin-left"
                  />

                  {print1 === true && (
                    <div className="margin-left label layout horizontal center">
                      <div>{`Печать меток: ${procent} %`}</div>
                      <span
                        className="icon-close"
                        onClick={() => {
                          this.props.stopPrint()
                        }}
                      />
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    )
  }
}
