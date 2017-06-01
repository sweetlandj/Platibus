using System;
using System.Reflection;
using System.Web.Mvc;

namespace Platibus.SampleWebApp.Controllers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SubmitActionAttribute : ActionNameSelectorAttribute
    {
        private readonly string _parameterName;
        private readonly string _parameterValue;

        public SubmitActionAttribute(string parameterName)
        {
            _parameterName = parameterName;
        }

        public SubmitActionAttribute(string parameterName, string parameterValue)
        {
            _parameterName = parameterName;
            _parameterValue = parameterValue;
        }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            var isValidName = false;
            var result = controllerContext.Controller.ValueProvider.GetValue(_parameterName);
            if (result != null)
            {
                var value = result.AttemptedValue;
                isValidName = string.IsNullOrWhiteSpace(_parameterValue) ||
                              string.Equals(value, _parameterValue, StringComparison.OrdinalIgnoreCase);
                controllerContext.Controller.ControllerContext.RouteData.Values[_parameterName] = value;
            }
            return isValidName;
        }
    }
}