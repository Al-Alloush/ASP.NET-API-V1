using Infrastructure.Data;
using Infrastructure.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Error
{
    public class ApiResponse
    {


        // every error is going to have these two properities
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public ApiResponse(int statusCode, string message = null, bool regError = false, string details = null, int errroType = 4, string filePath = null, int errorLine = 0)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);

            // if this error need to register or any Server Error
            if (regError || statusCode == 500)
                AddError(Message, StatusCode, errroType, details, filePath, errorLine).Wait();
        }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            // fancy switch expressions
            return statusCode switch
            {
                202 => "return null",
                201 => "you have made a success request",
                400 => "you have made a bad request!",
                401 => "you are not Authorized",
                403 => "you don't have the permissions",
                404 => "Resource not found!",
                405 => "Method Not Allowed, A request was made of a resource using a request method not supported by that resource; for example, using GET on a form which requires data to be presented via POST, or using PUT on a read-only resource.",
                500 => "Errors are the path to the dark side. Errors lead to anger.  Anger leads to hate.  Hate leads to a career change",
                _ => null/* _ => the default in switch */
            };
        }

        public async Task AddError(string msg, int statusCode = 0, int typeId = 4, string erDetails = null, string filePath = null, int errorLine = 0)
        {
            var _globalFunc = new GlobalFunctions();
            /************************* ADD Error In AppError Table ****************************/
            await _globalFunc.ErrorRegister(msg: msg,
                statusCode: statusCode,
                details: erDetails, 
                errorTypeId: typeId, /* the error type: 1: Exception, 2:Server, 3: third party, 4: Other */
                filePath: filePath, /* Get File Name */
                errorLine: errorLine); /* Get Error Line */
            /************************* END ADD Error In AppError Table ************************/
        }

    }
}
