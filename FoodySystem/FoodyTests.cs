using Foody.Tests.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Foody
{
    [TestFixture]
    public class FoodyTests
    {
        private RestClient client;
        private static string lastCreatedFoodId;

        private const string BaseUrl = "http://144.91.123.158:81";
        private const string Username = "smdsg";
        private const string Password = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(Username, Password);

            RestClientOptions options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            RestClient tempClient = new RestClient(BaseUrl);
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName = username, password });

            RestResponse response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }

                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Test]
        [Order(1)]
        public void CreateFood_WithRequiredFields_ShouldReturnCreated()
        {
            var foodData = new FoodDTO
            {
                Name = "Test Food",
                Description = "This is a test food description.",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodData);

            var response = client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Expected status code 201 Created.");
            Assert.That(createResponse, Is.Not.Null);
            Assert.That(createResponse.FoodId, Is.Not.Null.And.Not.Empty);

            lastCreatedFoodId = createResponse.FoodId;
        }

        [Test]
        [Order(2)]
        public void EditFoodTitle_ShouldReturnSuccess()
        {
            var patchData = new List<PatchFoodDTO>
            {
                new PatchFoodDTO
                {
                    Path = "/name",
                    Op = "replace",
                    Value = "Edited Food"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{lastCreatedFoodId}", Method.Patch);
            request.AddJsonBody(patchData);

            var response = client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse, Is.Not.Null);
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test]
        [Order(3)]
        public void GetAllFoods_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);

            var response = client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);
        }

        [Test]
        [Order(4)]
        public void DeleteFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Food/Delete/{lastCreatedFoodId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test]
        [Order(5)]
        public void CreateFood_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var invalidFood = new FoodDTO
            {
                Name = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(invalidFood);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Test]
        [Order(6)]
        public void EditNonExistingFood_ShouldReturnNotFound()
        {
            string nonExistingFoodId = "999999";

            var patchData = new List<PatchFoodDTO>
            {
                new PatchFoodDTO
                {
                    Path = "/name",
                    Op = "replace",
                    Value = "Edited Invalid Food"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{nonExistingFoodId}", Method.Patch);
            request.AddJsonBody(patchData);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Expected status code 404 Not Found.");
            Assert.That(response.Content, Does.Contain("No food revues..."));
        }

        [Test]
        [Order(7)]
        public void DeleteNonExistingFood_ShouldReturnBadRequest()
        {
            string nonExistingFoodId = "123112";

            var request = new RestRequest($"/api/Food/Delete/{nonExistingFoodId}", Method.Delete);

            var response = client.Execute(request);

            var errorResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(errorResponse, Is.Not.Null);
            Assert.That(errorResponse.Msg, Is.EqualTo("Unable to delete this food revue!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}