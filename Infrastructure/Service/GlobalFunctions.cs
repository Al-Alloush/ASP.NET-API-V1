using Core.Models.Error;
using Core.Models.Identity;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class GlobalFunctions
    {

        private readonly AppAllDbContext _context;

        public GlobalFunctions()
        {
            _context = new AppAllDbContext();
        }

        /// <summary>
        /// ErrorTypes: 
        /// </summary>
        /// <remarks>
        /// <para>msg:  Error message</para>
        /// <para>ErrorType: 1- Exception : Exception in Code, 2- third Party,
        /// ErrorType: 2- Server : Server Error,
        /// ErrorType: 3- ThirdParty : Third Party like Email Sender API,
        /// ErrorType: 4- Other : Other Errors </para>
        /// <para>line: Error Line</para>
        /// </remarks>
        public async Task ErrorRegister(string msg, int statusCode = 0, string details = null, int errorTypeId = 4, string filePath = null, int errorLine=0)
        {
            var fileP = "";
            if (!string.IsNullOrEmpty(filePath))
                fileP = $"File Path:{filePath}\n";

            if (errorLine != 0)
                fileP = $"{fileP}in line: {errorLine}\n";

            var appError = new AppError
            {
                Message = msg,
                StatusCode = statusCode,
                ErrorTypeId = errorTypeId,
                Details = fileP + details ,
                ErrorDate = DateTime.Now
            };
            await _context.AppError.AddAsync(appError);
            await _context.SaveChangesAsync();

            // add this code where errors need to register
            /************************* ADD Error In AppError Table ****************************/
            //await _globalFunc.ErrorRegister(msg: "Some text to descript the error",
            //    statusCode: 0,
            //    errorTypeId: 2, /* the error type: 1: Exception, 2:Server, 3: third party, 4: Other */
            //    filePath: new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName(),/* Get File Name */
            //    errorLine: (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()); /* Get Error Line */
            /************************* END ADD Error In AppError Table ************************/

        }


        /// <summary> Calculate the age </summary>
        /// <returns> If the age is less than the specified age return true, else return false.</returns> 
        public bool NotAllowedAge(DateTime bornDate)
        {
            DateTime today = DateTime.Today;
            var _bornDate = (DateTime)bornDate;
            int age = today.Year - _bornDate.Year;
            if (_bornDate > today.AddYears(-age))
                age--;

            if (age < 18)
                return true;
            else
                return false;
        }
    }
}
