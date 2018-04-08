using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Face;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
using System.Collections;
using System.Web;
using System.Dynamic;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification;
using Newtonsoft.Json.Serialization;
using NAudio.Wave;


namespace WebApplication1.Controllers
{
    public class VoiceAPIController : ApiController
    {
        private string _selectedFile = "";

        const string SUCCESSFULL = "SUCCESSFULL";
        const string FAILURE = "FAILURE";
        const string USERCANNOTBEIDENTIFIED = "USER CANNOT BE IDENTIFIED";

        const string BINGSPEECHAPIKEY = "ffb6a06f528441b891be8f0538e67624";

        const string COGNITIVESPEECHAPIKEY = "05d22648c9544427aa99bc5419a6d79c";




        //    /api/VoiceAPI/RegisterUser
        [HttpGet]
        [ActionName("RegisterUser")]
        public async Task<string> RegisterUser(Stream userAudioStream)
        {
            #region TestBed
            if (userAudioStream == null)
            {
                string audioFilePath = @"C:\Users\Sachin13390\Desktop\Speech3.wav";
                userAudioStream = File.OpenRead(audioFilePath);
            }
            #endregion

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

            var identificationProfileURI = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles?" + queryString;
            HttpResponseMessage response;

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes("{\"locale\":\"en-us\",}");
            string profileId = string.Empty;
            string operationUrl = string.Empty;
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(identificationProfileURI, content);
                var profileIdResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                if (profileIdResponseData.Count() > 0)
                {
                    profileId = profileIdResponseData.First().Value.ToString();
                }
            }
            try
            {
                if (string.IsNullOrEmpty(profileId) == false)
                {
                    for (int enrolmentCount = 0; enrolmentCount < 3; enrolmentCount++)
                    {

                        client = new HttpClient();

                        // Request headers
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);
                        // Request parameters
                        queryString["shortAudio"] = "true";
                        var enrollmentProfileUri = string.Format("https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles/{0}/enroll?{1}", profileId, queryString);

                        var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString("u"));
                        Byte[] bytes;

                        Stream userAudioTemp = null;
                        userAudioTemp = userAudioStream;

                        using (MemoryStream userAudioStreamRecvd = new MemoryStream())
                        {
                            userAudioStream.Position = 0;
                            userAudioStream.CopyTo(userAudioStreamRecvd);
                            bytes = ConvertWavTo16000Hz16BitMonoWav(userAudioStreamRecvd.ToArray());
                        }
                        Stream audioStream = new MemoryStream(bytes);

                        content.Add(new StreamContent(audioStream), "Data", "testFile_" + DateTime.Now.ToString("u"));
                        response = await client.PostAsync(enrollmentProfileUri, content).ConfigureAwait(false);
                        if (response.StatusCode == HttpStatusCode.Accepted)
                        {
                            IEnumerable<string> operationLocation = response.Headers.GetValues("Operation-Location");
                            if (operationLocation.Count() == 1)
                            {
                                operationUrl = operationLocation.First();
                            }
                            else
                            {
                                return FAILURE;
                            }
                        }

                    }

                    /////////// 3 STEP operationUrl

