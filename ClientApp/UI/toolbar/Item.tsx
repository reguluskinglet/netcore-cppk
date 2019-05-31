import * as React from 'react'
import * as classnames from 'classnames';

const Item = ({ right = false, children = null }: { right?: boolean, children?: any }) => {
    return (
        <li className={classnames({ 'right': right})} >
            {children}
        </li>
    )
}

export default Item