import * as React from 'react'
import * as classnames from 'classnames'

interface HeaderCellProps {
    style?: any
    column?: string
    sortDirection?: number
    className?: string

    onSortColumn?: (sortOptons) => void
    onCheck?: (event) => void

    showCheckbox?: boolean
    checkboxValue?: boolean
}

export default class HeaderCell extends React.Component<HeaderCellProps, any> {

    get getSortOptions() {
        var sortDirection = this.props.sortDirection;

        if (sortDirection == undefined || sortDirection == 1)
            sortDirection = 0;
        else
            sortDirection = 1;

        return {
            column: this.props.column,
            direction: sortDirection
        }
    }

    render() {
        const { sortDirection, className} = this.props;

        var sort = ' sort-' + sortDirection;

        var classname = classnames(className, {
            [sort]: sortDirection != undefined
        })

        const classes = classname || classnames('header-cell', 'flex', 'layout', 'horizontal', 'center')
        return (
            <div
                className={classes}
                onClick={() => { this.props.onSortColumn(this.getSortOptions) }}
            >
                <span className="flex">{this.props.children}</span>
                {this.props.showCheckbox && (
                    <input type="checkbox" checked={this.props.checkboxValue} onChange={this.props.onCheck} />
                )}{' '}
            </div>
        )
    }
}
