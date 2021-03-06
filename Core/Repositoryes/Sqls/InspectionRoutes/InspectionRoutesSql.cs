﻿using System;
using Rzdppk.Core.Services.Interfaces;
using static Rzdppk.Core.Other.Other;

namespace Rzdppk.Core.Repositoryes.Sqls.InspectionRoutes
{
    public class InspectionRoutesSql : ISqlQueryStorage
    {
        private const string Table = "InspectionRoutes";

        public string Count() 
        {
            return $@"select Count(*) from {Table} ";
        }

        public string Select()
        {
            return $@"select * from {Table} ";
        }

        public string GetAllPaging(int skip, int limit)
        {
            return $@"
            select * from {Table} 
            {SqlPagingSortByIdAsc(skip, limit)}
            ";
        }

        public string GetByRouteId(int routeId)
        {
            return  $@"
                select * from {Table} where RouteId = {routeId}
            ";
        }




        public string Add(int routeId, DateTime start, DateTime end, int? checkListType)
        {
            return $@"
                insert into {Table} (routeId, [Start], CheckListType, [End]) 
                values({routeId}, '{start}', {checkListType} , '{end}')
                SELECT SCOPE_IDENTITY()
            ";
        }

        public string Update(int routeId, DateTime start, DateTime end, int? checkListType, int id)
        {
            return $@"
                update {Table} set routeId = '{routeId}', [start] = '{start}', [end] = '{end}', checkListType = {checkListType} where id = {id}
            ";
        }


        public string Delete(int id)
        {
            return $@"
                delete from {Table} where id = {id}
            ";
        }

        public string ById(int id)
        {
            return $@"
                select * from {Table} where id = {id}                
            ";
        }
    }
}