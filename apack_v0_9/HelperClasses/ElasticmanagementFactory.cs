using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apack.HelperClasses
{
    class ElasticFactory
    {
        ElasticServer _elasticServer;

        public ElasticServer GetElasticServer
        {
            get
            {
                if (_elasticServer != null)
                {
                    return _elasticServer;
                }
                _elasticServer = new ElasticServer();
                return _elasticServer;
            }
        }
    }
}
