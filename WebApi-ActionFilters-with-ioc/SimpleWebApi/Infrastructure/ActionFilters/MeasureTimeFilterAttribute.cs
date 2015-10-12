using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleWebApi.Infrastructure.ActionFilters
{
    public class MeasureTimeFilterAttribute : Attribute
    {
        public string Label { get; set; }

        public MeasureTimeFilterAttribute(string label)
        {
            Label = label;
        }
    }
}