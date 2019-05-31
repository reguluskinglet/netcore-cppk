import * as React from 'react'
import * as classnames from 'classnames';

interface HeaderCellProps
{
    style?: any
    column: GridColumn
    sortDirection?: number
    onSortColumn?: (sortOptons) => void
    colSpan?: number
    width?: number
    className?: string
}

export default class HeaderCell extends React.Component<HeaderCellProps, any> {

    get getSortOptions(){
        var sortDirection = this.props.sortDirection;

        if (sortDirection == undefined || sortDirection == 1)
            sortDirection = 0;
        else
            sortDirection = 1;

        return {
            sortOptions: {
                column: this.props.column.name,
                direction: sortDirection
            }
        }
    }

    render() {

        const {sortDirection, onSortColumn, column} = this.props;

        var sort = 'sort-' + sortDirection;

        var className = classnames(this.props.className,{
            [sort]: sortDirection != undefined
        })

        return <th className={className} style={this.props.style} colSpan={this.props.colSpan} onClick={() => { onSortColumn(this.getSortOptions) }}>
            <span>{column.displayName}</span>
            {this.props.children}
        </th>
    }

}