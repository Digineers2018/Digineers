using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebApplication1.Controllers
{
    public class TableStorageController : ApiController
    {
        const string TABLECONNECTIONSTRING = "DefaultEndpointsProtocol=https;AccountName=stoarageaccount;AccountKey=586DxL4HL0ayGYspCRCRAEizJTLgm1z7t9wlBcBVxzvwXQ5aJ5SGzk9sQbjZ7HdetuY2lN9GRJbeVawSdOSg0Q==;EndpointSuffix=core.windows.net";


        CloudStorageAccount cloudStorageAccount = null;
        CloudTableClient cloudTableClient = null;
        CloudTable cloudTable = null;
        const string AZURETABLEREFRENCE = "person";
        bool IsParameterInitialize = false;


        public void InitializeParameters()
        {
            cloudStorageAccount = CloudStorageAccount.Parse(TABLECONNECTIONSTRING);
            cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            cloudTable = cloudTableClient.GetTableReference(AZURETABLEREFRENCE);
            cloudTable.CreateIfNotExists();
            IsParameterInitialize = true;
        }

        //    /api/VoiceAPI/RegisterUser
        [HttpGet]
        [ActionName("RegisterUser")]
        public void RegisterUser()
        {
            CloudStorageAccount csa = CloudStorageAccount.Parse(TABLECONNECTIONSTRING);

            CloudTableClient tableClient = csa.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("person");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
        }

        public void InsertUSerInAzureTable(string FaceProfileID, string AudioProfileID, string UserName)
        {
            if (IsParameterInitialize == false)
            {
                InitializeParameters();
            }

            PersonEntity personEntity = new PersonEntity(FaceProfileID, AudioProfileID);
            personEntity.UserName = UserName;

            TableOperation insertOperation = TableOperation.Insert(personEntity);
            cloudTable.Execute(insertOperation);
        }


        public string GetUserAudioProfileID(string FaceProfileID)
        {
            if (IsParameterInitialize == false)
            {
                InitializeParameters();
            }

            TableQuery<PersonEntity> query = new TableQuery<PersonEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, FaceProfileID));

            // Print the fields for each customer.
            foreach (PersonEntity personEntity in cloudTable.ExecuteQuery(query))
            {
                return personEntity.RowKey; // Row Key in Person is Audio Profile
            }
            return null;
        }

        public void DeletePersonTable(string FaceProfileID)
        {
            if (IsParameterInitialize == false)
            {
                InitializeParameters();
            }
            cloudTable.DeleteIfExists();
        }

    }



    public class PersonEntity : TableEntity
    {
        public PersonEntity(string FaceProfileID, string AudioProfileID)
        {
            this.PartitionKey = FaceProfileID;
            this.RowKey = FaceProfileID;
        }

        public PersonEntity() { }

        public string UserName { get; set; }

    }
}
