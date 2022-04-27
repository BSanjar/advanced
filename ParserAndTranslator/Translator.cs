using advanced.models;
using advancedLevel.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace advancedLevel
{
    public class Translator
    {
        private readonly IOptions<Settings> _appSettings;
        public Translator(IOptions<Settings> appSettings)
        {
            _appSettings = appSettings;
        }
        //сделал отдельный метод, потому что может несколько функций вызываться(проверка текста, проверка языка итд). 
        public MethodResult translateText(string text, string ln1, string ln2)
        {
            MethodResult mr = new MethodResult();
            try
            {
                //проверка текста на корректность, закоментировал потому что API чуть странно работает))
                //MethodResult checkResult = checkText(text, ln1);
                MethodResult translateResult = translate(text, ln1, ln2);

                return translateResult;
            }
            catch (Exception ex)
            {
                mr.code = -1;
                mr.message = "can`t translate text, error: " + ex.Message;
                mr.text = null;
            }

            return mr;
        }


        /// <summary>
        /// валдидация текста перед проверкой
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fromText"></param>
        /// <returns></returns>
        public MethodResult checkText(string text, string fromText)
        {
            MethodResult mr = new MethodResult();
            try
            {
                string addr = _appSettings.Value.urlCheckTranslate;
                text = "q=" + text;
                string response = SendRequest(text, addr);


                mr.text = response;
                mr.message = "success";
                mr.code = 0;
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));
            }
            return mr;
        }

        /// <summary>
        /// метод который возвращает переведенный текст
        /// </summary>
        /// <param name="text">текст для перевода</param>
        /// <param name="fromText">язык текста для перевода</param>
        /// <param name="toText">язык переведенного текста</param>
        /// <returns></returns>
        public MethodResult translate(string text, string fromText, string toText)
        {
            try
            {
                MethodResult mr = new MethodResult();
                var content = "q=" + text + "&target=" + fromText + "&source=" + toText;
                var res = SendRequest(content, _appSettings.Value.urlTranslate);
                TransatorApiResult jsonResult = new TransatorApiResult();
                //parse json
                jsonResult = JsonConvert.DeserializeObject<TransatorApiResult>(res);
                if (jsonResult != null)
                {
                    mr.text = jsonResult.data.translations[0].translatedText;
                    mr.code = 0;
                    mr.message = "success";
                }
                else
                {
                    mr.text = "";
                    mr.code = 1;
                    mr.message = "object is null";
                }
                return mr;
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));
            }
        }

        /// <summary>
        /// запрос к сервису переводчика
        /// </summary>
        /// <param name="body">тело запроса</param>
        /// <param name="addr">адресс запроса</param>
        /// <returns></returns>
        private string SendRequest(string body, string addr)
        {
            string descript = "";
            string status = "info";
            string resp = "";
            try
            {

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                addr = addr.Trim('/').Trim('\\'); // в конце адреса удалить слэш, если он имеется
                ServicePointManager.ServerCertificateValidationCallback = ((senderr, certificate, chain, sslPolicyErrors) => true);
                WebRequest _request = HttpWebRequest.Create(addr);

                //метод POST/GET и.т.д
                _request.Method = "POST";
                _request.ContentType = "application/x-www-form-urlencoded";
                //_request.ContentLength = body.Length;

                //добавляю загаловки сервиса

                _request.Headers.Add("Accept-Encoding", "application/gzip");
                _request.Headers.Add("X-RapidAPI-Host", "google-translate1.p.rapidapi.com");
                _request.Headers.Add("X-RapidAPI-Key", _appSettings.Value.XRapidAPIKey);


                // пишу тело
                StreamWriter _streamWriter = new StreamWriter(_request.GetRequestStream());
                _streamWriter.Write(body);
                _streamWriter.Close();
                // читаем тело
                WebResponse _response = _request.GetResponse();

                StreamReader _streamReader = new StreamReader(_response.GetResponseStream());
                resp = _streamReader.ReadToEnd();

            }
            catch (Exception ex)
            {
                throw new Exception("can`t send request to API translator, error: " + ex.Message);
            }

            return resp;
        }
    }
}
