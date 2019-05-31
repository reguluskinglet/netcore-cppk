import * as React from 'react'
import { RouteComponentProps } from 'react-router'
import { provide } from 'redux-typed'
import { ApplicationState } from '../../store'
import * as Store from '../../store/CommonStore'
import Modal from 'antd/lib/modal'
import './style.css'
import './button.css'
import { faultTypeLabels } from '../Reports'

class DeleteDialog extends React.Component<Props, any> {
  componentWillMount() {
    this.setState({
      visible: false
    })
  }

  componentWillReceiveProps(props) {
    if (props.common.callback !== this.props.common.callback) {
      this.setState({ ...this.state, visible: true })
    }
  }

  toggleVisibility = () => this.setState({ visible: !this.state.visible })

  render() {
    const { common: { message, callback } } = this.props

    return (
      <div>
        <Modal
          title="Удаление объекта"
          visible={this.state.visible}
          onOk={() => {
            callback()
            this.toggleVisibility()
          }}
          onCancel={this.toggleVisibility}
          okText="Да"
          cancelText="Нет"
          closable={false}
        >
          <p>{message || 'Вы уверены что хотите удалить объект?'}</p>
        </Modal>
      </div>
    )
  }
}

const provider = provide((state: ApplicationState) => ({ common: state.common }), Store.actionCreators)

type Props = typeof provider.allProps

export default provider.connect(DeleteDialog)
