using Ihelpers.Helpers;
using Core.Logger;
using Newtonsoft.Json;
using Idata.Data.Entities.Ireport;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Azure;

namespace Core.Exceptions
{

    [Serializable]
    /// <summary>
    /// The `ExceptionBase` class is used as a base class for custom exceptions in the application.
    /// </summary>
    public class ExceptionBase : Exception
    {
        public int CodeResult { get; set; } = 0; // The result code
        public string CustomMessage { get; set; } = ""; // Custom error message
		public List<string> CustomMessages { get; set; } = new(); // Custom error messages
		public object? toReturn { get; set; } = null; // Object to return

        private long? user_id = null; // User ID

        public bool reportException = true;

        // Default constructor
        public ExceptionBase() { }

        // Constructor with message
        public ExceptionBase(string message, bool reportException = true) : base(message)
        {
            this.reportException = reportException;
            this.CustomMessage = message;

		}

        // Constructor with message, inner exception, and result code
        public ExceptionBase(string message, Exception inner, int codeResult, bool reportException = true) : base(message, inner)
        {
            CodeResult = codeResult;
            this.reportException = reportException;
			this.CustomMessage = message;

		}

		// Constructor with custom message, message, and result code
		public ExceptionBase(string customMessage, string message, int codeResult, bool reportException = true) : base(message)
        {
            this.CustomMessage = customMessage;
            this.CodeResult = codeResult;
            this.reportException = reportException;
        }

        // Constructor with custom message and result code
        public ExceptionBase(string customMessage, int codeResult, bool reportException = true) : base(customMessage)
		{
            CustomMessage = customMessage;
            CodeResult = codeResult;
            this.reportException = reportException;
        }

        // Constructor with custom message, result code, and user ID
        public ExceptionBase(string customMessage, int codeResult, long? _userId, bool reportException = true) : base(customMessage)
		{
            CustomMessage = customMessage;
            CodeResult = codeResult;
            user_id = _userId;
            this.reportException = reportException;
        }

        // Method to handle exceptions
        public static void HandleException(Exception ex, string customMessage, string? databaseMessage = null, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null, long? userID = null)
        {
            // Try to roll back the current transaction
            if (transaction != null)
            {
                try
                { transaction.Rollback(); }
                catch { }
            }
            
            int codeResult = 0;
            bool reportException = true;

           

            if (ex is ExceptionBase)
            {
                customMessage = ((ExceptionBase)ex).CustomMessage;
                codeResult = ((ExceptionBase)ex).CodeResult;
                reportException = ((ExceptionBase)ex).reportException;
            }

            // Log the error message in a separate task
            Task.Factory.StartNew(() => CoreLogger.LogMessage(customMessage, databaseMessage + ex.ToString(), LogType.Exception, userID, reportException));

#if DEBUG

#else
            //Report the exception to dev team
            //deprecated moved to LoggerBase in Core Module
            // if (reportException && Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("App:Environment") != null && Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("App:Environment") != "dev" && codeResult != 404 && codeResult != 204)
            //{
            //    //Report the exception to dev team
            //    ReportException(ex, customMessage, databaseMessage + ex.ToString(), userID);
            //}
#endif


            // If the exception was thrown intentionally for us then throw it, if not then create a new BaseException for throw
            if (ex is ExceptionBase)
            {
                throw ex;
            }
            else
            {
                if (ex.Message.Contains("Microsoft.EntityFrameworkCore.Query.InvalidIncludePathError"))
                {
                    throw new ExceptionBase(ex.Message, ex.Message, 400);
                }
                throw new ExceptionBase(customMessage, ex.Message, 500);
            }

        }

        // Method to handle exceptions without pause the application flow
        public static void HandleSilentException(Exception ex, string customMessage, string? databaseMessage = null, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null, long? userID = null)
        {
            // Try to roll back the current transaction
            if (transaction != null)
            {
                try
                { transaction.Rollback(); }
                catch { }
            }

            // Log the error message in a separate task if environment is not dev (staging, production... etc)
            Task.Factory.StartNew(() => CoreLogger.LogMessage(customMessage, databaseMessage + ex.ToString(), LogType.Exception, userID));


            int codeResult = 0;
            bool reportException = true;

            if (ex is ExceptionBase)
            {
                customMessage = ((ExceptionBase)ex).CustomMessage;
                codeResult = ((ExceptionBase)ex).CodeResult;
                reportException = ((ExceptionBase)ex).reportException;
            }


#if DEBUG

#else
            //Report the exception to dev team
            if (reportException && Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("App:Environment") != null && Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("App:Environment") != "dev" && codeResult != 404 && codeResult != 204)
            {
                //Report the exception to dev team
                ReportException(ex, customMessage, databaseMessage + ex.ToString(), userID);
            }
#endif


        }
        // Method to create a response from an exception
        public object CreateResponseFromException()
        {

            // Create the list of messages to be returned
            List<object> _messages = new List<object>();

			if (!this.CustomMessages.Contains(this.CustomMessage) && !string.IsNullOrEmpty(this.CustomMessage))
			{
				this.CustomMessages.Add(this.CustomMessage);

			}
			foreach (var customMessage in this.CustomMessages)
            {
				// Create a message to add to the list
				object message = new
				{
					message = $"{customMessage}",
					type = "error"
				};

				// Add the message to the list
				_messages.Add(message);
			}

            //Create the return object that contains the error messages matching front needs
            object toReturn = new
            {
                messages = _messages
            };
            //return the object
            return toReturn;
        }


        private static void ReportException(Exception ex, string message, string stackTrace, long? UserId)
        {

            string sourceApp = CoreLogger.sourceApp;

            string environment = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("App:Environment");
            //Obtener el message provider actual y enviar la excepcion
            Task.Factory.StartNew(() => Ihelpers.Extensions.ConfigContainer.messageProvider?.SendMessageAsync($"{JsonConvert.SerializeObject(new { recipient = new { email = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string[]>("LaravelSync:DevelopersEmail") }, link = $"", title = $"{sourceApp} ({environment}) {message}", icon_class = "fa-bell", message = $"<details><summary>StackTrace</summary><p>{stackTrace}</p></details>", setting = new { saveInDatabase = 1 }, is_action = true, data = "" })}", "platform.event.exceptions"));

        }
    }

}
