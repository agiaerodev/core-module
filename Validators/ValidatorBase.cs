using Idata.Entities.Core;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Core.Validators
{

    /// <summary>
    /// This class is intended to valdiate models, using .Net validation engine (MVC, API).
    /// </summary>
    public static class ValidatorBase
    {
        /// <summary>
        /// Function that validates any entity based on its data annotations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>List<string> containing all errors or null if no error is present</string></returns>
        public static List<string>? ValidateEntity<T>(this T obj, JObject? specificFields = null) where T : EntityBase
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj, null, null);

            if (!Validator.TryValidateObject(obj, context, results, true))
            {


                if (specificFields == null)
                {
                    //Return a list containing all errors found by .Net engine
                    var errorMessages = new List<string>();
                    foreach (var validationResult in results)
                    {
                        errorMessages.Add(validationResult.ErrorMessage);
                    }
                    return errorMessages;
                }
                else
                {
                    //Return a list containing all errors found by .Net engine but only those that were sent by front end (update mostly)

                    var errorMessages = new List<string>();

                    foreach (var validationResult in results)
                    {

                        var propertyName = TryGetMemberName(obj, validationResult);

                        if (specificFields.ContainsKey(propertyName))
                        {
                            errorMessages.Add(validationResult.ErrorMessage);
                        }

                    }
                    return errorMessages;
                }

            }

            // if no error present, return null
            return null;
        }



        private static string TryGetMemberName<T>(T obj, ValidationResult validationResult) where T : EntityBase
        {
            if(validationResult.MemberNames != null)
            {
              return validationResult.MemberNames.First();
            }
            else
            {
                //Get it hard way
                var allProperties = obj.getProperties().Select(pro => pro.Name).ToList();

                var message = validationResult.ErrorMessage.Replace("The field ", string.Empty).Split(" ").ToList();

                var propName = allProperties.Where(propN => message.Contains(propN)).FirstOrDefault();

                return propName; 
            }

            
        }
    }
}
