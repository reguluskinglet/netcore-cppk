import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../store';
import * as CommonStore from '../../store/CommonStore';
import { Loading } from './Loading'

class LoadingOverlay extends React.Component<Props, undefined> {

    render() {
        return this.props.isShowLoadingOverlay && <><div className="backdrop fix active"></div><Loading /></>
    }
}

const provider = provide((state: ApplicationState) => state.common, CommonStore.actionCreators)

type Props = typeof provider.allProps;

export default provider.connect(LoadingOverlay);
