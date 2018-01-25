using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using ReportBuilderAjax.Web.Attributes;
using ReportBuilderAjax.Web.Common;
using ReportBuilderAjax.Web.Exceptions;
using Newtonsoft.Json;

namespace ReportBuilderAjax.Web.HttpHandlers
{
    /// <summary>
    /// Base handler for CCNSIPrototypeHandler
    /// </summary>
    public class RFBaseHandler : IHttpHandler, IRequiresSessionState, IIntegrationInfo
    {
        private const string ACTION = "action";

        protected HttpRequest currentRequest;
        protected HttpResponse currentResponse;
        protected HttpSessionState currentSession;

        protected JavaScriptObject Params { get; set; }

        public UserContext UserContext
        {
            get;
            set;
        }

        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                Init(context);
                invokeAction();
            }
            catch(Exception e)
            {
                Logger.Write(e, MethodBase.GetCurrentMethod(), "Error getting data from server.", UserContext.UserID);
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        #endregion

        /// <summary>
        /// Invokes the action.
        /// </summary>
        private void invokeAction()
        {
            string actionName = null;
            MethodInfo method = null;
            object[] actualParams = null;
            if (!Params.ContainsKey(ACTION))
            {
                throw new RFParameterAbsentException(ACTION);
            }
            actionName = Params[ACTION].ToString();
            if (string.IsNullOrEmpty(actionName))
            {
                throw new RFActionNameAbsentException();
            }

            method = GetType().GetMethod(actionName,
                                         BindingFlags.NonPublic | BindingFlags.Instance |
                                         BindingFlags.Public |
                                         BindingFlags.Static);
            if (null == method)
            {
                throw new RFInvalidMethodNameException(actionName);
            }

            object[] attributes = null;

            attributes =
                method.GetCustomAttributes(typeof (RFAjaxAccessibleAttribute), false) as RFAjaxAccessibleAttribute[];
            if (attributes == null || attributes.Length == 0)
            {
                throw new RFNotAjaxAccessibleException(actionName);
            }

            ParameterInfo[] parameters = method.GetParameters();

            /// validate parameters if exists
            if (parameters.Length > 0)
            {
                actualParams = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type parameterType = parameters[i].ParameterType;

                    object currentParam;

                    if (!Params.TryGetValue(parameters[i].Name, out currentParam))
                    {
                        throw new RFParameterAbsentException(parameters[i].Name);
                    }

                    actualParams[i] = castParameter(currentParam, parameterType);
                }
            }

            string acceptEncoding = currentRequest.Headers["Accept-Encoding"];
            Stream prevUncompressedStream = currentResponse.Filter;

            if (!string.IsNullOrEmpty(acceptEncoding))
            {
                acceptEncoding = acceptEncoding.ToLower();

                if (acceptEncoding.Contains("gzip"))
                {
                    // gzip
                    currentResponse.Filter = new GZipStream(prevUncompressedStream, CompressionMode.Compress);
                    currentResponse.AppendHeader("Content-Encoding", "gzip");
                }
                else if (acceptEncoding.Contains("deflate"))
                {
                    // deflate
                    currentResponse.Filter = new DeflateStream(prevUncompressedStream, CompressionMode.Compress);
                    currentResponse.AppendHeader("Content-Encoding", "deflate");
                }
            }
            object result = null;
            try
            {
                result = method.Invoke(this, actualParams);
            }
            catch (Exception e)
            {
                string messageString = string.Format("Error execute method: \"{0}\" with parameters: \r\n", actionName);
                foreach (KeyValuePair<string, object> keyValuePair in Params)
                {
                    if (keyValuePair.Key.ToLower() != ACTION.ToLower())
                    {
                        messageString += string.Format("\tName: {0} - Value: {1}\r\n", keyValuePair.Key,
                                                       keyValuePair.Value);
                    }
                }
                Logger.Write(e, MethodBase.GetCurrentMethod(), messageString, UserContext.UserID);
            }
             
            currentResponse.Write(JavaScriptConvert.SerializeObject(result));
        }

        /// <summary>
        /// Casts the parameter.
        /// </summary>
        /// <param name="currentParam">The current param.</param>
        /// <param name="parameterType">Type of the parameter.</param>
        /// <returns></returns>
        private static object castParameter(object currentParam, Type parameterType)
        {
            object result;
            if (currentParam is JavaScriptArray)
            {
                JavaScriptArray javaScriptArray = currentParam as JavaScriptArray;
                object[] array = new object[javaScriptArray.Count];

                for(int i = 0; i < javaScriptArray.Count; i++)
                {
                    array[i] = javaScriptArray[i];
                }

                result = array;
            }
            else if (currentParam is JavaScriptObject)
            {
                JavaScriptObject javaScriptObject = currentParam as JavaScriptObject;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> item in javaScriptObject)
                {
                    dictionary.Add(item.Key, item.Value);
                }

                result = dictionary;
            }
            else
            {
                try
                {
                    result = Convert.ChangeType(currentParam, parameterType);
                }
                catch(InvalidCastException e)
                {
                    throw new RFInvalidParameterTypeException(e);
                }
            }

            return result;
        }

        /// <summary>
        /// Inits handler
        /// </summary>
        /// <param name="context">The context.</param>
        protected virtual void Init(HttpContext context)
        {
            currentRequest = context.Request;
            currentResponse = context.Response;
            currentSession = context.Session;

            string hackParams = currentRequest[Const.RP_PARAMS_ARRAY].Replace("'null'", "''").Replace("\"null\"", "\"\"").Replace("\\\"", "\"");
            if (hackParams == "SilverlightObject")
            {
                Params = (deserializeInputStream(currentRequest.InputStream) as JavaScriptObject)[Const.RP_PARAMS_ARRAY] as JavaScriptObject;
            }
            else
            {
                Params = JavaScriptConvert.DeserializeObject(hackParams) as JavaScriptObject;
            }
        }

        private static object deserializeInputStream(Stream inputStream)
        {
            string data;
            using (StreamReader reader = new StreamReader(inputStream))
            {
                data = Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
            }

            return JavaScriptConvert.DeserializeObject(data);
        }
    }
}