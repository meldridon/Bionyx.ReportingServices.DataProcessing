using System;
using System.Runtime.Serialization;
using Bionyx.WebApi.ReportingServices;
using Bionyx.WebApi.ReportingServices.Common;
using DemoReportWebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace DemoReportWebApi.Controllers
{
    [Route("api/reports/[controller]")]
    public class ValuesController : Controller
    {
        [DataContract]
        public class Parameters
        {
            [DataMember]
            public int? P1 { get; set; }

            [DataMember]
            public decimal? P2 { get; set; }

            [DataMember]
            public string P3 { get; set; }
        }

        // GET api/values

        public ValuesController()
        {
            ReportReponseFactory = new ReportReponseFactory();
        }

        public ReportReponseFactory ReportReponseFactory { get; }

        [HttpPost]
        public ReportResponse Post([FromBody] Parameters parameters, [FromQuery]string behavior)
        {
            if (behavior == "schemaOnly") return ReportReponseFactory.CreateSchemaOnlyReportResponse<TestType, Parameters>();

            var response = ReportReponseFactory.CreateMultipleRowReportResponse<TestType, Parameters>();
            response.Value = new[]
            {
                new TestType
                {
                    Id = 1,
                    Date = DateTime.Now,
                    Name = "Record 1",
                    Value = 1234.563456m
                },
                new TestType
                {
                    Id = 2,
                    Date = DateTime.Now.AddMonths(1),
                    Name = "Record 2",
                    Value = 0.0m
                },
                new TestType
                {
                    Id = 3,
                    Date = DateTime.Now.AddYears(1),
                    Name = "Record 3",
                    Value = 829337498234.23847m
                },
            };
            return response;
        }
    }
}
