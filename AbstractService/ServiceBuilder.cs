﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MapleAutoBooster.Abstract
{
    public class ServiceBuilder
    {
        public static AbstractBoosterService ReBuildService(ServiceConfig config, bool Runtime)
        {
            Type serviceType = Type.GetType(config.ServiceTypeId);
            if (serviceType == null || !typeof(AbstractBoosterService).IsAssignableFrom(serviceType))
                return null;
            AbstractBoosterService service = (AbstractBoosterService)Activator.CreateInstance(serviceType);
            service.Id = config.Guid;
            service.ServiceTypeId = config.ServiceTypeId;
            service.ServiceName = config.ServiceName;
            service.ServiceGroup = config.ServiceGroup;
            service.ServicePolicy = config.ServicePolicy;
            service.ServiceDescription = config.ServiceDescription;
            service.Operations = new List<OperateObject>();

            //解析所有的操作
            var operations = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(config.Operations);
            if (operations != null && operations.Count > 0)
            {
                foreach (var item in operations)
                {
                    var opObject = new OperateObject()
                    {
                        OperateId = item["OperateId"].ToString(),
                        OperateTarget = item["OperateTarget"].ToString(),
                        OperateDescription = item["OperateDescription"].ToString(),
                        Operations = new List<IOperation>(),
                    };
                    foreach (var opItem in item["Operations"] as JArray)
                    {
                        IOperation operation = new Operation(opItem["OperationString"].ToString());
                        operation.HandleOperationMethod(service, (m, p) =>
                         {
                             if (operation.ValidateOperationMethod(m, p))
                                 opObject.Operations.Add(operation);
                         });
                    }
                    service.Operations.Add(opObject);
                }
            }
            return service;
        }
    }
}
