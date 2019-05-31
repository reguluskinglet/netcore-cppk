import './filter.scss'
import * as React from 'react'
import { provide } from 'redux-typed';
import { ApplicationState } from '../../store';
import * as GridStore from '../../store/GridStore';

import { AutocompliteDataSource, DropDown } from '../../UI/dropdown'
import { DatePicker, DatePickerRange } from '../../UI/datepicker'
import { Input } from '../../UI/input'
import { FilterInfo, ReferenceBookType } from '../../common'
import { dataSourceSelect } from '../../resource'
import { Button } from '../../UI/button'
import thunk from 'redux-thunk';



interface ExternalProps
{
    filterTitle: any;
    onlyFilter?: boolean;
    autocompliteDataSource?: (name) => SelectItem[];
    additionalFilter?: any;
}

class Filter extends React.Component<Props, any> {



    getValue(conditionName: string) {

        var value;

        if (this.props.grid.filter)
            value = this.props.grid.filter[conditionName];

        if (value == null)
            value = '';

        return value;
    }

    applyDependentFilter=(dataSource: SelectItemDependent[], condition: string)=> {

        if (condition === 'carriageId') {
            var trainId = this.getValue('trainId');

            if (trainId)
                dataSource = dataSource.filter(x => x.dependentId === trainId);

        }
        else if (condition === 'trainId') {
            var stantionId = this.getValue('stantionId');

            if (stantionId)
                dataSource = dataSource.filter(x => x.dependentId === stantionId);

        }
        return dataSource;
    };

    getFilter(filterInfo: FilterInfo) {
        switch (filterInfo.filterType) {
            case 0:
                return <Input
                    name={filterInfo.conditionName}
                    value={this.getValue(filterInfo.conditionName)}
                    title={this.props.filterTitle[filterInfo.conditionName]}
                    handleChange={this.handleChange}
                />;
            case 1:

                var dataSource: SelectItemDependent[];

                if (this.props.autocompliteDataSource)
                    dataSource = this.props.autocompliteDataSource(filterInfo.conditionName);

                if (!dataSource)
                    dataSource =  this.props.grid.data.dataSource[ReferenceBookType[filterInfo.data]];

                dataSource = this.applyDependentFilter(dataSource, filterInfo.conditionName);

                return <AutocompliteDataSource
                    dataSource={dataSource}
                    name={filterInfo.conditionName}
                    value={this.getValue(filterInfo.conditionName)}
                    title={this.props.filterTitle[filterInfo.conditionName]}
                    handleChange={this.handleChangeAutocomplite}
                />;
            case 2:
                var val;
                if (location.pathname === '/journals' && filterInfo.conditionName === 'status' && !dataSourceSelect[filterInfo.conditionName].find(x => x.value==100)) {
                dataSourceSelect[filterInfo.conditionName] = dataSourceSelect[filterInfo.conditionName]
                        .concat({ value: 100, text: 'Все открытые' });

                val = 100;
                }
                return <DropDown
                    emptyText="Все"
                    dataSource={dataSourceSelect[filterInfo.conditionName]}
                    name={filterInfo.conditionName}
                    value={val === undefined ? this.getValue(filterInfo.conditionName): val}
                    title={this.props.filterTitle[filterInfo.conditionName]}
                    handleChange={this.handleChange} />;
            case 3:
                var condition = filterInfo.conditionName;
                var nameStart = condition + 'Start';
                var valueStart = this.getValue(nameStart);
                var nameEnd = condition + 'End';
                var valueEnd = this.getValue(nameEnd);

                return <DatePickerRange
                    title={this.props.filterTitle[filterInfo.conditionName]}
                    valueStart={valueStart}
                    nameStart={nameStart}
                    valueEnd={valueEnd}
                    nameEnd={nameEnd}
                    handleChange={this.props.changeFilter}
                />;
            case 5:
                var chkBoxVal = this.getValue(filterInfo.conditionName);
                var id = '' + Math.random();
                return <div className="main-field">
                           <div className="field-label label-clear">
                       
                        <div className="field-input">
                            
                            <input id={id} type="checkbox" name={filterInfo.conditionName} checked={chkBoxVal}
                                onChange={() => this.handleChange(chkBoxVal = !chkBoxVal, filterInfo.conditionName)} />
                            &nbsp;
                            <label htmlFor={id} className="field-title" title={this.props.filterTitle[filterInfo.conditionName]}>
                            {this.props.filterTitle[filterInfo.conditionName]}
                        </label>
                               </div>
                           </div>
                       </div>;
            default:
                console.log('Тип фильтра не определен: ' + filterInfo.conditionName);
        }
    }

    handleChange = (newValue, field) => {
        console.log('newVal: ' + newValue);
        this.props.changeFilter(newValue, field);

        if (field === 'floorIds')
            this.props.changeFilter('', 'roomIds');
        else if (field === 'companyIds')
            this.props.changeFilter('', 'divisionIds');
    };

    handleChangeAutocomplite = (newValue: SelectItem, field) => {

        this.props.changeFilter(newValue.value, field);
    };

    clearFilter = () => {
        this.props.clearFilter();
    };

    applyFilter = (e) => {
        e.preventDefault();

        this.props.applyFilter(this.props.grid.filter);
    };

    render() {

        if (!this.props.grid.filterInfo)
            return null;
       
        var filters =  this.props.grid.filterInfo.map((filter, index) =>
                <div className="filter-item" key={filter.conditionName}>
                    {this.getFilter(filter)}
                </div>
            );

        if (this.props.onlyFilter)
            return <div>{filters}</div>;

        return <div className="filter filter-fix">
            <form onSubmit={this.applyFilter}>
                <div className="filter-center">
                    {filters}
                </div>
                <div className="filter-right">
                    <Button label="Применить фильтр" type="submit" />
                    <Button label="Сбросить фильтр" onClick={this.clearFilter} />
                    {this.props.additionalFilter ? this.props.additionalFilter() : null}
                </div>
            </form>
        </div>;
    }
}

const provider = provide(
    (state: ApplicationState) => ({ grid: state.grid, }),
    ({ ...GridStore.actionCreators })
).withExternalProps<ExternalProps>();

type Props = typeof provider.allProps;

export default provider.connect(Filter);