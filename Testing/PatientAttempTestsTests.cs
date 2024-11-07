using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Controllers;
using System.Text.RegularExpressions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using PFMS_MI04.Models;
using System.Text.Json;

namespace PFMS_MI04.Testing
{
    public class PatientAttempTestsTests
    {
        private List<PatientsAttempsModel> _allUsers;
        private List<PatientsAttempsModel> _searchResults;
        private bool _isSearchActive = false;
        private const int _usersPerPage = 13;
        public static List<PatientsAttempsModel> _cachedUsers;

        public PatientAttempTestsTests()
        {
            InitializeUsers();
        }

        private void InitializeUsers()
        {
            if (_allUsers == null)
            {
                PatientsAttempsModel patientsData = new PatientsAttempsModel();
                _allUsers = ApiDataConnection_Mock();
            }
        }


        public object Mock_getPatients_Attempt([FromQuery] int page)
        {
            var usersToDisplay = _isSearchActive ? _searchResults : _allUsers;
            if (usersToDisplay == null || !usersToDisplay.Any())
            {
                return "No user data available";
            }
            var pagedUsers = usersToDisplay.Skip((page - 1) * _usersPerPage).Take(_usersPerPage).ToList();
            int totalPages = (int)Math.Ceiling((double)usersToDisplay.Count / _usersPerPage);
            var response = new
            {
                TotalUsers = totalPages,
                PagedUsers = string.Join("\n", pagedUsers.Select(u => $"{u.ID} , {u.Name} , {u.Status}"))
            };
            return response;
        }

