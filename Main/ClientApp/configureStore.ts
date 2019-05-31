import {
  createStore,
  applyMiddleware,
  compose,
  combineReducers,
  GenericStoreEnhancer,
  Store,
  StoreEnhancerStoreCreator,
  ReducersMapObject
} from 'redux'
import thunk from 'redux-thunk'
import { routerReducer, routerMiddleware } from 'react-router-redux'
import * as StoreModule from './store'
import { ApplicationState, reducers } from './store'
import { History } from 'history'
import { typedToPlain } from 'redux-typed'

export default function configureStore(history: History, initialState?: ApplicationState) {
  const composeEnhancers = window['__REDUX_DEVTOOLS_EXTENSION_COMPOSE__'] || compose

  const createStoreWithMiddleware = composeEnhancers(applyMiddleware(thunk, typedToPlain, routerMiddleware(history)))(
    createStore
  )

  const allReducers = buildRootReducer(reducers)
  const store = createStoreWithMiddleware(allReducers, initialState) as Store<ApplicationState>

  if (module.hot) {
    module.hot.accept('./store', () => {
      const nextRootReducer = require<typeof StoreModule>('./store')
      store.replaceReducer(buildRootReducer(nextRootReducer.reducers))
    })
  }

  return store
}

function buildRootReducer(allReducers: ReducersMapObject) {
  return combineReducers<ApplicationState>(Object.assign({}, allReducers, { routing: routerReducer }))
}
