using System;
using System.Collections.Generic;
using System.Text;
using WarpCore.Data.Schema;
using WarpCore.DbEngine.Azure;

namespace WarpCore.Crm
{
    public class IndividualRepository
    {
        public void Save(Individual individual)
        {
            new DbEngineAdapter(new AzureTableDataAccessLayer(new AzureClientFactory(AzureClientFactory.Emulate))).Save(individual);
        }
    }
}
