import * as React from 'react'
import HeaderCell from './HeaderCell'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../store';
import * as GridStore from '../../store/GridStore';
import Checkbox from '../checkbox/Checkbox'

interface ExternalProps {
    addClassName?: (columnName) => string
    withcellText?: string

    isTree?: boolean
    withColumnEdit?: boolean
    withSelectAllCheckBox?: boolean
    withCheckBox?: boolean

    selectAll?: (checked, e?) => void
}

class Header extends React.Component<Props, undefined>{   

    render() {

        const { sortColumn, sortOptions, visibleColumns, withcellText, isTree, withColumnEdit, withSelectAllCheckBox, selectAll, isSelectedAll } = this.props;
        var maxLevel = 1; //TODO доделать должны получать с сервера

        return (
            <tr>
                {isTree && <th />}
                {withColumnEdit && <th />}
                {withSelectAllCheckBox ?
                    <th className="fix row-check">
                        <Checkbox checked={isSelectedAll} onChange={(checked, event) => selectAll(checked, event)} disabled={!withSelectAllCheckBox} />
                    </th>
                    : null
                }
                {withcellText ? <th /> : null}
                {visibleColumns.map((x, i) => <HeaderCell
                    key={i}
                    column={x}
                    sortDirection={sortOptions.column == x.name ? sortOptions.direction : null}
                    onSortColumn={sortColumn}
                    width={x.width}
                    colSpan={!isTree ? null: i == 0 ? maxLevel + 1 : null}/>
            )}
            </tr>
        )
    }
}


const provider = provide(
    (state: ApplicationState) => state.grid,
    GridStore.actionCreators
).withExternalProps<ExternalProps>();

type Props = typeof provider.allProps;

export default provider.connect(Header);