import * as React from 'react'
import NavMenu from './NavMenu'
import Tooltip from './Tooltip'
import DeleteDialog from './DeleteDialog'
import Dialog from './Dialogs'
import LoadingOverlay from '../UI/loading/LoadingOverlay'

export const PrivateLayout = ({ children }) => (
    <div className="primary-layout" style={{ minWidth: '1024px' }}>
     <LoadingOverlay />
     <Dialog />
    <NavMenu />
    {children}
    <Tooltip />
    <div className="footer bottom-text layout vertical center">{'Разработано компанией АйТи, '+new Date().getFullYear()+' г.'}</div>
    <DeleteDialog />
  </div>
)
