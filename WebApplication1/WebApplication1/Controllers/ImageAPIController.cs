using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Drawing;
using System.Web.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Face;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Face.Contract;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class ImageAPIController : ApiController
    {
        const string IMAGE_SUBSCRIPTION_KEY = "d51fdfb00e354267bd91d120d92b2f70";
        const string IMAGE_OCP_AICP_SUBSCRIPTION_KEY = "Ocp-Apim-Subscription-Key";

        const string SUCCESSFULL = "SUCCESSFULL";
        const string FAILURE = "FAILURE";
        const string USERCANNOTBEIDENTIFIED = "USER CANNOT BE IDENTIFIED";

        public Bitmap input_Face_Image;
        string image_File_Path = null;

        string personGroupId = "12111993133902018";
        string groupName = "Mastek_Digineers";

        const string FACE_API_ENDPOINT = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(IMAGE_SUBSCRIPTION_KEY, FACE_API_ENDPOINT);


        string[] file_Paths;
        string[] directories;


        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        private async Task<Face[]> UploadAndDetectFaces(Stream imageFileStream)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                Face[] faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                return faces;
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                //MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Error");
                return new Face[0];
            }
        }

        private void CreateUserGroup()
        {
            // To use only once

            //try
            //{
            //    await faceServiceClient.DeletePersonGroupAsync(personGroupId);
            //}
            //catch (Exception e)
            //{
            //    string exception = e.ToString();
            //}

            //await faceServiceClient.CreatePersonGroupAsync(personGroupId, groupName);

        }

        private Stream GetImageStream(string imageFilePath)
        {
            Stream imageFileStream = File.OpenRead(imageFilePath);

            return imageFileStream;
        }


        //    /api/ImageAPI/RegisterUser
        [HttpGet]
        [ActionName("RegisterUser")]
        public async Task<string> RegisterUser(List<Stream> listUserImages, string personName = "")
        {
            try
            {
                #region TestBed
                //if (listUserImages == null)
                //{
                //    listUserImages = new List<Stream>();

                //    string directoryPath = @"C:\Users\Sachin13390\Desktop\Face_Data";

                //    string[] allDirectory = Directory.GetDirectories(directoryPath);

                //    foreach (string dir in allDirectory)
                //    {
                //        personName = Path.GetFileName(dir);
                //        string[] file_Paths = Directory.GetFiles(dir);
                //        foreach (string filePath in file_Paths)
                //        {
                //            listUserImages.Add(GetImageStream(filePath));
                //        }
                //    }
                //}
                #endregion

                CreatePersonResult person = await faceServiceClient.CreatePersonInPersonGroupAsync(personGroupId, personName);

                foreach (Stream imageStream in listUserImages)
                {
                    await faceServiceClient.AddPersonFaceInPersonGroupAsync(personGroupId, person.PersonId, imageStream);
                }


                await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                TrainingStatus trainingStatus = null;
                while (true)
                {
                    trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                    if (trainingStatus.Status != Status.Running)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }

                return SUCCESSFULL;
            }
            catch (Exception exception)
            {
                return FAILURE;
            }
        }

        //    /api/ImageAPI/IdentifyUser
        [HttpGet]
        [ActionName("IdentifyUser")]
        public async Task<string> IdentifyUser(Stream imageStream)
        {
            #region TestBed
            //string imageFilePath = @"C:\Users\Sachin13390\Desktop\images.jpg";
            //imageStream = GetImageStream(imageFilePath);
            #endregion

            try
            {
                Face[] faces = await UploadAndDetectFaces(imageStream);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                foreach (var identifyResult in await faceServiceClient.IdentifyAsync(personGroupId, faceIds))
                {
                    if (identifyResult.Candidates.Length != 0)
                    {
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceServiceClient.GetPersonInPersonGroupAsync(personGroupId, candidateId);
                        return person.Name;
                    }
                }
                return USERCANNOTBEIDENTIFIED;
            }
            catch (Exception exception)
            {
                return FAILURE;
            }
        }

    }
}
