using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Infrastructure.Service
{
    public class EmailSmsSenderService
    {
        private readonly IConfiguration _config;
        private string _SendGridAPI_Key;
        private string _SendGridAPI_Email;
        private string _SendSmsAccountSID;
        private string _SendSmsAuthToken;
        private string _SendSmsNumber;

        public EmailSmsSenderService(IConfiguration config)
        {
            _config = config;
            _SendGridAPI_Key = _config["SendGridAPI:SendGridApiKey"];
            _SendGridAPI_Email = _config["SendGridAPI:OutputEmail"];
            _SendSmsAccountSID = _config["SendSmsTwilioAPI:AccountSID"];
            _SendSmsAuthToken = _config["SendSmsTwilioAPI:AuthToken"];
            _SendSmsNumber = _config["SendSmsTwilioAPI:TwilioPhoneNumber"];
        }

        /// <summary> Send activation link by(SendGrid API) to email </summary>
        /// <returns>
        /// true if sent successfully, else return false </returns> 
        public async Task<bool> SendGridApiEmail(string subject, string to, string messageBody)
        {
            try
            {
                var _client = new SendGridClient(_SendGridAPI_Key);
                var _from = new EmailAddress(_SendGridAPI_Email);
                var _to = new EmailAddress(to);
                var email = MailHelper.CreateSingleEmail(_from, _to, subject, messageBody, messageBody);
                await _client.SendEmailAsync(email);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary> Send SMS(Twilio API) to mobile number with verification code,  and verification link to verify the phone number directly from mobile</summary>
        /// <returns>
        /// true if sent successfully, else return false </returns> 
        public async Task<bool> SmsSender(string phoneNumber, string UserName, string token, string link)
        {
            try
            {
                // Convert the Number to be valid in Twilio API
                var ToPhoneNumber = ConvertNumberToTwilioFormat(phoneNumber);

                // if phone number is valid
                if (!ToPhoneNumber.Contains("false"))
                {
                    // Initialize the Twilio client
                    TwilioClient.Init(_SendSmsAccountSID, _SendSmsAuthToken);

                    // Send a new outgoing SMS by POSTing to the Messages resource
                    await MessageResource.CreateAsync(
                        // the phone number SMS API Twilio has a special format, it must not contain 00, for that change it to 
                        // From number, must be an SMS-enabled Twilio number
                        from: new PhoneNumber(_SendSmsNumber),
                        to: new PhoneNumber(ToPhoneNumber),
                        // Message content
                        body: $"Hey {UserName} the tolken: {token} or click on this link: \n {link}"
                    );
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string ConvertNumberToTwilioFormat(string phoneNumber)
        {
            var _phoneNumber = phoneNumber;
            string first2 = _phoneNumber.Substring(0, 2);
            if (first2 == "00")
            {
                _phoneNumber = "+" + _phoneNumber.Substring(2);
                return _phoneNumber;
            }
            else if (first2.Contains("+"))
                return _phoneNumber;

            return "false";
        }
    }
}
