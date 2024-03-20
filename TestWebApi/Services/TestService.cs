using Lydong.DynamicApi.Marks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TestWebApi.Params;

namespace TestWebApi.Services
{
    public class TestService: IDynamicApi
    {

        //GET /api/test/t1
        public string GetT1()
        {
            return "t1";
        }


        //POST /api/test/t2
        [HttpPost]
        public string GetT2()
        {
            return "t2";
        }

        //GET /api/test/t3 query
        public string GetT3(TestParam p)
        {
            return $"t3/{p.Id}/{p.Name}/{p.Age}";
        }

        //POST /api/test/t4 body
        [HttpPost]
        public string T4(TestParam p)
        {
            return $"t4/{p.Id}/{p.Name}/{p.Age}";
        }


        //PUT /api/test/t5 body
        [HttpPut]
        public string T5(TestParam p)
        {
            return $"t5/{p.Id}/{p.Name}/{p.Age}";
        }


        //DELETE /api/test/t6 body
        [HttpDelete]
        public string T6(TestParam p)
        {
            return $"t6/{p.Id}/{p.Name}/{p.Age}";
        }

        //GET /api/test/t7 route
        [Route("{id}")]
        public string GetT7([FromRoute]string id)
        {
            return $"t7/{id}";
        }

        //不暴露
        [NonDynamicApi]
        public string GetT8()
        {
            return "t8";
        }

        //不暴露
        private string GetT9()
        {
            return "t9";
        }
    }
}
