import { ActionCreatorGeneric } from 'redux-typed'
import * as CommonStore from './CommonStore'
import * as UserStore from './UserStore'
import * as IncidentStore from './IncidentStore'
import * as EquipmentStore from './EquipmentStore'
import * as FailsStore from './FailsStore'
import * as DirectionsStore from './DirectionsStore'
import * as BrigadesStore from './BrigadesStore'
import * as EmployeesStore from './EmployeesStore'
import * as LocationsStore from '../components/Locations/LocationsStore'
import * as ModelsStore from './ModelsStore'
import * as StationsStore from '../components/Stations/store'
import * as TrainsStore from '../components/Trains/TrainsStore'
import * as TasksStore from './TasksStore'
import * as TaskStore from './TaskStore'
import * as TaskCreateStore from './TaskCreateStore'
import * as JournalsStore from '../components/Journals/store'
import * as ReportsStore from './ReportsStore'
import * as RolesStore from './RolesStore'
import * as UsersStore from './UsersStore'
import * as ScheduleEventsStore from './ScheduleEventsStore'
import * as ScheduleBrigadesStore from './ScheduleBrigadesStore'
import * as TagsStore from '../components/Tags/store'
import * as TagStore from '../components/Tags/TagStore'
import * as TemplatesStore from './TemplatesStore'
import * as GoepStore from '../components/Planning/Goep/store'
import * as DispatcherStore from '../components/Planning/Dispatcher/store'
import * as EditGoepStore from '../components/Planning/Goep/Graph/store'
import * as NpmnStore from '../components/Planning/Nmpn/store'
import * as PouStore from '../components/Planning/Pou/store'
import * as MonitorsStore from './MonitorsStore'
import * as GiveStore from '../components/Give/store'
import * as ServiceStore from '../components/Service/store'
import * as ParkingStore from '../components/Parking/ParkingStore'
import * as RoutesStore from '../components/Routes/store'
import * as FailsMDStore from '../components/FailsMD/store'
import * as DialogStore from './DialogStore';
import * as GridStore from './GridStore';

export interface ApplicationState {
    dispetcher: DispatcherStore.State
    dialog: DialogStore.DialogState
    grid: GridStore.GridState
    common: CommonStore.CommonState
    user: UserStore.UsersState
    incident: IncidentStore.IncidentState
    equipment: EquipmentStore.EquipmentState
    fails: FailsStore.State
    directions: DirectionsStore.State
    brigades: BrigadesStore.State,
    employees: EmployeesStore.State,
    locations: LocationsStore.State
    models: ModelsStore.State
    stations: StationsStore.State
    trains: TrainsStore.State
    tasks: TasksStore.State
    task: TaskStore.State
    journals: JournalsStore.State
    reports: ReportsStore.State
    taskCreate: TaskCreateStore.State
    roles: RolesStore.State
    users: UsersStore.State
    scheduleEvents: ScheduleEventsStore.State
    scheduleBrigades: ScheduleBrigadesStore.State
    tags: TagsStore.State
    tag: TagStore.State
    templates: TemplatesStore.State
    goep: GoepStore.State
    editGoep: EditGoepStore.State
    npmn: NpmnStore.State
    pou: PouStore.State
    monitors: MonitorsStore.State
    give: GiveStore.State
    serv: ServiceStore.State
    parking: ParkingStore.State
    routes: RoutesStore.State
    failsmd: FailsMDStore.State
}

export const reducers = {
    dispetcher: DispatcherStore.reducer,
    common: CommonStore.reducer,
    user: UserStore.reducer,
    incident: IncidentStore.reducer,
    equipment: EquipmentStore.reducer,
    fails: FailsStore.reducer,
    directions: DirectionsStore.reducer,
    brigades: BrigadesStore.reducer,
    employees: EmployeesStore.reducer,
    locations: LocationsStore.reducer,
    models: ModelsStore.reducer,
    stations: StationsStore.reducer,
    trains: TrainsStore.reducer,
    tasks: TasksStore.reducer,
    task: TaskStore.reducer,
    journals: JournalsStore.reducer,
    reports: ReportsStore.reducer,
    taskCreate: TaskCreateStore.reducer,
    roles: RolesStore.reducer,
    users: UsersStore.reducer,
    scheduleEvents: ScheduleEventsStore.reducer,
    scheduleBrigades: ScheduleBrigadesStore.reducer,
    tags: TagsStore.reducer,
    tag: TagStore.reducer,
    templates: TemplatesStore.reducer,
    goep: GoepStore.reducer,
    editGoep: EditGoepStore.reducer,
    npmn: NpmnStore.reducer,
    pou: PouStore.reducer,
    monitors: MonitorsStore.reducer,
    give: GiveStore.reducer,
    serv: ServiceStore.reducer,
    parking: ParkingStore.reducer,
    routes: RoutesStore.reducer,
    failsmd: FailsMDStore.reducer,
    dialog: DialogStore.reducer,
    grid: GridStore.reducer
}

export type ActionCreator = ActionCreatorGeneric<ApplicationState>
