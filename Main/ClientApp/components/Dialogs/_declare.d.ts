interface InspectionUI {
    id: any
    type: string
    trainName: string
    status: string
    author: string
    brigadeName: string
    date: any
    labels: InspectionLabelUI[]
    inspectionDataCarriages: InspectionDataCarriageUI[]
    inspectionDataUis: InspectionDataUI[]
    taskCount: number,
    signature: string
}

interface InspectionLabelUI {
    carriageName: string
    equipmentName: string
    date: any
    label: string
}

interface InspectionDataCarriageUI {
    carriageName: string
    value: any
}

interface InspectionDataUI {
    type: string
    value: string
}
