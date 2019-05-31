import * as React from 'react'
import * as classnames from 'classnames';
import { Button } from '../button'
import { Loading } from '../loading'

interface ToolBarButtonProps {
    label: string
    right?: boolean
    red?: boolean
    disabled?: boolean
    onClick?: () => void
    showLoading?: boolean
    href?: any
    hide?: boolean
}

const ToolBarButton = ({ label, right = false, red = false, disabled = false, onClick, showLoading, href, hide = false }: ToolBarButtonProps) => {

    if (hide)
        return null;

    return (
        <li className={classnames({ 'right': right })} >
            <Button label={label} red={red} onClick={onClick} disabled={disabled} href={href} />
            {showLoading ? <Loading className="bar-btn-load" text={''} /> : null}
        </li>
    )
}

export default ToolBarButton