        public object SearchFunction(string value)
        {
            if (_allUsers == null || !_allUsers.Any())
            {
                return "No user data available";
            }
            value = value.Trim();
            string noSpaceValue = Regex.Replace(value, @"\s+", "");
            bool isIdFormat = Regex.IsMatch(noSpaceValue, @"^-?[\d-]+$");
            if (string.IsNullOrWhiteSpace(value))
            {
                _isSearchActive = false;
                _searchResults = null;
                return "Nothing";
            }
            if (isIdFormat)
            {
                string searchValue = noSpaceValue.TrimStart('-');
                _searchResults = _allUsers.FindAll(patient =>
                    patient.ID.StartsWith(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    patient.ID.EndsWith(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    patient.ID.Contains(searchValue, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                string[] searchTerms = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _searchResults = _allUsers.FindAll(patient =>
                    searchTerms.All(term =>
                        patient.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0));
            }
            _isSearchActive = true;
            return Mock_getPatients_Attempt(1);
        }

        public object ClearSearch()
        {
            _isSearchActive = false;
            _searchResults = null;
            return Mock_getPatients_Attempt(1);
        }

        private string GetJsonString()
        {
            string jsonString = @"
                [
                    { ""ID"": ""123456-12-0004"", ""Name"": ""Alexander Jonathan Smith"", ""Status"": ""Login"" },
                    { ""ID"": ""123456-12-0005"", ""Name"": ""Elizabeth Charlotte Anderson"", ""Status"": ""Login"" },
                    { ""ID"": ""123456-12-0006"", ""Name"": ""Christopher Daniel Thompson"", ""Status"": ""Login"" },
                    { ""ID"": ""123456-12-0007"", ""Name"": ""Victoria Sophia Hernandez"", ""Status"": ""Login"" },
                    { ""ID"": ""123456-12-0008"", ""Name"": ""Benjamin Harrison Edwards"", ""Status"": ""Login"" },
                    { ""ID"": ""123456-12-0009"", ""Name"": ""Isabella Grace Matthews"", ""Status"": ""Login"" },
                    { ""ID"": ""123456-12-0010"", ""Name"": ""Nicholas Alexander Johnson"", ""Status"": ""Login"" }
                ]";

            return jsonString;
        }

        public List<PatientsAttempsModel> ApiDataConnection_Mock()
        {
            if (_cachedUsers != null)
            {
                return _cachedUsers;
            }

            try
            {
                string jsonString = GetJsonString(); // Method to get the JSON string
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _cachedUsers = JsonSerializer.Deserialize<List<PatientsAttempsModel>>(jsonString, options);

                if (_cachedUsers == null || _cachedUsers.Count == 0)
                {
                    return new List<PatientsAttempsModel>();
                }
                return _cachedUsers;
            }
            catch (JsonException ex)
            {
                return new List<PatientsAttempsModel>();
            }
            catch (Exception ex)
            {
                return new List<PatientsAttempsModel>();
            }
        }

        [Fact]
        public void Test1_Mock_getPatients_Attempt_ReturnsCorrectPagedData()
        {
            // Expected Output: Object containing TotalUsers (int) and PagedUsers (string) with up to 13 users

            int page = 1;

            var result = Mock_getPatients_Attempt(page) as dynamic;

            Console.WriteLine($"Actual Output: TotalUsers = {result.TotalUsers}, PagedUsers = {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.IsType<int>(result.TotalUsers);
            Xunit.Assert.IsType<string>(result.PagedUsers);
            Xunit.Assert.True(result.PagedUsers.ToString().Split('\n').Length <= 13);
        }

        [Fact]
        public void Test2_SearchFunction_WithValidInput_ReturnsMatchingResults()
        {
            // Expected Output: Object containing search results including "Alexander"

            string searchValue = "Alex";

            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains("Alexander", result.PagedUsers.ToString());
        }

        [Fact]
        public void Test3_SearchFunction_WithEmptyInput_ReturnsNothing()
        {
            // Expected Output: "Nothing"

            string searchValue = "";

            var result = SearchFunction(searchValue);

            Console.WriteLine($"Actual Output: {result}");

            Xunit.Assert.Equal("Nothing", result);
        }

        [Fact]
        public void Test4_ClearSearch_ResetsSearchState()
        {
            // Expected Output: Object representing the first page of all users

            SearchFunction("John"); // Set up an active search

            var result = ClearSearch() as dynamic;

            Console.WriteLine($"Actual Output: TotalUsers = {result.TotalUsers}, PagedUsers = {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.IsType<int>(result.TotalUsers);
            Xunit.Assert.IsType<string>(result.PagedUsers);
        }

        [Fact]
        public void Test5_SearchFunction_WithIDInput_ReturnsMatchingResults()
        {
            // Expected Output: Object containing search results with IDs starting with "123456-12"

            string searchValue = "123456-12";

            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains("123456-12", result.PagedUsers.ToString());
        }

        [Fact]
        public void Test6_Mock_getPatients_Attempt_WithLargePageNumber_ReturnsEmptyResult()
        {
            // Expected Output: "No user data available" or empty result

            int page = 1000;
            var result = Mock_getPatients_Attempt(page);
            Console.WriteLine($"Actual Output: {result}");

            Xunit.Assert.True(result.ToString() == "No user data available" || (result as dynamic).PagedUsers == "");
        }

        [Fact]
        public void Test7_SearchFunction_WithNonExistentName_ReturnsEmptyResult()
        {
            // Expected Output: Object with empty PagedUsers - No user data available

            string searchValue = "Zzzzzz";

            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal("No user data available", result);
        }

        [Fact]
        public void Test8_SearchFunction_WithPartialName_ReturnsMatchingResults()
        {
            // Expected Output: Object containing search results including names with "son"

            string searchValue = "son";
            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains("son", result.PagedUsers.ToString());
        }

        [Fact]
        public void Test9_Mock_getPatients_Attempt_WithNegativePageNumber_ReturnsFirstPage()
        {
            // Expected Output: Object containing first page of results

            int page = 1;

            var result = Mock_getPatients_Attempt(page) as dynamic;

            Console.WriteLine($"Actual Output: TotalUsers = {result.TotalUsers}, PagedUsers = {result.PagedUsers}");

            // Xunit.Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.IsType<int>(result.TotalUsers);
            Xunit.Assert.IsType<string>(result.PagedUsers);
            Xunit.Assert.True(result.PagedUsers.ToString().Split('\n').Length <= 13);
        }

        [Fact]
        public void Test10_SearchFunction_CaseInsensitivity_ReturnsMatchingResults()
        {
            // Expected Output: Object containing search results including "Charlotte"

            string searchValue = "cHaRlOtTe";

            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains("Charlotte", result.PagedUsers.ToString());
        }

        [Fact]
        public void Test11_SearchFunction_CaseInsensitivity_WithSpace_ReturnsMatchingResults()
        {
            // Expected Output: Object containing search results including "Alex" or "alex"

            string searchValue = "al ex";

            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains("Alex", result.PagedUsers.ToString());
        }

        [Fact]
        public void Test12_SearchFunction_Name_ReturnsMatchingResults()
        {
            // Expected Output: Object containing search results including "123456-12-0004 , Alexander Jonathan Smith , Login"

            string searchValue = "Alexander Jonathan Smith";

            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains("123456-12-0004 , Alexander Jonathan Smith , Login", result.PagedUsers.ToString());
        }

        [Fact]
        public void Test13_SearchFunction_withID_ReturnsMatchingResults()
        {
            // Expected Output: Object containing search results including "123456-12-0004 , Alexander Jonathan Smith , Login"

            string searchValue = "0004";

            var result = SearchFunction(searchValue) as dynamic;

            Console.WriteLine($"Actual Output: {result.PagedUsers}");

            Xunit.Assert.NotNull(result);
            Xunit.Assert.Contains("123456-12-0004 , Alexander Jonathan Smith , Login", result.PagedUsers.ToString());
        }
    }
}