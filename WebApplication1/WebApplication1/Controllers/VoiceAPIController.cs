﻿using System;
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



        //    /api/VoiceAPI/RegisterUser
        [HttpGet]
        [ActionName("RegisterUser")]
        public async Task<string> RegisterUser(List<Stream> listUserImages, string personName = "")
        {
            byte[] bytes = File.ReadAllBytes(@"C:\Users\Sachin13390\Desktop\VoiceSamples\amy.wav");

            //BitArray bits = new BitArray(bytes);
            //StringBuilder audioBinary = new StringBuilder();
            //foreach(var audioBit in bits)
            //{
            //    if(audioBit.ToString().Equals("True"))
            //    {
            //        audioBinary.Append("1");
            //    }
            //    else
            //    {
            //        audioBinary.Append("0");
            //    }
                
            //}

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "05d22648c9544427aa99bc5419a6d79c");

            var identificationProfileURI = "https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles?" + queryString;
            
            
            

            HttpResponseMessage response;

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes("{\"locale\":\"en-us\",}");
            string profileId = string.Empty;
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(identificationProfileURI, content);
                var data = await response.Content.ReadAsAsync<ExpandoObject>();
                if (data.Count() > 0)
                {
                    profileId = data.First().Value.ToString();
                }
            }
            try
            {
                if (string.IsNullOrEmpty(profileId) == false)
                {
                    client = new HttpClient();

                    // Request headers
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "05d22648c9544427aa99bc5419a6d79c");
                    // Request parameters
                    queryString["shortAudio"] = "true";
                    var enrollmentProfileUri = string.Format("https://westus.api.cognitive.microsoft.com/spid/v1.0/identificationProfiles/{0}/enroll?{1}", profileId, queryString);
                    //using (var content = new ByteArrayContent(bytes))
                    //{
                    //    content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav;samplerate=1600");
                    //    response = await client.PostAsync(enrollmentProfileUri, content);
                    //    var data = await response.Content.ReadAsAsync<ExpandoObject>();
                    //    //if (data.Count() > 0)
                    //    //{
                    //    //    profileId = data.First().Value.ToString();
                    //    //}
                    //}

                    var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString("u"));
                    Stream audioStream = File.OpenRead(@"C:\Users\Sachin13390\Desktop\VoiceSamples\amy.wav");
                    content.Add(new StreamContent(audioStream), "Data", "testFile_" + DateTime.Now.ToString("u"));
                    response = await client.PostAsync(enrollmentProfileUri, content).ConfigureAwait(false);
                    var data = await response.Content.ReadAsAsync<ExpandoObject>();
                }
            }
            catch(Exception enrollmentException)
            {

            }
            return SUCCESSFULL;
        }


        public static byte[] ConvertWavTo8000Hz16BitMonoWav(byte[] inArray)
        {
            using (var mem = new MemoryStream(inArray))
            {
                using (var reader = new WaveFileReader(mem))
                {
                    using (var converter = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        using (var upsampler = new WaveFormatConversionStream(new WaveFormat(8000, 16, 1), converter))
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
                                return m.ToArray();
                            }
                        }
                    }
                }
            }
        }

    }
}
