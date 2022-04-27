using advanced.models;
using advancedLevel;
using advancedLevel.Models;
using Aspose.Words;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace advanced.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranslatorController : ControllerBase
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly IOptions<Settings> _appSettings;
        public TranslatorController(IOptions<Settings> appSettings)
        {
            _appSettings = appSettings;
        }

        [HttpPost]
        public ResponseApi Post()
        {
            ResponseApi response = new ResponseApi();
            try
            {
                var reqBody = HttpContext;
                if (reqBody != null && reqBody.Request != null &&
                    reqBody.Request.Form != null &&
                    reqBody.Request.Form.Files != null && reqBody.Request.Form.Files.Count > 0
                    )
                {
                    string fromLanguage = "";
                    string toLanguage = "";


                    if (reqBody.Request.Headers != null && reqBody.Request.Headers.Count > 0 && reqBody.Request.Headers.ContainsKey("fromLanguage") && reqBody.Request.Headers.ContainsKey("toLanguage"))
                    {
                        fromLanguage = reqBody.Request.Headers["fromLanguage"];
                        toLanguage = reqBody.Request.Headers["toLanguage"];


                        if (reqBody.Request.Form.Files.Where(a => (a.FileName.Contains(".docx") || a.FileName.Contains(".doc"))).ToList().Count > 0)
                        {
                            response.code = "0";
                            response.comment = "ok";

                            List<files> files = new List<files>();
                            files file;
                            Document doc;
                            foreach (var i in reqBody.Request.Form.Files.Where(a => (a.FileName.Contains(".docx") || a.FileName.Contains(".doc"))))
                            {
                                //читаю файл из потока и записываю в document(aspose)
                                using (Stream stream = i.OpenReadStream())
                                {
                                    doc = new Document(stream);
                                }

                                file = new files();
                                file.fileName = i.FileName;

                                FileParser fp = new FileParser(_appSettings);
                                MethodResult mr = fp.parseFile(doc);
                                file.resultParse = mr.message;

                                if (mr.code == 0)
                                {
                                    Translator translator = new Translator(_appSettings);
                                    //временно, пока не будет подписки
                                    mr.text = mr.text.Replace("Created with an evaluation copy of Aspose.Words. To discover the full versions of our APIs please visit: https://products.aspose.com/words/\r\n\n", "");
                                    MethodResult resultTrans = translator.translateText(mr.text, fromLanguage, toLanguage);

                                    file.resultTranslate = resultTrans.message;

                                    if (resultTrans != null && resultTrans.code != null && resultTrans.code == 0)
                                    {
                                        file.translatedText = resultTrans.text;
                                    }
                                }
                                files.Add(file);
                            }
                            response.files = new List<files>();
                            response.files = files;
                        }
                        else
                        {
                            response = new ResponseApi();
                            response.code = "1";
                            response.comment = "Not found word files!";
                            response.files = null;
                        }
                    }
                    else
                    {
                        response = new ResponseApi();
                        response.code = "1";
                        response.comment = "required headers not passed!";
                        response.files = null;
                    }
                }
                else
                {
                    response = new ResponseApi();
                    response.code = "1";
                    response.comment = "Not found files!";
                    response.files = null;
                }
            }
            catch (Exception ex)
            {
                response = new ResponseApi();
                response.code = "-1";
                response.comment = "try again please, error: " + ex.Message;
                response.files = null;
            }
            return response;
        }
    }
}
