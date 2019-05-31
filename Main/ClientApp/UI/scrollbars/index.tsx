import * as React from 'react'
import Scrollbars from 'react-custom-scrollbars';

export const ScrollBarsVertical = ({ width, height, children }) => {
    return <Scrollbars style={{ width: width, height: height }}
        renderTrackHorizontal={props => <div {...props} style={{ display: 'none' }} className="track-horizontal" />}
        renderThumbVertical={({ style, ...props }: any) => (
            <div style={{ ...style, width: 4, cursor: 'pointer', backgroundColor: '#0077977f' }}{...props} />
        )}
    >
        {children}
    </Scrollbars>
}