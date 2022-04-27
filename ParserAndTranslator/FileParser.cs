using advanced.models;
using advancedLevel.Models;
using Aspose.Words;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace advancedLevel
{
    /// <summary>
    /// Класс для парсинга файла
    /// </summary>
    public class FileParser
    {
        private readonly IOptions<Settings> _appSettings;

        public FileParser(IOptions<Settings> appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Метод для парсинга файла, метод парсит word файл и возвращает текст с header\footer или footnode или первый параграф каждой секции в документе, 
        /// </summary>
        /// <param name="doc">документ</param>
        /// <returns></returns>
        public MethodResult parseFile(Document doc)
        {
            MethodResult result;
            try
            {
                List<Node> hfList = new List<Node>();
                List<Node> footNoteList = new List<Node>();
                List<Node> firstPharagaraphs = new List<Node>();

                //все сноски в документе
                footNoteList.AddRange(doc.GetChildNodes(NodeType.Footnote, true).ToList());

                foreach (Section section in doc.Sections)
                {
                    //Все колонтитулы в секции
                    hfList.AddRange(section.HeadersFooters.ToList());

                    //Первый параграф каждой секции
                    firstPharagaraphs.Add(section.GetChildNodes(NodeType.Paragraph, true).First());
                }

                result = new MethodResult();
                if (hfList != null && hfList.Count() > 0)
                {
                    result.code = 0;
                    result.message = "document contains headers and footers!";
                    result.text = formatterText(hfList);
                }
                else
                {
                    if (footNoteList != null && footNoteList.Count() > 0)
                    {
                        result.code = 0;
                        result.message = "document doesn't contain headers and footers, but contain footnodes!";
                        result.text = formatterText(footNoteList);
                    }
                    else
                    {
                        if (firstPharagaraphs != null && firstPharagaraphs.Count() > 0)
                        {
                            result.code = 0;
                            result.message = "document doesn't contain headers/footers and footnodes, but contain pharagaraphs!";
                            result.text = formatterText(firstPharagaraphs);
                        }
                        else
                        {
                            result.code = 1;
                            result.message = "document doesn't contain headers/footers, footnodes and also pharagaraphs!";
                            result.text = null;
                        }
                    }
                }

                return result;

            }
            catch (Exception ex)
            {
                return new MethodResult() { code = -1, message = "can`t parse file, error: " + ex.Message, text = null };
            }
        }

        public string formatterText(List<Node> nodes)
        {
            try
            {
                string text = "";
                foreach (var node in nodes)
                {
                    text += node.GetText() + "\n\n";
                }
                return text;
            }
            catch (Exception ex)
            {
                throw (new Exception("unable to read text: " + ex.Message));
            }
        }

    }
}
