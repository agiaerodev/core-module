using Ihelpers.Helpers;
using Idata.Data;
using Idata.Entities.Core;
using Newtonsoft.Json;
using Idata.Data.Entities.Iprofile;

namespace Core.Logger
{
    public class CoreLogger
    {
        public static string sourceApp { get; set; } = "Platform";

        public static async void LogMessage(string? message = null, string? stackTrace = null, LogType logType = LogType.Information, long? userId = null, bool? reportException = true)
        {


            Log? common = new();

            common.message = message;

            common.stackTrace = stackTrace;

            common.user_id = userId;

            common.type = Enum.GetName(typeof(LogType), logType);

            common.source_app = sourceApp;

            //call the task without force await
            Task.Factory.StartNew(() => InsertMessage(common));

            if (logType == LogType.Exception && Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("App:Environment") != "dev" && reportException == true) 
            {

                ReportException(stackTrace, message, userId);

            }
        }


        private static async void InsertMessage(Log common)
        {
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
            try
            {
                using (IdataContext _dataContext = new IdataContext())
                {

                    //Begin the transaction
                    transaction = await _dataContext.Database.BeginTransactionAsync();

                    //save the model in database and commit transaction
                    await _dataContext.Logs.AddAsync(common);
                    await _dataContext.SaveChangesAsync(CancellationToken.None);
                    await transaction.CommitAsync();


                }

            }
            catch (Exception ex)
            {
                //This exception can only be debugged in debug mode
            }
        }

        private static void ReportException(string exceptionStackTrace, string message, long? UserId)
        {

            string sourceApp = CoreLogger.sourceApp;

            string environment = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string>("App:Environment");
            //Obtener el message provider actual y enviar la excepcion
            Task.Factory.StartNew(() => Ihelpers.Extensions.ConfigContainer.messageProvider?.SendMessageAsync($"{JsonConvert.SerializeObject(new { recipient = new { email = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string[]>("LaravelSync:DevelopersEmail") }, link = $"", title = $"{sourceApp} ({environment}) {message}", icon_class = "fa-bell", message = $"<details><summary>StackTrace</summary><p>{exceptionStackTrace}</p></details>", setting = new { saveInDatabase = 1 }, is_action = true, data = "" })}", "platform.event.exceptions"));

        }
    }
}
