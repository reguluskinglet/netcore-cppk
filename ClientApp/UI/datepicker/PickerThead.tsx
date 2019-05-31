import * as React from 'react'

interface PickerTheadProps 
{
    head: any
    children?: React.ReactNode
    onClick?: (d) => void
    onNext?: () => void
    onPrev?: () => void
}

export default class PickerThead extends React.Component<PickerTheadProps, any> {

    render() {

        const {onClick, onNext, onPrev, head, children} = this.props;

        return (
            <thead>
                <tr>
                    <th className="dp-prev" onClick={onPrev}><span>‹</span></th>
                    <th className="dp-switch" colSpan={5} onClick={onClick}>{head}</th>
                    <th className="dp-next" onClick={onNext}><span>›</span></th>
                </tr>
                <tr>
                    {children}
                </tr>
            </thead>
        );
    }
}


