#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ReportBuilderAjax.Web.Attributes;
using ReportBuilderAjax.Web.Common;

#endregion

namespace ReportBuilderAjax.Web.HttpHandlers
{
    public class RFBaseTemplateHandler : RFBaseHandler
    {
        private string _templateId;

        /// <summary>
        /// Gets the "templateId" attribute name.
        /// </summary>
        /// <value>Attribute name.</value>
        private string TemplateId
        {
            get
            {
                if (string.IsNullOrEmpty(_templateId))
                {
                    string templateId = ConfigurationManager.AppSettings[Const.TEMLATE_ID];
                    if (string.IsNullOrEmpty(templateId))
                    {
                        templateId = Const.TEMLATE_ID;
                    }
                    _templateId = templateId;
                }
                return _templateId;
            }
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="fileName">Path to the template of file.</param>
        /// <returns>JSON object with requested template.</returns>
        [RFAjaxAccessible]
        public Dictionary<string, object> GetTemplate(string fileName)
        {
            return getTemplates(fileName);
        }

        /// <summary>
        /// Gets all templates.
        /// </summary>
        /// <returns>JSON object with all templates exists in "Templates" folder</returns>
        [RFAjaxAccessible]
        public Dictionary<string, object> GetAllTemplates()
        {
            return getTemplates(null);
        }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <param name="templateFileName">Name of the template file.</param>
        /// <returns></returns>
        private Dictionary<string, object> getTemplates(string templateFileName)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            Dictionary<string, object> templates = new Dictionary<string, object>();
            result.Add("Templates", templates);

            XmlNodeList nodes;

            if (string.IsNullOrEmpty(templateFileName))
            {
                nodes = getTemplateNodes(null);
            }
            else
            {
                nodes = getTemplateNodes(templateFileName);
            }

            foreach (XmlNode node in nodes)
            {
                templates.Add(node.Attributes[TemplateId].Value, node.OuterXml);
            }

            currentResponse.ContentType = "text/javascript";

            return result;
        }

        /// <summary>
        /// Gets the template nodes.
        /// </summary>
        /// <param name="templateFileName">Name of the template file.</param>
        /// <returns></returns>
        private XmlNodeList getTemplateNodes(string templateFileName)
        {
            if (currentRequest.PhysicalApplicationPath != null)
            {
                string fullTemplatePath = Path.Combine(Path.Combine(currentRequest.PhysicalApplicationPath, "Templates"));

                StringBuilder templatesBuilder = new StringBuilder("<root>");

                if (string.IsNullOrEmpty(templateFileName))
                {
                    string[] templateFiles = Directory.GetFiles(fullTemplatePath);

                    for (int i = 0; i < templateFiles.Length; i++)
                    {
                        string fileContent = prepareFileContent(Path.GetFileNameWithoutExtension(templateFiles[i]),
                                                                File.ReadAllText(templateFiles[i]));

                        templatesBuilder.Append(fileContent);
                    }
                }
                else
                {
                    string filePath = string.Format("{0}{1}", Path.Combine(fullTemplatePath, templateFileName),
                                                    Const.TEMPLATE_EXT);
                    if (File.Exists(filePath))
                    {
                        string fileContent = File.ReadAllText(filePath);
                        fileContent = prepareFileContent(templateFileName, fileContent);
                        templatesBuilder.Append(fileContent);
                    }
                }

                templatesBuilder.Append("</root>");

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(templatesBuilder.ToString());

                XmlNodeList nodes = doc.SelectNodes(string.Format("//*[@{0}]", TemplateId));

                if (null == nodes || nodes.Count == 0)
                {
                    throw new Exception("Nothing was found! Template is not correct");
                }
                return nodes;
            }
            throw new Exception("Physical Application Path is not defined!!!");
        }

        /// <summary>
        /// Prepares the content of the file.
        /// </summary>
        /// <param name="templateFileName">Name of the template file.</param>
        /// <param name="templateFileContent">Content of the template file.</param>
        /// <returns></returns>
        private string prepareFileContent(string templateFileName, string templateFileContent)
        {
            templateFileContent = templateFileContent.Replace("\t", "").Replace(Environment.NewLine, "");
            string searchedText = string.Format("{0}=\"([a-zA-Z0-9_]*)\"", TemplateId);
            string replacmentText = string.Format("{0}=\"{1}_$1\"", TemplateId, templateFileName);

            templateFileContent = Regex.Replace(templateFileContent, searchedText, replacmentText);
            return templateFileContent;
        }
    }
}