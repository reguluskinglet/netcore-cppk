import { typeName, Action } from 'redux-typed'
import { RequestEvent, GridType } from './enums'


@typeName("RequestLoading")
export class RequestLoading extends Action {

    constructor(public isLoading: boolean){
        super();
    }
}

@typeName('LoadingOverlay')
export class LoadingOverlay extends Action {
    constructor(public showLoading) {
        super()
    }
}

@typeName('Request')
export class RequestAction extends Action {
    constructor(public event: RequestEvent, public showLoading = GridType.none, public data = null) {
    super()
  }
}

@typeName("SHOW_DIALOG")
export class ShowDialog extends Action {
    constructor
        (
        public responceMessage: any,
        public status: number,
        public typeDialog: string,
        public open: boolean,
        public action?: (data) => void
        ) {
        super();

    }
}

@typeName("TOOGLE_DIALOG")
export class ToogleDialog extends Action {
    constructor
        (
        public open: boolean
        ) {
        super();

    }
}

@typeName('show_tooltip')
export class ShowTooltip extends Action {
  constructor(public message: any, public t: number) {
    super()
  }
}

@typeName('show_DeleteDialog')
export class ShowDeleteDialog extends Action {
  constructor(public message: any, public callback: any) {
    super()
  }
}

@typeName('hide_tooltip')
export class HideTooltip extends Action {
  constructor() {
    super()
  }
}

@typeName('UpdateRow')
export class UpdateRow extends Action {
  constructor(public row: any) {
    super()
  }
}

@typeName('INSERT_ROW')
export class InsertRow extends Action {
  constructor(public row: any) {
    super()
  }
}