                    if (string.IsNullOrEmpty(operationUrl) == false)
                    {
                        client = new HttpClient();

                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                        response = await client.GetAsync(operationUrl);
                        var operationStatusResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                        return operationStatusResponseData.First().Value.ToString();
                    }
                }
                
            }
            catch(Exception enrollmentException)
            {

            }
            return SUCCESSFULL;
        }

        //    /api/VoiceAPI/IndentifyUser
        [HttpGet]
        [ActionName("IndentifyUser")]
        public async Task<string> IndentifyUser(Stream userAudioStream)
        {
            try
            {
                // Get all Profiles

                string allProfileID = "";
                string matchedProfileID = "0be37623-ad65-45dc-ad42-8dd5b461d3c7";
                string operationUrl = string.Empty;

                if (userAudioStream == null)
                {
                    string audioFilePath = @"C:\Users\Sachin13390\Desktop\Speech2.wav";
                    userAudioStream = File.OpenRead(audioFilePath);
                }

                // Getting All Profile IDS

                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                var enrollmentVerifyProfileUri = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles?" + queryString;

                var response = await client.GetAsync(enrollmentVerifyProfileUri);

                var profileIdResponseData = await response.Content.ReadAsAsync<List<Object>>();

                foreach (var foundProfileID in profileIdResponseData)
                {
                    allProfileID = allProfileID + "," + (foundProfileID as Newtonsoft.Json.Linq.JObject)["identificationProfileId"].ToString();
                }

                allProfileID = allProfileID.Substring(1);



                client = new HttpClient();
                queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                // Request parameters
                queryString["shortAudio"] = "true";
                enrollmentVerifyProfileUri = string.Format("https://westus.api.cognitive.microsoft.com/spid/v1.0/identify?identificationProfileIds={0}&{1}", allProfileID, queryString);

                var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString("u"));

                Byte[] bytes;
                using (MemoryStream userAudioStreamRecvd = new MemoryStream())
                {
                    userAudioStream.CopyTo(userAudioStreamRecvd);
                    bytes = ConvertWavTo16000Hz16BitMonoWav(userAudioStreamRecvd.ToArray());
                }
                Stream audioStream = new MemoryStream(bytes);

                content.Add(new StreamContent(audioStream), "Data", "testFile_" + DateTime.Now.ToString("u"));
                response = await client.PostAsync(enrollmentVerifyProfileUri, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    IEnumerable<string> operationLocation = response.Headers.GetValues("Operation-Location");
                    if (operationLocation.Count() == 1)
                    {
                        operationUrl = operationLocation.First();
                    }
                    else
                    {
                        return FAILURE;
                    }
                }

                if (string.IsNullOrEmpty(operationUrl) == false)
                {
                    client = new HttpClient();

                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COGNITIVESPEECHAPIKEY);

                    response = await client.GetAsync(operationUrl);
                    var operationStatusResponseData = await response.Content.ReadAsAsync<ExpandoObject>();
                    return operationStatusResponseData.First().Value.ToString();
                }

                return SUCCESSFULL;
            }
            catch (Exception exception)
            {
                return FAILURE;
            }
        }

        //    /api/VoiceAPI/UserSpeechToText
        [HttpGet]
        [ActionName("UserSpeechToText")]
        public string SpeechToText(Stream userAudio)
        {
            string speechInText = string.Empty;

            string requestURI = "https://speech.platform.bing.com/speech/recognition/interactive/cognitiveservices/v1?language=en-US&format=detailed";
            HttpWebRequest request = null;
            request = (HttpWebRequest)HttpWebRequest.Create(requestURI);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = BINGSPEECHAPIKEY;

            // Send an audio file by 1024 byte chunks
            using (FileStream fs = new FileStream(@"C:\Users\Sachin\Desktop\Recording.wav", FileMode.Open, FileAccess.Read))
            {

                /*
                * Open a request stream and write 1024 byte chunks in the stream one at a time.
                */
                byte[] buffer = null;
                int bytesRead = 0;
                using (Stream requestStream = request.GetRequestStream())
                {
                    /*
                    * Read 1024 raw bytes from the input audio file.
                    */
                    buffer = new Byte[checked((uint)Math.Min(1024, (int)fs.Length))];
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }

                    // Flush
                    requestStream.Flush();
                }
            }
            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    speechInText = sr.ReadToEnd();
                }
            }
            return speechInText;
        }

        public static byte[] ConvertWavTo16000Hz16BitMonoWav(byte[] inArray)
        {
            //WaveFileWriter w = null;
            //try
            {
                using (var mem = new MemoryStream(inArray))
                {
                    using (var reader = new WaveFileReader(mem))
                    {
                        using (var converter = WaveFormatConversionStream.CreatePcmStream(reader))
                        {
                            using (var upsampler = new WaveFormatConversionStream(new WaveFormat(16000, 16, 1), converter))
                            {
                                byte[] data;
                                using (var m = new MemoryStream())
                                {
                                    upsampler.CopyTo(m);
                                    data = m.ToArray();
                                }
                                using (var m = new MemoryStream())
                                {
                                    // to create a propper WAV header (44 bytes), which begins with RIFF 
                                    var w = new WaveFileWriter(m, upsampler.WaveFormat);
                                    // append WAV data body
                                    w.Write(data, 0, data.Length);
                                    w.Dispose();
                                    return m.ToArray();

                                }
                            }
                        }
                    }
                }
            }
            //catch(Exception conversionException)
            //{
            //    return null;
            //}
            //finally
            //{
            //    w.Dispose();
            //}
        }

    }
}
